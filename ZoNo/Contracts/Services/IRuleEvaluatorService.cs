namespace ZoNo.Contracts.Services
{
  public interface IRuleEvaluatorService<TInput, TOutput> : IDisposable
  {
    bool EvaluateRules(TInput input, TOutput output);
  }
}
