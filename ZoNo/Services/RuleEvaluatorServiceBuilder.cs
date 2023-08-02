using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using ZoNo.Contracts.Services;
using ZoNo.Models;

namespace ZoNo.Services
{
  public class RuleEvaluatorServiceBuilder : IRuleEvaluatorServiceBuilder
  {
    public class InputType<T>
    {
      public required string RuleId { get; set; }
      public required T Input { get; set; }
    }

    public class OutputType<T1, T2>
    {
      public required string RuleId { get; set; }
      public required T1 Input { get; set; }
      public required T2 Output { get; set; }
    }

    private class RuleEvaluatorService<Input, Output> : IRuleEvaluatorService<Input, Output>
    {
      private IList<Rule> _rules;
      private ScriptRunner<bool>? _inputEvaluator;
      private ScriptRunner<object>? _outputEvaluator;

      public RuleEvaluatorService(IList<Rule> rules)
      {
        _rules = rules.Select(rule => rule.Clone()).ToArray();
      }

      public async Task InitializeAsync()
      {
        await Task.Run(() =>
        {
          var inputCode = $$"""
                          switch (RuleId)
                          {
                            {{string.Join($"{Environment.NewLine}  ", _rules.Select(rule => $"case \"{rule.Id}\": {{ {rule.InputExpression} }} break;"))}}
                            default: throw new NotImplementedException();
                          }
                          """;
          var inputScript = CSharpScript.Create<bool>(
            inputCode,
            options: ScriptOptions.Default
              .WithReferences("System.Core")
              .WithImports("System", "System.Linq"),
            globalsType: typeof(InputType<Input>)
          );
          _inputEvaluator = inputScript.CreateDelegate();

          var outputCode = $$"""
                           switch (RuleId)
                           {
                             {{string.Join($"{Environment.NewLine}  ", _rules.Select(rule => $"case \"{rule.Id}\": {{ {string.Join(" ", rule.OutputExpressions)} }} break;"))}}
                             default: throw new NotImplementedException();
                           }
                           """;
          var outputScript = CSharpScript.Create<object>(
            outputCode,
            options: ScriptOptions.Default
              .WithReferences("Splitwise", "ZoNo")
              .WithImports("System", "System.Linq", "Splitwise.Models", "ZoNo.Models"),
            globalsType: typeof(OutputType<Input, Output>)
          );
          _outputEvaluator = outputScript.CreateDelegate();
        });
      }

      public async Task EvaluateRulesAsync(Input input, Output output)
      {
        foreach (var rule in _rules)
        {
          if (await _inputEvaluator!(new InputType<Input>() { RuleId = rule.Id.ToString(), Input = input }))
          {
            await _outputEvaluator!(new OutputType<Input, Output>() { RuleId = rule.Id.ToString(), Input = input, Output = output });
          }
        }
      }
    }

    public async Task<IRuleEvaluatorService<Input, Output>> BuildAsync<Input, Output>(IList<Rule> rules)
    {
      var ruleEvaluator = new RuleEvaluatorService<Input, Output>(rules);
      await ruleEvaluator.InitializeAsync();
      return ruleEvaluator;
    }
  }
}
