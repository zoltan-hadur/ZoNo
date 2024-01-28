using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Tracer.SourceGenerator
{
  [Generator]
  public class TraceDatabaseFormatSourceGenerator : IIncrementalGenerator
  {
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
      var formatCalls = context.SyntaxProvider.CreateSyntaxProvider(
        static (node, _) => IsPotentialFormatCall(node),
        static (context, _) => TransformPotentialFormatCall(context)
      ).Where(potentialFormatCall => potentialFormatCall is not null);

      var invalidFormatCallLocations = formatCalls.Where(IsNotValidFormatCall).Select((x, _) => x.GetLocation()).Collect();
      context.RegisterSourceOutput(invalidFormatCallLocations, ReportDiagnostics_1);

      var validFormatCalls = formatCalls.Where(IsValidFormatCall).Collect();
      context.RegisterSourceOutput(validFormatCalls, ReportDiagnostics_2);

      var compilationAndFormatCalls = context.CompilationProvider.Combine(validFormatCalls);
      context.RegisterSourceOutput(compilationAndFormatCalls, static (context, source) => GenerateCode(source.Left, source.Right, context));
    }

    private static bool IsPotentialFormatCall(SyntaxNode node) => node is InvocationExpressionSyntax
    {
      Expression: IdentifierNameSyntax { Identifier.Text: "Format" } or
                  MemberAccessExpressionSyntax { Name.Identifier.Text: "Format" }
    };

    private static InvocationExpressionSyntax TransformPotentialFormatCall(GeneratorSyntaxContext context)
    {
      var formatCall = context.Node as InvocationExpressionSyntax;
      var symbolInfo = context.SemanticModel.GetSymbolInfo(formatCall);
      if ((symbolInfo.Symbol ?? symbolInfo.CandidateSymbols.First()).ToDisplayString() !=
        "Tracer.Utilities.TraceUtility.Format(System.Collections.Generic.IEnumerable<object>, string, int)")
      {
        return null;
      }
      return formatCall;
    }

    private static bool IsValidFormatCall(InvocationExpressionSyntax node) => node is
    {
      ArgumentList: ArgumentListSyntax
      {
        Arguments: SeparatedSyntaxList<ArgumentSyntax> { Count: 1 } arguments
      }
    } && arguments[0] is ArgumentSyntax
    {
      Expression: CollectionExpressionSyntax
    };

    private static bool IsNotValidFormatCall(InvocationExpressionSyntax node) => !IsValidFormatCall(node);

    private static void ReportDiagnostics_1(SourceProductionContext context, ImmutableArray<Location> invalidFormatCallLocations)
    {
      foreach (var location in invalidFormatCallLocations)
      {
        context.ReportDiagnostic(Diagnostic.Create(
          new DiagnosticDescriptor(
              id: "TRACER_1",
              title: "Wrong use of Format.",
              messageFormat: "Format must use a single collection expression as its single parameter.",
              category: "Design",
              defaultSeverity: DiagnosticSeverity.Error,
              isEnabledByDefault: true
          ), location)
        );
      }
    }

    private static void ReportDiagnostics_2(SourceProductionContext context, ImmutableArray<InvocationExpressionSyntax> formatCalls)
    {
      var lineSpans = new HashSet<(string, int)>();
      foreach (var formatCall in formatCalls)
      {
        var location = formatCall.GetLocation();
        var mappedLineSpan = location.GetMappedLineSpan();
        var lineSpan = (mappedLineSpan.Path, mappedLineSpan.StartLinePosition.Line);
        if (lineSpans.Contains(lineSpan))
        {
          context.ReportDiagnostic(Diagnostic.Create(
          new DiagnosticDescriptor(
              id: "TRACER_2",
              title: "Wrong use of Format.",
              messageFormat: "Only one Format call is allowed per line.",
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

    private static void GenerateCode(Compilation compilation, ImmutableArray<InvocationExpressionSyntax> formatCalls, SourceProductionContext context)
    {
      var formatCallInfos = formatCalls.Select(formatCall =>
      {
        var lineSpan = formatCall.GetLocation().GetLineSpan();
        var filePath = lineSpan.Path;
        var lineNumber = lineSpan.StartLinePosition.Line + 1;
        var collectionExpressionSyntax = formatCall.ArgumentList.Arguments[0].Expression as CollectionExpressionSyntax;
        var parameters = collectionExpressionSyntax.Elements.Select(x => x.ToString());
        return (filePath, lineNumber, parameters);
      });

      var mainMethod = compilation.GetEntryPoint(context.CancellationToken);

      var sourceCode =
$@"using Tracer.Utilities;

namespace {mainMethod.ContainingNamespace.ToDisplayString()}
{{
  public static partial class TraceDatabaseFiller
  {{
    static partial void FillTraceFormatParameters()
    {{
      TraceDatabase.AddTraceFormatParameters(
      [
        {string.Join(",\r\n        ", formatCallInfos
          .Select(formatCallInfo => $"(@\"{formatCallInfo.filePath}\", {formatCallInfo.lineNumber}, [{string
            .Join(", ", formatCallInfo.parameters
              .Select(parameter => $"@\"{parameter}\""))}])"))}
      ]);
    }}
  }}
}}";

      context.AddSource("TraceDatabaseFiller.Format.g.cs", sourceCode);
    }
  }
}
