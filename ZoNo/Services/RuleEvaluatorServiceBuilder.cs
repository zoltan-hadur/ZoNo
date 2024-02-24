using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Reflection;
using System.Runtime.Loader;
using Tracer.Contracts;
using ZoNo.Contracts.Services;
using ZoNo.Models;

namespace ZoNo.Services
{
  public class RuleEvaluatorServiceBuilder(
    ITraceFactory traceFactory) : IRuleEvaluatorServiceBuilder
  {
    private readonly ITraceFactory _traceFactory = traceFactory;

    private class RuleEvaluatorService<TInput, TOutput>(
      IEnumerable<Rule> rules,
      ITraceFactory traceFactory) : IRuleEvaluatorService<TInput, TOutput>
    {
      private class CustomAssemblyLoadContext : AssemblyLoadContext
      {
        public CustomAssemblyLoadContext() : base(isCollectible: true)
        {

        }

        protected override Assembly Load(AssemblyName name)
        {
          return null;
        }
      }

      private readonly ITraceFactory _traceFactory = traceFactory;
      private readonly IList<Rule> _rules = rules.Select(rule => rule.Clone()).ToArray();
      private CustomAssemblyLoadContext _assemblyLoadContext = new();
      private MethodInfo _ruleEvaluator = null;

      public async Task InitializeAsync()
      {
        using var trace = _traceFactory.CreateNew();

        await Task.Run(() =>
        {
          var className = "RuleEvaluator";
          var methodName = "Evaluate";
          string code = $$"""
            using System;
            using System.Linq;
            using ZoNo.Models;

            public static class {{className}}
            {
              public static bool {{methodName}}(string RuleId, {{typeof(TInput).Name}} Input, {{typeof(TOutput).Name}} Output, {{typeof(ExecutionType).Name}} ExecutionType)
              {
                var RemoveThisElementFromList = false;

                switch (RuleId)
                {
                  case "ForDefaultType": return false;
            {{string.Join("\r", _rules.Select(rule => $$"""

                  // {{rule.Description}}
                  case "{{rule.Id}}":
                    {
                      switch (ExecutionType)
                      {
                        case ExecutionType.Condition:
                          {
                            {{rule.InputExpression.Replace("\r", $"\r                ")}}
                          }
                        case ExecutionType.Action:
                          {
                            {{string.Join("\r                ", rule.OutputExpressions)}}
                            return RemoveThisElementFromList;
                          }
                        default: throw new ArgumentException($"ExecutionType '{ExecutionType}' is not valid.");
                      }
                    }
            """))}}
            
                  default: throw new ArgumentException($"RuleId '{RuleId}' is not valid.");
                }
              }
            }
            """;
          trace.Debug(Format([code]));

          var assemblyPath = Path.GetDirectoryName(typeof(object).Assembly.Location);
          var compilation = CSharpCompilation.Create(
            assemblyName: Path.GetRandomFileName(),
            syntaxTrees: [CSharpSyntaxTree.ParseText(code)],
            references:
            [
              MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Private.CoreLib.dll")),
              MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Linq.dll")),
              MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Collections.dll")),
              MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Runtime.dll")),
              MetadataReference.CreateFromFile(typeof(TInput).Assembly.Location),
              MetadataReference.CreateFromFile(typeof(TOutput).Assembly.Location),
              MetadataReference.CreateFromFile(typeof(ExecutionType).Assembly.Location)
            ],
            options: new CSharpCompilationOptions(
              outputKind: OutputKind.DynamicallyLinkedLibrary,
              optimizationLevel: OptimizationLevel.Release,
              platform: Platform.X64
            )
          );

          using var memoryStream = new MemoryStream();
          var result = compilation.Emit(memoryStream);
          if (!result.Success)
          {
            throw new InvalidOperationException(string.Join(Environment.NewLine, result.Diagnostics.Select(diagnostic => diagnostic.ToString())));
          }
          memoryStream.Seek(0, SeekOrigin.Begin);
          _ruleEvaluator = _assemblyLoadContext.LoadFromStream(memoryStream).GetType(className).GetMethod(methodName);
        });
      }

      public bool EvaluateRules(TInput input, TOutput output)
      {
        using var trace = _traceFactory.CreateNew();
        trace.Debug(Format([input, output]));
        foreach (var rule in _rules)
        {
          trace.Debug(Format([rule.Description]));
          var result = EvaluateRule(rule.Id.ToString(), input, default, ExecutionType.Condition);
          trace.Debug(Format([rule.InputExpression, result]));
          if (result)
          {
            var removeThisElementFromList = EvaluateRule(rule.Id.ToString(), input, output, ExecutionType.Action);
            trace.Debug(Format([string.Join(Environment.NewLine, rule.OutputExpressions), removeThisElementFromList]));
            if (removeThisElementFromList)
            {
              return false;
            }
          }
        }
        return true;
      }

      private bool EvaluateRule(string ruleId, TInput input, TOutput output, ExecutionType executionType)
      {
        return (bool)_ruleEvaluator.Invoke(null, [ruleId, input, output, executionType]);
      }

      public void Dispose()
      {
        using var trace = _traceFactory.CreateNew();
        trace.Debug(Format([_assemblyLoadContext is null]));
        if (_assemblyLoadContext is not null)
        {
          _ruleEvaluator = null;
          _assemblyLoadContext.Unload();
          _assemblyLoadContext = null;
          GC.Collect();
        }
      }
    }

    public async Task InitializeAsync()
    {
      using var trace = _traceFactory.CreateNew();
      using var ruleEvaluator = await BuildAsync<object, object>([]);
    }

    public async Task<IRuleEvaluatorService<TInput, TOutput>> BuildAsync<TInput, TOutput>(IEnumerable<Rule> rules)
    {
      using var trace = _traceFactory.CreateNew();
      trace.Debug(Format([rules.Count()]));
      var ruleEvaluator = new RuleEvaluatorService<TInput, TOutput>(rules, _traceFactory);
      await ruleEvaluator.InitializeAsync();
      return ruleEvaluator;
    }
  }
}
