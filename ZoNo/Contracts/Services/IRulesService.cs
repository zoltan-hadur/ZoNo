using ZoNo.Models;

namespace ZoNo.Contracts.Services
{
  public interface IRulesService
  {
    Task LoadRulesAsync();
    Task<IList<Rule>> GetRulesAsync(RuleType type);
    Task SaveRulesAsync(RuleType type, IList<Rule> rules);
  }
}
