using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Tracer.SourceGenerator
{
  [Generator]
  public class TraceDatabaseMethodSourceGenerator : IIncrementalGenerator
  {
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
      var traceCreations = context.SyntaxProvider.CreateSyntaxProvider(
        static (node, _) => IsPotentialTraceCreation(node),
        static (context, _) => TransformPotentialTraceCreation(context)
      ).Where(potentialTraceCreation => potentialTraceCreation is not (null, null));

      var invalidTraceCreationLocations = traceCreations.Where(IsNotValidTraceCreation).Collect();
      context.RegisterSourceOutput(invalidTraceCreationLocations, ReportDiagnostics_3_4_5);

      var validFormatCalls = traceCreations.Where(IsValidTraceCreation).Collect();
      context.RegisterSourceOutput(validFormatCalls, ReportDiagnostics_6);

      var compilationAndFormatCalls = context.CompilationProvider.Combine(validFormatCalls);
      context.RegisterSourceOutput(compilationAndFormatCalls, static (context, source) => GenerateCode(source.Left, source.Right, context));
    }

    private static bool IsPotentialTraceCreation(SyntaxNode node) => node is InvocationExpressionSyntax
    {
      Expression: MemberAccessExpressionSyntax { Name.Identifier.Text: "CreateNew" }
    };

    private static (InvocationExpressionSyntax TraceCreation, GeneratorSyntaxContext? Context) TransformPotentialTraceCreation(GeneratorSyntaxContext context)
    {
      var traceCreation = context.Node as InvocationExpressionSyntax;
      var symbolInfo = context.SemanticModel.GetSymbolInfo(traceCreation);
      if ((symbolInfo.Symbol ?? symbolInfo.CandidateSymbols.First()).ToDisplayString() !=
        "Tracer.Contracts.ITraceFactory.CreateNew(string, int)")
      {
        return (null, null);
      }
      return (traceCreation, context);
    }

    private static bool IsValidTraceCreation((InvocationExpressionSyntax TraceCreation, GeneratorSyntaxContext? Context) tuple) => tuple.TraceCreation is
    {
      Parent: EqualsValueClauseSyntax
      {
        Parent: VariableDeclaratorSyntax
        {
          Parent: VariableDeclarationSyntax
          {
            Parent: (LocalDeclarationStatementSyntax { UsingKeyword.Text: "using" } or UsingStatementSyntax) and StatementSyntax
            {
              Parent: BlockSyntax
              {
                Parent: MethodDeclarationSyntax or
                        ConstructorDeclarationSyntax or
                        LocalFunctionStatementSyntax or
                        AccessorDeclarationSyntax { Parent: AccessorListSyntax { Parent: PropertyDeclarationSyntax } }
              }
            }
          }
        }
      }
    };

    private static bool IsNotValidTraceCreation((InvocationExpressionSyntax TraceCreation, GeneratorSyntaxContext? Context) tuple) => !IsValidTraceCreation(tuple);

    private static void ReportDiagnostics_3_4_5(SourceProductionContext context, ImmutableArray<(InvocationExpressionSyntax TraceCreation, GeneratorSyntaxContext? Context)> tuples)
    {
      foreach (var tuple in tuples)
      {
        if (tuple.TraceCreation.Parent is not EqualsValueClauseSyntax ||
            tuple.TraceCreation.Parent.Parent is not VariableDeclaratorSyntax ||
            tuple.TraceCreation.Parent.Parent.Parent is not VariableDeclarationSyntax)
        {
          context.ReportDiagnostic(Diagnostic.Create(
            new DiagnosticDescriptor(
                id: "TRACER_3",
                title: "Wrong use of CreateNew.",
                messageFormat: "ITrace returned by CreateNew must be assigned to a newly declared variable.",
                category: "Design",
                defaultSeverity: DiagnosticSeverity.Error,
                isEnabledByDefault: true
            ), tuple.TraceCreation.GetLocation())
          );
        }
        else if (tuple.TraceCreation.Parent.Parent.Parent.Parent is not (LocalDeclarationStatementSyntax { UsingKeyword.Text: "using" } or UsingStatementSyntax))
        {
          context.ReportDiagnostic(Diagnostic.Create(
            new DiagnosticDescriptor(
                id: "TRACER_4",
                title: "Wrong use of CreateNew.",
                messageFormat: "CreateNew must be used within a using block or with using statement.",
                category: "Design",
                defaultSeverity: DiagnosticSeverity.Error,
                isEnabledByDefault: true
            ), tuple.TraceCreation.GetLocation())
          );
        }
        else if (tuple.TraceCreation.Parent.Parent.Parent.Parent.Parent.Parent is not (
                  MethodDeclarationSyntax or
                  ConstructorDeclarationSyntax or
                  LocalFunctionStatementSyntax or
                  AccessorDeclarationSyntax { Parent: AccessorListSyntax { Parent: PropertyDeclarationSyntax } }))
        {
          context.ReportDiagnostic(Diagnostic.Create(
            new DiagnosticDescriptor(
                id: "TRACER_5",
                title: "Wrong use of CreateNew.",
                messageFormat: "CreateNew must be used within one of the following: method, constructor, local function or a property.",
                category: "Design",
                defaultSeverity: DiagnosticSeverity.Error,
                isEnabledByDefault: true
            ), tuple.TraceCreation.GetLocation())
          );
        }
      }
    }

    private static void ReportDiagnostics_6(SourceProductionContext context, ImmutableArray<(InvocationExpressionSyntax TraceCreation, GeneratorSyntaxContext? Context)> tuples)
    {
      var lineSpans = new HashSet<(string, int)>();
      foreach (var tuple in tuples)
      {
        var location = tuple.TraceCreation.GetLocation();
        var mappedLineSpan = location.GetMappedLineSpan();
        var lineSpan = (mappedLineSpan.Path, mappedLineSpan.StartLinePosition.Line);
        if (lineSpans.Contains(lineSpan))
        {
          context.ReportDiagnostic(Diagnostic.Create(
          new DiagnosticDescriptor(
              id: "TRACER_6",
              title: "Wrong use of CreateNew.",
              messageFormat: "Only one CreateNew is allowed per line.",
              category: "Design",
              defaultSeverity: DiagnosticSeverity.Error,
              isEnabledByDefault: true
          ), location)
        );
        }
        else
        {
          lineSpans.Add(lineSpan);
        }
      }
    }

    private static void GenerateCode(Compilation compilation, ImmutableArray<(InvocationExpressionSyntax TraceCreation, GeneratorSyntaxContext? Context)> tuples, SourceProductionContext context)
    {
      var traceMethodInfos = tuples.Select(tuple =>
      {
        var lineSpan = tuple.TraceCreation.GetLocation().GetLineSpan();
        var filePath = lineSpan.Path;
        var lineNumber = lineSpan.StartLinePosition.Line + 1;

        var syntaxNode = tuple.TraceCreation.Parent.Parent.Parent.Parent.Parent.Parent;

        var isAsyncVoid = syntaxNode is MethodDeclarationSyntax
        {
          ReturnType: PredefinedTypeSyntax { Keyword: SyntaxToken { Text: "void" } },
          Modifiers: SyntaxTokenList { Count: > 0 } syntaxTokenList
        } && syntaxTokenList.Any(x => x is SyntaxToken { Value: "async" });

        var symbol = tuple.Context?.SemanticModel.GetDeclaredSymbol(syntaxNode) as IMethodSymbol;
        var method = $"{symbol.ContainingType} {symbol.Name}({string.Join(", ", symbol.Parameters)})";

        return (filePath, lineNumber, method, isAsyncVoid);
      });

      var mainMethod = compilation.GetEntryPoint(context.CancellationToken);

      var sourceCode =
$@"using Tracer.Utilities;

namespace {mainMethod.ContainingNamespace.ToDisplayString()}
{{
  public static partial class TraceDatabaseFiller
  {{
    public static void Fill()
    {{
      FillTraceMethods();
      FillTraceFormatParameters();
    }}

    private static void FillTraceMethods()
    {{
      TraceDatabase.AddTraceMethodInfos(
      [
        {string.Join(",\r\n        ", traceMethodInfos.Select(traceMethodInfo => $"(@\"{traceMethodInfo.filePath}\", {traceMethodInfo.lineNumber}, @\"{traceMethodInfo.method}\", {(traceMethodInfo.isAsyncVoid ? "true" : "false")})"))}
      ]);
    }}

    static partial void FillTraceFormatParameters();
  }}
}}";

      context.AddSource("TraceDatabaseFiller.Method.g.cs", sourceCode);
    }
  }
}
