using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Linq;

namespace Tracer.SourceGenerator
{
  [Generator]
  public class TraceMethodDatabaseSourceGenerator : IIncrementalGenerator
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
                    Parent: MethodDeclarationSyntax or ConstructorDeclarationSyntax or LocalFunctionStatementSyntax
                  }
                }
              }
            }
          }
        },
        static (context, _) =>
        {
          var invocationExpressionSyntax = context.Node as InvocationExpressionSyntax;
          var syntaxNode = invocationExpressionSyntax.Parent.Parent.Parent.Parent.Parent.Parent;
          var symbol = context.SemanticModel.GetDeclaredSymbol(syntaxNode) as IMethodSymbol;
          return (invocationExpressionSyntax, symbol);
        }
      );

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
  public static class TraceMethodDatabaseFiller
  {{
    public static void Fill()
    {{
      {string.Join("\r\n      ", traceMethodInfos.Select(traceMethodInfo => $"TraceMethodDatabase.AddMethod(@\"{traceMethodInfo.filePath}\", {traceMethodInfo.lineNumber}, @\"{traceMethodInfo.method}\");"))}
      TraceMethodDatabase.FinishAdding();
    }}
  }}
}}
";

      context.AddSource("TraceMethodDatabaseFiller.g.cs", source);
    }
  }
}
