using ZoNo.Contracts.Services;
using ZoNo.Models;

namespace ZoNo.Services
{
  public class RulesService : IRulesService, IImportRulesService, ISplitwiseRulesService
  {
    private readonly ILocalSettingsService _localSettingsService;
    private readonly string SettingName;

    private Rule[]? _rules;
    private SemaphoreSlim _isLoaded = new SemaphoreSlim(initialCount: 0, maxCount: 1);

    public RulesService(ILocalSettingsService localSettingsService, string settingName)
    {
      _localSettingsService = localSettingsService;
      SettingName = settingName;
    }

    public async Task LoadRulesAsync()
    {
      _rules = await _localSettingsService.ReadSettingAsync<Rule[]>(SettingName) ?? new Rule[]
      {
        new Rule { InputExpression = "AccountId == \"0\"", OutputExpressions = new List<string>{ "AccountId = \"Asd\"" } },
        new Rule { InputExpression = "Amount > 0", OutputExpressions = new List<string>{ "AccountId = AccountId + \" 100\"" } },
        new Rule { InputExpression = "Amount < 0", OutputExpressions = new List<string>{ "AccountId = AccountId + \" \" + TransactionTime.Year" } },
        new Rule { InputExpression = "AccountId == \"0\"", OutputExpressions = new List<string>{ "AccountId = \"Asd\"" } },
        new Rule { InputExpression = "Amount > 0", OutputExpressions = new List<string>{ "AccountId = AccountId + \" 100\"", "AccountId = AccountId + \" 100\"" } },
        new Rule { InputExpression = "Amount < 0", OutputExpressions = new List<string>{ "AccountId = AccountId + \" \" + TransactionTime.Year" } },
        new Rule { InputExpression = "Amount < 0", OutputExpressions = new List<string>{ "AccountId = AccountId + \" \" + TransactionTime.Year" } },
        new Rule { InputExpression = "Amount < 0", OutputExpressions = new List<string>{ "AccountId = AccountId + \" \" + TransactionTime.Year" } },
        new Rule { InputExpression = "Amount < 0", OutputExpressions = new List<string>{ "AccountId = AccountId + \" \" + TransactionTime.Year" } },
        new Rule { InputExpression = "Amount < 0", OutputExpressions = new List<string>{ "AccountId = AccountId + \" \" + TransactionTime.Year" } },
        new Rule { InputExpression = "Amount < 0", OutputExpressions = new List<string>{ "AccountId = AccountId + \" \" + TransactionTime.Year" } },
        new Rule { InputExpression = "Amount < 0", OutputExpressions = new List<string>{ "AccountId = AccountId + \" \" + TransactionTime.Year" } },
        new Rule { InputExpression = "Amount < 0", OutputExpressions = new List<string>{ "AccountId = AccountId + \" \" + TransactionTime.Year" } },
        new Rule { InputExpression = "Amount < 0", OutputExpressions = new List<string>{ "AccountId = AccountId + \" \" + TransactionTime.Year" } }
      };
      _isLoaded.Release();
    }

    public async Task<IEnumerable<Rule>> GetRulesAsync()
    {
      await Loading();
      return _rules!.Select(rule => rule.Clone());
    }

    public async Task SaveRulesAsync(IEnumerable<Rule> rules)
    {
      await Loading();
      _rules = rules.Select(rule => rule.Clone()).ToArray();
      await _localSettingsService.SaveSettingAsync(SettingName, _rules);
    }

    private async Task Loading()
    {
      if (await _isLoaded.WaitAsync(TimeSpan.FromSeconds(5)) == false)
      {
        throw new InvalidOperationException("Loading of rules has timed out!");
      }
      _isLoaded.Release();
    }
  }
}
