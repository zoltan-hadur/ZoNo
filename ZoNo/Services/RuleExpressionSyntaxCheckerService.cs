using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Data;
using System.Text.RegularExpressions;
using Tracer.Contracts;
using ZoNo.Contracts.Services;

namespace ZoNo.Services
{
  public class RuleExpressionSyntaxCheckerService(
    ITraceFactory traceFactory) : IRuleExpressionSyntaxCheckerService
  {
    private readonly ITraceFactory _traceFactory = traceFactory;

    private MetadataReference[] _metadataReferences =
    [
      MetadataReference.CreateFromFile(Path.Combine(Path.GetDirectoryName(typeof(object).Assembly.Location), "System.Private.CoreLib.dll")),
      MetadataReference.CreateFromFile(Path.Combine(Path.GetDirectoryName(typeof(object).Assembly.Location), "System.Linq.dll")),
      MetadataReference.CreateFromFile(Path.Combine(Path.GetDirectoryName(typeof(object).Assembly.Location), "System.Collections.dll")),
      MetadataReference.CreateFromFile(Path.Combine(Path.GetDirectoryName(typeof(object).Assembly.Location), "System.Runtime.dll"))
    ];
    private CSharpCompilation _compilation = CSharpCompilation.Create(
      assemblyName: Path.GetRandomFileName(),
      options: new CSharpCompilationOptions(
        outputKind: OutputKind.DynamicallyLinkedLibrary,
        platform: Platform.X64
      )
    );

    public async Task<(bool IsValid, string ErrorMessage)> CheckSyntaxAsync<TInput>(string inputExpression) =>
      await CheckSyntaxAsync(inputExpression, typeof(TInput));

    public async Task<(bool IsValid, string ErrorMessage)> CheckSyntaxAsync<TInput, TOutput>(string outputExpression) =>
      await CheckSyntaxAsync(outputExpression, typeof(TInput), typeof(TOutput));

    private async Task<(bool IsValid, string ErrorMessage)> CheckSyntaxAsync(string expression, Type inputType, Type outputType = null)
    {
      using var trace = _traceFactory.CreateNew();

      return await Task.Run(() =>
      {
        string code = $$"""
        using System;
        using System.Linq;
        using ZoNo.Models;
        
        public static class RuleEvaluator
        {
          public static {{(outputType is null ? "bool" : "void")}} Evaluate({{inputType.Name}} Input{{(outputType is null ? string.Empty : $", {outputType.Name} Output")}})
          {
            var RemoveThisElementFromList = false;

            {{expression.Replace("\r", "\r    ")}}
          }
        }
        """;
        trace.Debug(Format([code]));

        var references = _metadataReferences.Append(MetadataReference.CreateFromFile(inputType.Assembly.Location));
        if (outputType is not null)
        {
          references = references.Append(MetadataReference.CreateFromFile(outputType.Assembly.Location));
        }
        var compilation = _compilation.AddSyntaxTrees([CSharpSyntaxTree.ParseText(code)]).WithReferences(references);

        using var memoryStream = new MemoryStream();
        var result = compilation.Emit(memoryStream);
        var errorMessages = string.Join(Environment.NewLine, result.Diagnostics
          .Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
          .Select(diagnostic => diagnostic.ToString())
          .Select(errorMessage => Regex.Replace(errorMessage, @"^\((\d+),(\d+)\)", match => $"({int.Parse(match.Groups[1].Value) - 10},{int.Parse(match.Groups[2].Value) - 4})"))
        );
        trace.Debug(Format([result.Success, errorMessages]));

        return (result.Success, errorMessages);
      });
    }
  }
}
