using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Linq;

namespace Tracer.SourceGenerator
{
  [Generator]
  public class TraceDatabaseFormatSourceGenerator : IIncrementalGenerator
  {
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
      var traceFormatCalls = context.SyntaxProvider.CreateSyntaxProvider(
        static (node, _) => node is InvocationExpressionSyntax
        {
          Expression: IdentifierNameSyntax { Identifier.Text: "Format" } or MemberAccessExpressionSyntax { Name.Identifier.Text: "Format" },
          ArgumentList: ArgumentListSyntax
          {
            Arguments: SeparatedSyntaxList<ArgumentSyntax> { Count: 1 } arguments
          }
        } && arguments[0] is ArgumentSyntax
        {
          Expression: CollectionExpressionSyntax
        },
        static (context, _) =>
        {
          var invocationExpressionSyntax = context.Node as InvocationExpressionSyntax;
          var symbolInfo = context.SemanticModel.GetSymbolInfo(invocationExpressionSyntax);
          if ((symbolInfo.Symbol ?? symbolInfo.CandidateSymbols.First()).ToString() != "Tracer.Utilities.TraceUtility.Format(System.Collections.Generic.IEnumerable<object>, string, int)")
          {
            return null;
          }
          return invocationExpressionSyntax;
        }
      ).Where(invocationExpressionSyntax => invocationExpressionSyntax is not null);

      var compilationAndTraceFormatCalls = context.CompilationProvider.Combine(traceFormatCalls.Collect());

      context.RegisterSourceOutput(compilationAndTraceFormatCalls, static (context, source) => Execute(source.Left, source.Right, context));
    }

    private static void Execute(Compilation compilation, ImmutableArray<InvocationExpressionSyntax> traceFormatCalls, SourceProductionContext context)
    {
      var traceFormatInfos = traceFormatCalls.Select(invocationExpressionSyntax =>
      {
        var lineSpan = invocationExpressionSyntax.GetLocation().GetLineSpan();
        var filePath = lineSpan.Path;
        var lineNumber = lineSpan.StartLinePosition.Line + 1;
        var collectionExpressionSyntax = invocationExpressionSyntax.ArgumentList.Arguments[0].Expression as CollectionExpressionSyntax;
        var parameters = collectionExpressionSyntax.Elements.Select(x => x.ToString()).ToArray();
        return (filePath, lineNumber, parameters);
      });

      var mainMethod = compilation.GetEntryPoint(context.CancellationToken);

      var source = $@"
using Tracer.Utilities;

namespace {mainMethod.ContainingNamespace.ToDisplayString()}
{{
  public static partial class TraceDatabaseFiller
  {{
    static partial void FillTraceFormatParameters()
    {{
      TraceDatabase.AddTraceFormatParameters(
      [
        {string.Join(",\r\n        ", traceFormatInfos.Select(traceFormatInfo => $"(@\"{traceFormatInfo.filePath}\", {traceFormatInfo.lineNumber}, [{string.Join(", ", traceFormatInfo.parameters.Select(parameter => $"@\"{parameter}\""))}])"))}
      ]);
    }}
  }}
}}
";

      context.AddSource("TraceDatabaseFiller.Format.g.cs", source);
    }
  }
}
