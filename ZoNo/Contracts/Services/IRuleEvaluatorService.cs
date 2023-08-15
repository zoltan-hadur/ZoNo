using static ZoNo.Services.RuleEvaluatorServiceBuilder;

namespace ZoNo.Contracts.Services
{
  public interface IRuleEvaluatorService<Input, Output>
  {
    Task<EvaluationResult> EvaluateRulesAsync(Input input, Output output);
  }
}
