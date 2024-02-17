using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using ZoNo.Contracts.Services;
using ZoNo.Models;

namespace ZoNo.Services
{
  public class RuleExpressionSyntaxCheckerService : IRuleExpressionSyntaxCheckerService
  {
    public async Task<(bool IsValid, string ErrorMessage)> CheckSyntaxAsync<Input>(string inputExpression) =>
      await CheckSyntaxAsync<Input, Input, bool>(inputExpression);

    public async Task<(bool IsValid, string ErrorMessage)> CheckSyntaxAsync<Input, Output>(string outputExpression) =>
      await CheckSyntaxAsync<Input, Output, object>(outputExpression);

    private async Task<(bool IsValid, string ErrorMessage)> CheckSyntaxAsync<TInput, TOutput, TScriptReturn>(string expression)
    {
      return await Task.Run(() =>
      {
        var script = CSharpScript.Create<TScriptReturn>(
          expression,
          options: ScriptOptions.Default
            .WithReferences("System.Core", "ZoNo")
            .WithImports("System", "System.Linq", "ZoNo.Models"),
          globalsType: typeof(GlobalType<TInput, TOutput>)
        );
        var compilationResult = script.Compile();
        return (compilationResult.IsEmpty, string.Join(Environment.NewLine, compilationResult.Select(x => x.ToString())));
      });
    }
  }
}
