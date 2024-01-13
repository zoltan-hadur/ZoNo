using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace Tracer.SourceGenerator
{
  [Generator]
  public class TraceMethodDatabaseSourceGenerator : ISourceGenerator
  {
    public void Execute(GeneratorExecutionContext context)
    {
      var traceMethodInfos = new List<(string FilePath, int LineNumber, string Method)>();

      foreach (var syntaxTree in context.Compilation.SyntaxTrees)
      {
        var semanticModel = context.Compilation.GetSemanticModel(syntaxTree);

        // Find all occurrences when a trace is created via factory
        var invocationExpressionSyntaxes = syntaxTree.GetRoot().DescendantNodes()
          .Where(syntaxNode => syntaxNode is InvocationExpressionSyntax invocationExpressionSyntax &&
                               semanticModel.GetSymbolInfo(invocationExpressionSyntax) is SymbolInfo symbolInfo &&
                               symbolInfo.Symbol is ISymbol symbol &&
                               symbol.ToString() == "Tracer.Contracts.ITraceFactory.CreateNew(string, int)")
          .Cast<InvocationExpressionSyntax>().ToArray();

        // Iterate through those occurrences
        foreach (var invocationExpressionSyntax in invocationExpressionSyntaxes)
        {
          // Check that the created trace is assigned to a freshly created variable with using var or in a using block which is in a method
          if (invocationExpressionSyntax.Parent is EqualsValueClauseSyntax equalsValueClauseSyntax &&
              equalsValueClauseSyntax.Parent is VariableDeclaratorSyntax variableDeclaratorSyntax &&
              variableDeclaratorSyntax.Parent is VariableDeclarationSyntax variableDeclarationSyntax &&
              variableDeclarationSyntax.Parent is StatementSyntax localDeclarationOrUsingStatementSyntax &&
                (localDeclarationOrUsingStatementSyntax is LocalDeclarationStatementSyntax localDeclarationStatementSyntax && localDeclarationStatementSyntax.UsingKeyword.Text == "using" ||
                localDeclarationOrUsingStatementSyntax is UsingStatementSyntax) &&
              localDeclarationOrUsingStatementSyntax.Parent is BlockSyntax blockSyntax)
          {
            if (blockSyntax.Parent is SyntaxNode syntaxNode &&
                  (syntaxNode is MethodDeclarationSyntax ||
                  syntaxNode is ConstructorDeclarationSyntax ||
                  syntaxNode is LocalFunctionStatementSyntax))
            {
              var symbol = semanticModel.GetDeclaredSymbol(syntaxNode) as IMethodSymbol;
              var lineSpan = invocationExpressionSyntax.GetLocation().GetLineSpan();
              var filePath = lineSpan.Path;
              var lineNumber = lineSpan.StartLinePosition.Line + 1;
              var method = $"{symbol.ContainingType} {symbol.Name}({string.Join(", ", symbol.Parameters)})";
              traceMethodInfos.Add((filePath, lineNumber, method));
            }
            else
            {
              var symbolInfo = semanticModel.GetSymbolInfo(invocationExpressionSyntax);
              context.ReportDiagnostic(Diagnostic.Create(
                new DiagnosticDescriptor(
                    "TRACER_1",
                    "Trace cannot be created here.",
                    "Method {0} cannot be used in {1}.",
                    "Design",
                    DiagnosticSeverity.Error,
                    true), invocationExpressionSyntax.GetLocation(), symbolInfo.Symbol.ToString(), blockSyntax.Parent.Kind()));
            }
          }
        }
      }

      var mainMethod = context.Compilation.GetEntryPoint(context.CancellationToken);

      var source = $@"
using Tracer.Utilities;

namespace {mainMethod.ContainingNamespace.ToDisplayString()}
{{
  public static class TraceMethodDatabaseFiller
  {{
    public static void Fill()
    {{
      {string.Join("\r\n      ", traceMethodInfos.Select(traceMethodInfo => $"TraceMethodDatabase.AddMethod(@\"{traceMethodInfo.FilePath}\", {traceMethodInfo.LineNumber}, @\"{traceMethodInfo.Method}\");"))}
      TraceMethodDatabase.FinishAdding();
    }}
  }}
}}
";

      context.AddSource("TraceMethodDatabaseFiller.g.cs", source);
    }

    public void Initialize(GeneratorInitializationContext context)
    {

    }
  }
}
