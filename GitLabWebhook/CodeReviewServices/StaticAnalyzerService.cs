using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using GitLabWebhook.Models;
using Microsoft.CodeAnalysis;

namespace GitLabWebhook.CodeReviewServicesdotne
{
    public class StaticAnalyzerService
    {
        public async Task<IEnumerable<Diagnostic>> AnalyzeDiffedFilesAsync(List<FileDiff> fileDiffs)
        {
            var diagnostics = new List<Diagnostic>();

            foreach (var fileDiff in fileDiffs)
            {
                var syntaxTree = CSharpSyntaxTree.ParseText(fileDiff.FileContents, path: fileDiff.FileName);
                var compilation = CSharpCompilation.Create("CodeAnalysis")
                    .AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location))
                    .AddSyntaxTrees(syntaxTree);

                // Get analyzers and use them
                var analyzers = GetRoslynAnalyzers();

                // Manually create compilation with analyzer diagnostics
                foreach (var analyzer in analyzers)
                {
                    var diagnosticResults = await GetDiagnosticsFromAnalyzer(compilation, analyzer);
                    diagnostics.AddRange(diagnosticResults);
                }
            }

            return diagnostics;
        }

        private static IEnumerable<DiagnosticAnalyzer> GetRoslynAnalyzers()
        {
            // Return instances of analyzers
            return new List<DiagnosticAnalyzer>
            {
                new CA1000DiagnosticAnalyzer(), // Long Method
                new CA1001DiagnosticAnalyzer(), // Long Class
                new CA1002DiagnosticAnalyzer(), // Hard Coded LocalHost
                new CA1003DiagnosticAnalyzer()  // using nameof rather than the ActionName
            };
        }

        // Get diagnostics for a specific analyzer
        private static async Task<IEnumerable<Diagnostic>> GetDiagnosticsFromAnalyzer(CSharpCompilation compilation, DiagnosticAnalyzer analyzer)
        {
            var additionalText = ImmutableArray<AdditionalText>.Empty; // Empty, or pass valid AdditionalText if required.
            var analyzerOptions = new AnalyzerOptions(additionalText);  // Correct constructor

            var compilationWithAnalyzers = compilation.WithAnalyzers(ImmutableArray.Create(analyzer));

            var diagnostics = await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync();
            return diagnostics;
        }

    }



    public class CA1000DiagnosticAnalyzer : DiagnosticAnalyzer
    {
        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            "CA1000",
            "Long Method",
            "Method '{0}' in file '{1}' (lines {2}-{3}) should not be longer than 50 lines.",
            "Naming",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true
        );

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.MethodDeclaration);
        }

        private void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            var methodDeclaration = (MethodDeclarationSyntax)context.Node;

            var startLine = methodDeclaration.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
            var endLine = methodDeclaration.GetLocation().GetLineSpan().EndLinePosition.Line + 1;
            var methodLength = endLine - startLine + 1;

            if (methodLength > 50)
            {
                var methodName = methodDeclaration.Identifier.Text;
                var filePath = context.Node.SyntaxTree.FilePath ?? "unknown";

                var diagnostic = Diagnostic.Create(
                    Rule,
                    methodDeclaration.GetLocation(),
                    methodName,
                    filePath,
                    startLine,
                    endLine
                );

                context.ReportDiagnostic(diagnostic);
            }
        }
    }






    public class CA1001DiagnosticAnalyzer : DiagnosticAnalyzer
    {
        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            "CA1001",
            "Too Many Parameters",
            "Method '{0}' in file '{1}' has {2} parameters, which exceeds the allowed maximum of 5.",
            "Design",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true
        );

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.MethodDeclaration);
        }

        private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            var methodDeclaration = (MethodDeclarationSyntax)context.Node;

            int parameterCount = methodDeclaration.ParameterList.Parameters.Count;

            if (parameterCount > 5)
            {
                var methodName = methodDeclaration.Identifier.Text;
                var filePath = context.Node.SyntaxTree.FilePath ?? "unknown";

                var diagnostic = Diagnostic.Create(
                    Rule,
                    methodDeclaration.GetLocation(),
                    methodName,
                    filePath,
                    parameterCount
                );

                context.ReportDiagnostic(diagnostic);
            }
        }
    }

    public class CA1002DiagnosticAnalyzer : DiagnosticAnalyzer
    {
        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            "CA1002",
            "Hardcoded 'localhost' URL",
            "Hardcoded 'localhost' URL found in file '{0}': \"{1}\"",
            "Reliability",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true
        );

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeLiteral, SyntaxKind.StringLiteralExpression);
            context.RegisterSyntaxNodeAction(AnalyzeInterpolated, SyntaxKind.InterpolatedStringExpression);
        }

        private static void AnalyzeLiteral(SyntaxNodeAnalysisContext context)
        {
            var literal = (LiteralExpressionSyntax)context.Node;
            var text = literal.Token.ValueText;

            if (text.Contains("localhost"))
            {
                var filePath = context.Node.SyntaxTree.FilePath ?? "unknown";
                var diagnostic = Diagnostic.Create(Rule, literal.GetLocation(), filePath, text);
                context.ReportDiagnostic(diagnostic);
            }
        }

        private static void AnalyzeInterpolated(SyntaxNodeAnalysisContext context)
        {
            var interpolated = (InterpolatedStringExpressionSyntax)context.Node;
            var combinedText = string.Concat(interpolated.Contents
                .OfType<InterpolatedStringTextSyntax>()
                .Select(part => part.TextToken.ValueText));

            if (combinedText.Contains("localhost"))
            {
                var filePath = context.Node.SyntaxTree.FilePath ?? "unknown";
                var diagnostic = Diagnostic.Create(Rule, interpolated.GetLocation(), filePath, combinedText);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }


    public class CA1003DiagnosticAnalyzer : DiagnosticAnalyzer
    {
        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: "CA1003",
            title: "Avoid nameof()",
            messageFormat: "Avoid using nameof(). Found: '{0}': \"{1}\"",
            category: "Design",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true
        );

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.InvocationExpression);
        }

        private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            var invocation = (InvocationExpressionSyntax)context.Node;
             var filePath = context.Node.SyntaxTree.FilePath ?? "unknown";


            if (invocation.Expression is IdentifierNameSyntax identifier &&
                identifier.Identifier.Text == "nameof")
            {
                var diagnostic = Diagnostic.Create(
                    Rule,
                    invocation.GetLocation(),
                    invocation.ToString(),
                    filePath);

                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}
