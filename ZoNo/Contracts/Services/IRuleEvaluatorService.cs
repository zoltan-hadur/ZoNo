namespace ZoNo.Contracts.Services
{
  public interface IRuleEvaluatorService<Input, Output>
  {
    Task<bool> EvaluateRulesAsync(Input input, Output output);
  }
}
