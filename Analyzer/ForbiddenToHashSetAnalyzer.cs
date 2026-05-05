using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace Analyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ForbiddenToHashSetAnalyzer : DiagnosticAnalyzer
    {
        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: "FT0002",
            title: "Forbidden LINQ method",
            messageFormat: "Do not use 'ToHashSet'. Use ToHashSetFast() instead.",
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
            var symbol = symbolInfo.Symbol as IMethodSymbol
                      ?? symbolInfo.CandidateSymbols.FirstOrDefault() as IMethodSymbol;

            if (symbol is null)
                return;

            // Handle extension methods properly
            var method = symbol.ReducedFrom ?? symbol;

            if (method.Name != "ToHashSet")
                return;

            if (method.ContainingType?.ToDisplayString() != "System.Linq.Enumerable")
                return;

            var diagnostic = Diagnostic.Create(
                Rule,
                invocation.GetLocation());

            context.ReportDiagnostic(diagnostic);
        }
    }
}