using ZoNo.Models;

namespace ZoNo.Contracts.Services
{
  public interface IRuleEvaluatorServiceBuilder
  {
    Task<IRuleEvaluatorService<Input, Output>> BuildAsync<Input, Output>(IList<Rule> rules);
  }
}
