using Tracer.Contracts;
using Windows.Storage;
using ZoNo.Contracts.Services;
using ZoNo.Helpers;
using ZoNo.Models;

namespace ZoNo.Services
{
  public class RulesService(
    ITraceFactory _traceFactory) : IRulesService
  {
    private readonly Dictionary<RuleType, Rule[]> _rules = [];

    public async Task InitializeAsync()
    {
      using var trace = _traceFactory.CreateNew();

      foreach (var type in Enum.GetValues<RuleType>())
      {
        _rules[type] = await ApplicationData.Current.LocalFolder.ReadAsync<Rule[]>($"Rules_{type}") ?? [];
        trace.Debug(Format([type, _rules[type].Length]));
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
      using var trace = _traceFactory.CreateNew();
      trace.Debug(Format([type]));
      return _rules[type].AsReadOnly();
    }

    public async Task SaveRulesAsync(RuleType type, IEnumerable<Rule> rules)
    {
      using var trace = _traceFactory.CreateNew();
      _rules[type] = rules.Select(rule => rule.Clone()).ToArray();
      trace.Debug(Format([type, _rules[type].Length]));
      await ApplicationData.Current.LocalFolder.SaveAsync($"Rules_{type}", _rules[type]);
    }
  }
}
