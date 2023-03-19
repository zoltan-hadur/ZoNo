using ZoNo.Models;

namespace ZoNo.Contracts.Services
{
  public interface IRulesService
  {
    Task LoadRulesAsync();
    Task AddRuleAsync(Rule rule);
    List<Rule> GetRules(RuleType type);
    Task UpdateRuleAsync(Rule rule);
    Task DeleteRuleAsync(Rule rule);
  }
}
