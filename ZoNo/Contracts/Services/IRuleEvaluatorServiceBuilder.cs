using ZoNo.Models;

namespace ZoNo.Contracts.Services
{
  public interface IRuleEvaluatorServiceBuilder
  {
    Task InitializeAsync();
    Task<IRuleEvaluatorService<TInput, TOutput>> BuildAsync<TInput, TOutput>(IEnumerable<Rule> rules);
  }
}
