using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using GitLabWebhook.Models;
using System.Text.Json;

namespace GitLabWebhook.CodeReviewServices
{
    public class StaticAnalyzerService
    {

        private IConfiguration _configuration;
        private string _npxPath;

        public StaticAnalyzerService(IConfiguration configuration){

            this._configuration = configuration;
            this._npxPath = configuration["Linting:NpxPath"];
        }

        public async Task<IEnumerable<CustomDiagnostic>> AnalyzeDiffedFilesAsync(List<FileDiff> fileDiffs)
        {
            var diagnostics = new List<CustomDiagnostic>();

            foreach (var fileDiff in fileDiffs)
            {
                var framework = FrameworkDetector.DetectFrameworkFromFiles(fileDiffs.Select(fd => fd.FileName).ToList());

                if (framework == CodeFramework.DotNet)
                {
                    var syntaxTree = CSharpSyntaxTree.ParseText(fileDiff.FileContents, path: fileDiff.FileName);
                    var compilation = CSharpCompilation.Create("CodeAnalysis")
                        .AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location))
                        .AddSyntaxTrees(syntaxTree);

                    var analyzers = GetRoslynAnalyzers();
                    foreach (var analyzer in analyzers)
                    {
                        var diagnosticResults = await GetDiagnosticsFromAnalyzer(compilation, analyzer);
                        diagnostics.AddRange(diagnosticResults);
                    }
                }
                else if (framework == CodeFramework.Angular)
                {
                    var angularDiagnostics = await AnalyzeWithEslintAsync(fileDiff);
                    diagnostics.AddRange(angularDiagnostics);
                }
            }

            return diagnostics;
        }

        private async Task<IEnumerable<CustomDiagnostic>> AnalyzeWithEslintAsync(FileDiff fileDiff)
        {
            var diagnostics = new List<CustomDiagnostic>();
            var tempPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

            try
            {
                var pluginPath = Path.Combine(AppContext.BaseDirectory, "eslint-plugin-angular-smells");

                Directory.CreateDirectory(tempPath);
                var tempFile = Path.Combine(tempPath, Path.GetFileName(fileDiff.FileName));
                await File.WriteAllTextAsync(tempFile, fileDiff.FileContents);

                var psi = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = _npxPath,
                    Arguments = $"eslint \"{tempPath}\"  -f json --config \"{Path.Combine(pluginPath, ".eslintrc.js")}\"",
                    WorkingDirectory = pluginPath,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = System.Diagnostics.Process.Start(psi);
                var output = await process.StandardOutput.ReadToEndAsync();
                var error = await process.StandardError.ReadToEndAsync();

                process.WaitForExit();

                if (process.ExitCode != 0 && string.IsNullOrWhiteSpace(output))
                {
                    Console.WriteLine("ESLint error: " + error);
                    return diagnostics;
                }

              
                return ParseEslintOutput(output);
                
                // JsonSerializer.Deserialize<List<EslintResult>>(output);
                // foreach (var result in results)
                // {
                //     foreach (var message in result.Messages)
                //     {
                //         diagnostics.Add(new CustomDiagnostic
                //         {
                //             FilePath = fileDiff.FileName,
                //             Message = message.Message,
                //             Line = message.Line,
                //             Column = message.Column,
                //             Severity = message.Severity == 2 ? "Error" : "Warning",
                //             Source = "eslint"
                //         });
                //     }
                // }


            }catch(Exception e){

                throw e; // rethrow
            }
            finally
            {
                if (Directory.Exists(tempPath))
                    Directory.Delete(tempPath, recursive: true);
            }

            return diagnostics;
        }

        public class CustomDiagnostic
        {
            public string RuleId { get; set; }
            public string FilePath { get; set; }
            public string Message { get; set; }
            public int Line { get; set; }
            public int Column { get; set; }
            public string Severity { get; set; } // "Warning" or "Error"
            public string Source { get; set; }   // "Roslyn" or "ESLint"
        }


        public class EslintMessage
        {
            public string RuleId { get; set; } // sometimes this can be string or null
            public int Severity { get; set; }
            public string Message { get; set; }
            public int Line { get; set; }
            public int Column { get; set; }
        }

        public class EslintResult
        {
            public string FilePath { get; set; }
            public List<EslintMessage> Messages { get; set; } = new();
            public int ErrorCount { get; set; }
            public int WarningCount { get; set; }
        }

        public static List<CustomDiagnostic> ParseEslintOutput(string eslintJson)
        {
            var results = JsonSerializer.Deserialize<List<EslintResult>>(eslintJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            var diagnostics = new List<CustomDiagnostic>();

            foreach (var result in results)
            {
                foreach (var message in result.Messages)
                {
                    diagnostics.Add(new CustomDiagnostic
                    {
                        FilePath = result.FilePath,
                        Message = message.Message,
                        RuleId = message.RuleId?.ToString(),
                        Severity = message.Severity switch
                        {
                            2 => "Error",
                            1 => "Warning",
                            _ => "Info"
                        },
                        Line = message.Line,
                        Column = message.Column
                    });
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
        private static async Task<IEnumerable<CustomDiagnostic>> GetDiagnosticsFromAnalyzer(CSharpCompilation compilation, DiagnosticAnalyzer analyzer)
        {
            var additionalText = ImmutableArray<AdditionalText>.Empty; // Empty, or pass valid AdditionalText if required.
            var analyzerOptions = new AnalyzerOptions(additionalText);  // Correct constructor

            var compilationWithAnalyzers = compilation.WithAnalyzers(ImmutableArray.Create(analyzer));

            var diagnostics = await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync();
            var customDiagnostics = new List<CustomDiagnostic>();

            foreach (var diagnostic in diagnostics)
            {
                var location = diagnostic.Location;
                var lineSpan = location.GetLineSpan();

                customDiagnostics.Add(new CustomDiagnostic
                {
                    FilePath = lineSpan.Path,
                    Message = diagnostic.GetMessage(),
                    Line = lineSpan.StartLinePosition.Line + 1,
                    Column = lineSpan.StartLinePosition.Character + 1,
                    Severity = diagnostic.Severity.ToString(), // Info, Warning, Error
                    Source = "Roslyn"
                });
            }

            return customDiagnostics;
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
