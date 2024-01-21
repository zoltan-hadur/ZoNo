using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Linq;

namespace Tracer.SourceGenerator
{
  [Generator]
  public class TraceDatabaseMethodSourceGenerator : IIncrementalGenerator
  {
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
      var traceCreationsAndEnclosingMethods = context.SyntaxProvider.CreateSyntaxProvider(
        static (node, _) => node is InvocationExpressionSyntax
        {
          Expression: MemberAccessExpressionSyntax { Name.Identifier.Text: "CreateNew" },
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
        },
        static (context, _) =>
        {
          var invocationExpressionSyntax = context.Node as InvocationExpressionSyntax;

          var symbolInfo = context.SemanticModel.GetSymbolInfo(invocationExpressionSyntax);
          var symbolString = (symbolInfo.Symbol ?? symbolInfo.CandidateSymbols.First()).ToString();
          if (symbolString != "Tracer.Contracts.ITraceFactory.CreateNew(string, int)")
          {
            return (null, null);
          }

          var syntaxNode = invocationExpressionSyntax.Parent.Parent.Parent.Parent.Parent.Parent;
          var symbol = context.SemanticModel.GetDeclaredSymbol(syntaxNode) as IMethodSymbol;
          return (invocationExpressionSyntax, symbol);
        }
      ).Where(tuple => tuple is not (null, null));

      var compilationAndTraceCreationsAndEnclosingMethods = context.CompilationProvider.Combine(traceCreationsAndEnclosingMethods.Collect());

      context.RegisterSourceOutput(compilationAndTraceCreationsAndEnclosingMethods, static (context, source) => Execute(source.Left, source.Right, context));
    }

    private static void Execute(Compilation compilation, ImmutableArray<(InvocationExpressionSyntax InvocationExpressionSyntax, IMethodSymbol MethodSymbol)> traceCreationsAndEnclosingMethods, SourceProductionContext context)
    {
      var traceMethodInfos = traceCreationsAndEnclosingMethods.Select(tuple =>
      {
        var lineSpan = tuple.InvocationExpressionSyntax.GetLocation().GetLineSpan();
        var filePath = lineSpan.Path;
        var lineNumber = lineSpan.StartLinePosition.Line + 1;
        var method = $"{tuple.MethodSymbol.ContainingType} {tuple.MethodSymbol.Name}({string.Join(", ", tuple.MethodSymbol.Parameters)})";
        return (filePath, lineNumber, method);
      });

      var mainMethod = compilation.GetEntryPoint(context.CancellationToken);

      var source = $@"
using Tracer.Utilities;

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
      TraceDatabase.AddTraceMethods(
      [
        {string.Join(",\r\n        ", traceMethodInfos.Select(traceMethodInfo => $"(@\"{traceMethodInfo.filePath}\", {traceMethodInfo.lineNumber}, @\"{traceMethodInfo.method}\")"))}
      ]);
    }}

    static partial void FillTraceFormatParameters();
  }}
}}
";

      context.AddSource("TraceDatabaseFiller.Method.g.cs", source);
    }
  }
}
