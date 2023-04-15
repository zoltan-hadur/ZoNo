namespace ZoNo.Contracts.Services
{
  public interface IRuleEvaluatorService<Input, Output>
  {
    Task EvaluateRulesAsync(Input input, Output output);
  }
}
