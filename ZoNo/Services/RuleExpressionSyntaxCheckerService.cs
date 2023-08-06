using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using ZoNo.Contracts.Services;
using static ZoNo.Services.RuleEvaluatorServiceBuilder;

namespace ZoNo.Services
{
  public class RuleExpressionSyntaxCheckerService : IRuleExpressionSyntaxCheckerService
  {
    public async Task<(bool IsValid, string ErrorMessage)> TryCheckSyntaxAsync<Input>(string inputExpression)
    {
      return await Task.Run(() =>
      {
        var script = CSharpScript.Create<bool>(
          inputExpression,
          options: ScriptOptions.Default
            .WithReferences("System.Core")
            .WithImports("System", "System.Linq"),
          globalsType: typeof(InputType<Input>)
        );
        var compilationResult = script.Compile();
        return (compilationResult.IsEmpty, string.Join(Environment.NewLine, compilationResult.Select(x => x.ToString())));
      });
    }

    public async Task<(bool IsValid, string ErrorMessage)> TryCheckSyntaxAsync<Input, Output>(string outputExpression)
    {
      return await Task.Run(() =>
      {
        var script = CSharpScript.Create<object>(
          outputExpression,
          options: ScriptOptions.Default
            .WithReferences("Splitwise", "ZoNo")
            .WithImports("System", "System.Linq", "Splitwise.Models", "ZoNo.Models"),
          globalsType: typeof(OutputType<Input, Output>)
        );
        var compilationResult = script.Compile();
        return (compilationResult.IsEmpty, string.Join(Environment.NewLine, compilationResult.Select(x => x.ToString())));
      });
    }
  }
}
