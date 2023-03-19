using Microsoft.CodeAnalysis.CSharp.Scripting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZoNo.Contracts.Services;
using ZoNo.Models;

namespace ZoNo.Services
{
  public class RuleEvaluatorService : IRuleEvaluatorService
  {
    public async Task EvaluateRulesAsync<Input, Output>(IEnumerable<Rule> rules, Input input, Output output)
    {
      foreach (var rule in rules)
      {
        var _inputEvaluator = CSharpScript.Create<bool>(rule.InputExpression, globalsType: typeof(Input)).CreateDelegate();
        if (await _inputEvaluator(input))
        {
          foreach (var outputExpression in rule.OutputExpressions)
          {
            var _outputEvaluator = CSharpScript.Create<object>(outputExpression, globalsType: typeof(Output)).CreateDelegate();
            await _outputEvaluator(output);
          }
        }
      }
    }
  }
}
