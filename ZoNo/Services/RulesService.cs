using ZoNo.Contracts.Services;
using ZoNo.Models;

namespace ZoNo.Services
{
  public class RulesService : IRulesService
  {
    private const string SettingRules = "Rules";

    private readonly ILocalSettingsService _localSettingsService;

    protected List<Rule> _rules = new List<Rule>();
    private bool _isLoaded = false;

    public RulesService(ILocalSettingsService localSettingsService)
    {
      _localSettingsService = localSettingsService;
    }

    private async Task OnSaveChangesAsync()
    {
      await _localSettingsService.SaveSettingAsync(SettingRules, _rules);
    }

    private async Task OnLoadRulesAsync()
    {
      _rules = await _localSettingsService.ReadSettingAsync<List<Rule>>(SettingRules) ?? new List<Rule>()
      {
        new Rule { Type = RuleType.Import, InputExpression = "AccountId == \"0\"", OutputExpressions = new List<string>{ "AccountId = \"Asd\"" } },
        new Rule { Type = RuleType.Import, InputExpression = "Amount > 0", OutputExpressions = new List<string>{ "AccountId = AccountId + \" 100\"" } },
        new Rule { Type = RuleType.Import, InputExpression = "Amount < 0", OutputExpressions = new List<string>{ "AccountId = AccountId + \" \" + TransactionTime.Year" } },
        new Rule { Type = RuleType.Splitwise, InputExpression = "AccountId == \"0\"", OutputExpressions = new List<string>{ "AccountId = \"Asd\"" } },
        new Rule { Type = RuleType.Splitwise, InputExpression = "Amount > 0", OutputExpressions = new List<string>{ "AccountId = AccountId + \" 100\"", "AccountId = AccountId + \" 100\"" } },
        new Rule { Type = RuleType.Splitwise, InputExpression = "Amount < 0", OutputExpressions = new List<string>{ "AccountId = AccountId + \" \" + TransactionTime.Year" } },
        new Rule { Type = RuleType.Splitwise, InputExpression = "Amount < 0", OutputExpressions = new List<string>{ "AccountId = AccountId + \" \" + TransactionTime.Year" } },
        new Rule { Type = RuleType.Splitwise, InputExpression = "Amount < 0", OutputExpressions = new List<string>{ "AccountId = AccountId + \" \" + TransactionTime.Year" } },
        new Rule { Type = RuleType.Splitwise, InputExpression = "Amount < 0", OutputExpressions = new List<string>{ "AccountId = AccountId + \" \" + TransactionTime.Year" } },
        new Rule { Type = RuleType.Splitwise, InputExpression = "Amount < 0", OutputExpressions = new List<string>{ "AccountId = AccountId + \" \" + TransactionTime.Year" } },
        new Rule { Type = RuleType.Splitwise, InputExpression = "Amount < 0", OutputExpressions = new List<string>{ "AccountId = AccountId + \" \" + TransactionTime.Year" } },
        new Rule { Type = RuleType.Splitwise, InputExpression = "Amount < 0", OutputExpressions = new List<string>{ "AccountId = AccountId + \" \" + TransactionTime.Year" } },
        new Rule { Type = RuleType.Splitwise, InputExpression = "Amount < 0", OutputExpressions = new List<string>{ "AccountId = AccountId + \" \" + TransactionTime.Year" } },
        new Rule { Type = RuleType.Splitwise, InputExpression = "Amount < 0", OutputExpressions = new List<string>{ "AccountId = AccountId + \" \" + TransactionTime.Year" } }
      };
    }

    public async Task LoadRulesAsync()
    {
      await OnLoadRulesAsync();
      _isLoaded = true;
    }

    public async Task AddRuleAsync(Rule rule)
    {
      if (!_isLoaded)
      {
        throw new InvalidOperationException("The rules has not been loaded yet!");
      }
      _rules.Add(rule.Clone());
      await OnSaveChangesAsync();
    }

    public List<Rule> GetRules(RuleType type)
    {
      if (!_isLoaded)
      {
        throw new InvalidOperationException("The rules has not been loaded yet!");
      }
      return _rules.Where(rule => rule.Type == type).Select(rule => rule.Clone()).ToList();
    }

    public async Task UpdateRuleAsync(Rule rule)
    {
      if (!_isLoaded)
      {
        throw new InvalidOperationException("The rules has not been loaded yet!");
      }
      var storedRule = _rules.Single(storedRule => storedRule.Id == rule.Id);
      storedRule.Type = rule.Type;
      storedRule.InputExpression = rule.InputExpression;
      storedRule.OutputExpressions = rule.OutputExpressions.ToList();
      await OnSaveChangesAsync();
    }

    public async Task DeleteRuleAsync(Rule rule)
    {
      if (!_isLoaded)
      {
        throw new InvalidOperationException("The rules has not been loaded yet!");
      }
      var storedRule = _rules.Single(storedRule => storedRule == rule);
      _rules.Remove(storedRule);
      await OnSaveChangesAsync();
    }
  }
}
