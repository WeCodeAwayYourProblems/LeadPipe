using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Linq;

namespace Analyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class UnusedMemberAnalyzer : DiagnosticAnalyzer
    {
        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: "FT0006",
            title: "Unused member",
            messageFormat: "'{0}' is declared but never referenced",
            category: "Usage",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Members that are never referenced are dead code and should be removed.",
            customTags: new string[] { WellKnownDiagnosticTags.CompilationEnd, WellKnownDiagnosticTags.Unnecessary });

        // The accessibility levels this analyzer currently targets.
        // To expand coverage, add Accessibility.Internal, Accessibility.Public, etc.
        private static readonly ImmutableHashSet<Accessibility> TargetAccessibilities =
            ImmutableHashSet.Create(
                Accessibility.Private
            //,
            // Accessibility.Internal   // add when ready
            // ,   
            // Accessibility.Public     // add when ready
            // ,
            // Accessibility.Protected  // add when ready
            );

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterCompilationStartAction(compilationContext =>
            {
                // Thread-safe set of declared members we want to check
                var declared = new ConcurrentDictionary<ISymbol, bool>(SymbolEqualityComparer.Default);

                // Collect semantic models as Roslyn computes them rather than requesting
                // them ourselves, which would bypass caching and violate RS1030
                var semanticModels = new ConcurrentBag<SemanticModel>();

                compilationContext.RegisterSymbolAction(
                    symbolContext => CollectDeclaredMembers(symbolContext, declared),
                    SymbolKind.Method,
                    SymbolKind.Field,
                    SymbolKind.Property);

                compilationContext.RegisterSemanticModelAction(
                    semanticModelContext => semanticModels.Add(semanticModelContext.SemanticModel));

                compilationContext.RegisterCompilationEndAction(
                    endContext => ReportUnreferenced(endContext, declared, semanticModels));
            });
        }

        private static void CollectDeclaredMembers(
            SymbolAnalysisContext context,
            ConcurrentDictionary<ISymbol, bool> declared)
        {
            var symbol = context.Symbol;

            if (!TargetAccessibilities.Contains(symbol.DeclaredAccessibility))
                return;

            // Skip compiler-generated members (e.g. auto-property backing fields)
            if (symbol.IsImplicitlyDeclared)
                return;

            // Skip entry points
            if (symbol is IMethodSymbol method && IsEntryPoint(method))
                return;

            // Skip property accessors — the property symbol itself covers them
            if (symbol is IMethodSymbol accessor &&
                (accessor.MethodKind == MethodKind.PropertyGet ||
                 accessor.MethodKind == MethodKind.PropertySet))
                return;

            // Skip event accessors
            if (symbol is IMethodSymbol eventAccessor &&
                (eventAccessor.MethodKind == MethodKind.EventAdd ||
                 eventAccessor.MethodKind == MethodKind.EventRemove))
                return;

            // Skip constructors — they are invoked implicitly
            if (symbol is IMethodSymbol ctor &&
                ctor.MethodKind == MethodKind.Constructor)
                return;

            // Skip local functions - they are scoped to a method body and references
            // to them won't resolve correctly in the compilation-level identifier walk
            if (symbol is IMethodSymbol localFunc &&
                localFunc.MethodKind == MethodKind.LocalFunction)
                return;

            // Skip interface implementations - the member is reachable via the interface
            if (symbol is IMethodSymbol ifaceMethod &&
                !ifaceMethod.ExplicitInterfaceImplementations.IsEmpty)
                return;

            if (symbol is IPropertySymbol ifaceProp &&
                !ifaceProp.ExplicitInterfaceImplementations.IsEmpty)
                return;

            // Skip overrides — they satisfy a base class contract
            if (symbol.IsOverride)
                return;

            declared.TryAdd(symbol, false);
        }

        private static void ReportUnreferenced(
            CompilationAnalysisContext context,
            ConcurrentDictionary<ISymbol, bool> declared,
            ConcurrentBag<SemanticModel> semanticModels)
        {
            if (declared.IsEmpty)
                return;

            // Walk every syntax tree using the models Roslyn already computed
            foreach (var semanticModel in semanticModels)
            {
                var tree = semanticModel.SyntaxTree;
                var root = tree.GetRoot(context.CancellationToken);

                foreach (var identifier in root.DescendantTokens())
                {
                    if (!identifier.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.IdentifierToken))
                        continue;

                    var node = identifier.Parent;
                    if (node is null)
                        continue;

                    var symbolInfo = semanticModel.GetSymbolInfo(node, context.CancellationToken);
                    var referenced = symbolInfo.Symbol ?? symbolInfo.CandidateSymbols.FirstOrDefault();
                    if (referenced is null)
                        continue;

                    // Unwrap to original definition so partial methods, etc. resolve correctly
                    referenced = referenced.OriginalDefinition;

                    if (declared.ContainsKey(referenced))
                        declared[referenced] = true;
                }
            }

            // Report anything still marked as unreferenced
            foreach (var kvp in declared)
            {
                if (kvp.Value)
                    continue;

                var symbol = kvp.Key;
                var location = symbol.Locations.FirstOrDefault();
                if (location is null)
                    continue;

                context.ReportDiagnostic(Diagnostic.Create(
                    Rule,
                    location,
                    symbol.Name));
            }
        }

        private static bool IsEntryPoint(IMethodSymbol method)
        {
            return method.Name == "Main"
                && method.IsStatic
                && (method.Parameters.IsEmpty ||
                    (method.Parameters.Length == 1 &&
                     method.Parameters[0].Type is IArrayTypeSymbol arr &&
                     arr.ElementType.SpecialType == SpecialType.System_String));
        }
    }
}
