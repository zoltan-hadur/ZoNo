using Windows.Storage;
using ZoNo.Contracts.Services;
using ZoNo.Helpers;
using ZoNo.Models;

namespace ZoNo.Services
{
  public class RulesService : IRulesService
  {
    private Dictionary<RuleType, Rule[]> _rules = new Dictionary<RuleType, Rule[]>();
    private SemaphoreSlim _guard = new SemaphoreSlim(initialCount: 1, maxCount: 1);

    public RulesService()
    {

    }

    public async Task LoadRulesAsync()
    {
      using var guard = await LockGuard.CreateAsync(_guard, TimeSpan.Zero);
      foreach (var type in Enum.GetValues<RuleType>())
      {
        _rules[type] = await ApplicationData.Current.LocalFolder.ReadAsync<Rule[]>($"Rules_{type}") ?? Array.Empty<Rule>();
      }
    }

    public async Task<IList<Rule>> GetRulesAsync(RuleType type)
    {
      using var guard = await LockGuard.CreateAsync(_guard, TimeSpan.FromSeconds(5));
      return _rules[type].Select(rule => rule.Clone()).ToArray();
    }

    public async Task SaveRulesAsync(RuleType type, IList<Rule> rules)
    {
      using var guard = await LockGuard.CreateAsync(_guard, TimeSpan.FromSeconds(5));
      _rules[type] = rules.Select(rule => rule.Clone()).ToArray();
      await ApplicationData.Current.LocalFolder.SaveAsync($"Rules_{type}", _rules[type]);
    }
  }
}
