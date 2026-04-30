using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace Analyzer
{

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ForbiddenUnixTimeAnalyzer : DiagnosticAnalyzer
    {
        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: "FT0001",
            title: "Forbidden DateTimeOffset method",
            messageFormat: "Do not use '{0}'. Use approved wrapper instead.",
            category: "Usage",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
        }

        private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
        {
            var invocation = (InvocationExpressionSyntax)context.Node;
            var symbolInfo = context.SemanticModel.GetSymbolInfo(invocation);
            var symbol = symbolInfo.Symbol as IMethodSymbol ?? symbolInfo.CandidateSymbols.FirstOrDefault() as IMethodSymbol;

            if (symbol is null)
                return;

            var dtoType = context.SemanticModel.Compilation
                .GetTypeByMetadataName("System.DateTimeOffset");

            if (!SymbolEqualityComparer.Default.Equals(symbol.ContainingType, dtoType))
                return;

            if (symbol.Name == "ToUnixTimeSeconds" ||
                symbol.Name == "ToUnixTimeMilliseconds" ||
                symbol.Name == "FromUnixTimeSeconds" ||
                symbol.Name == "FromUnixTimeMilliseconds")
            {
                var diagnostic = Diagnostic.Create(
                    Rule,
                    invocation.GetLocation(),
                    symbol.Name);

                context.ReportDiagnostic(diagnostic);
            }
        }

        private string GetDebuggerDisplay()
        {
            return ToString();
        }
    }
}
