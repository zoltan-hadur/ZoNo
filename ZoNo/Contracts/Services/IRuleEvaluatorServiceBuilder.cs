using ZoNo.Models;

namespace ZoNo.Contracts.Services
{
  public interface IRuleEvaluatorServiceBuilder
  {
    Task InitializeAsync();
    Task<IRuleEvaluatorService<Input, Output>> BuildAsync<Input, Output>(IEnumerable<Rule> rules);
  }
}
