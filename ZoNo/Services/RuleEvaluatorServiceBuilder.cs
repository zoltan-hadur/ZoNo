using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Tracer.Contracts;
using ZoNo.Contracts.Services;
using ZoNo.Models;

namespace ZoNo.Services
{
  public class RuleEvaluatorServiceBuilder(
    ITraceFactory traceFactory) : IRuleEvaluatorServiceBuilder
  {
    private readonly ITraceFactory _traceFactory = traceFactory;

    private class RuleEvaluatorService<Input, Output>(
      IEnumerable<Rule> rules,
      ITraceFactory traceFactory) : IRuleEvaluatorService<Input, Output>
    {
      private readonly ITraceFactory _traceFactory = traceFactory;
      private readonly IList<Rule> _rules = rules.Select(rule => rule.Clone()).ToArray();
      private ScriptRunner<bool> _evaluator;

      public async Task InitializeAsync()
      {
        using var trace = _traceFactory.CreateNew();

        await Task.Run(() =>
        {
          var code = $$"""
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
                        {{rule.InputExpression.Replace("\r", $"\r            ")}}
                      } break;
                    case ExecutionType.Action:
                      {
                        {{string.Join("\r            ", rule.OutputExpressions)}}
                        return RemoveThisElementFromList;
                      } break;
                  }
                } break;
            """))}}

              default: throw new NotImplementedException();
            }
            """;

          trace.Debug(Format([code]));
          var script = CSharpScript.Create<bool>(
            code,
            options: ScriptOptions.Default
              .WithReferences("System.Core", "ZoNo")
              .WithImports("System", "System.Linq", "ZoNo.Models"),
            globalsType: typeof(GlobalType<Input, Output>)
          );
          _evaluator = script.CreateDelegate();
        });
      }

      public async Task<bool> EvaluateRulesAsync(Input input, Output output)
      {
        using var trace = _traceFactory.CreateNew();
        trace.Debug(Format([input, output]));
        foreach (var rule in _rules)
        {
          trace.Debug(Format([rule.Description]));
          var condition = new GlobalType<Input, Output>() { RuleId = rule.Id.ToString(), Input = input, Output = default, ExecutionType = ExecutionType.Condition };
          var result = await _evaluator(condition);
          trace.Debug(Format([rule.InputExpression, result]));
          if (result)
          {
            var action = new GlobalType<Input, Output>() { RuleId = rule.Id.ToString(), Input = input, Output = output, ExecutionType = ExecutionType.Action };
            var removeThisElementFromList = await _evaluator(action);
            trace.Debug(Format([string.Join(Environment.NewLine, rule.OutputExpressions), removeThisElementFromList]));
            if (removeThisElementFromList)
            {
              return false;
            }
          }
        }
        return true;
      }
    }

    public async Task InitializeAsync()
    {
      using var trace = _traceFactory.CreateNew();
      await BuildAsync<object, object>([]);
    }

    public async Task<IRuleEvaluatorService<Input, Output>> BuildAsync<Input, Output>(IEnumerable<Rule> rules)
    {
      using var trace = _traceFactory.CreateNew();
      trace.Debug(Format([rules.Count()]));
      var ruleEvaluator = new RuleEvaluatorService<Input, Output>(rules, _traceFactory);
      await ruleEvaluator.InitializeAsync();
      return ruleEvaluator;
    }
  }
}
