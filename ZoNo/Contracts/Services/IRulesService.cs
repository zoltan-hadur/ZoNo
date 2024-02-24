using ZoNo.Models;

namespace ZoNo.Contracts.Services
{
  public interface IRulesService
  {
    Task InitializeAsync();
    IReadOnlyCollection<Rule> GetRules(RuleType type);
    Task SaveRulesAsync(RuleType type, IEnumerable<Rule> rules);
  }
}
