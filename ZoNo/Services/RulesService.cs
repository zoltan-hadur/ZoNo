using Windows.Storage;
using ZoNo.Contracts.Services;
using ZoNo.Helpers;
using ZoNo.Models;

namespace ZoNo.Services
{
  public class RulesService : IRulesService
  {
    private readonly Dictionary<RuleType, Rule[]> _rules = [];

    public async Task InitializeAsync()
    {
      foreach (var type in Enum.GetValues<RuleType>())
      {
        _rules[type] = await ApplicationData.Current.LocalFolder.ReadAsync<Rule[]>($"Rules_{type}") ?? [];
      }

      // Handle previous versions of rules
      if (_rules[RuleType.Transaction].Length == 0)
      {
        _rules[RuleType.Transaction] = await ApplicationData.Current.LocalFolder.ReadAsync<Rule[]>("Rules_Import") ?? [];
      }
      if (_rules[RuleType.Expense].Length == 0)
      {
        _rules[RuleType.Expense] = await ApplicationData.Current.LocalFolder.ReadAsync<Rule[]>("Rules_Splitwise") ?? [];
      }
    }

    public IReadOnlyCollection<Rule> GetRules(RuleType type)
    {
      return _rules[type].AsReadOnly();
    }

    public async Task SaveRulesAsync(RuleType type, IEnumerable<Rule> rules)
    {
      _rules[type] = rules.Select(rule => rule.Clone()).ToArray();
      await ApplicationData.Current.LocalFolder.SaveAsync($"Rules_{type}", _rules[type]);
    }
  }
}
