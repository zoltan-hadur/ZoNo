using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.WinUI.UI;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Common;
using ZoNo.Contracts.Services;
using ZoNo.Models;
using ZoNo.Services;

namespace ZoNo.ViewModels.Import
{
  public partial class TransactionsViewModel : ObservableObject
  {
    private readonly ILocalSettingsService _localSettingsService;
    private readonly IExcelLoader _excelLoader;
    private readonly IRulesService _rulesService;
    private readonly IRuleEvaluatorService _ruleEvaluatorService;

    private ObservableCollection<Transaction> _transactions = new ObservableCollection<Transaction>();

    public AdvancedCollectionView TransactionsView { get; } = new AdvancedCollectionView();

    public Dictionary<string, ColumnViewModel>? Columns { get; private set; } = null;

    public TransactionsViewModel(ILocalSettingsService localSettingsService, IExcelLoader excelLoader, IRulesService rulesService, IRuleEvaluatorService ruleEvaluatorService)
    {
      _localSettingsService = localSettingsService;
      _excelLoader = excelLoader;
      _rulesService = rulesService;
      _ruleEvaluatorService = ruleEvaluatorService;
      TransactionsView.Source = _transactions;
    }

    public async Task Load()
    {
      Columns = new Dictionary<string, ColumnViewModel>()
      {
        { nameof(ColumnHeader.TransactionTime ), new() { ColumnHeader = ColumnHeader.TransactionTime , IsVisible = true  } },
        { nameof(ColumnHeader.AccountingDate  ), new() { ColumnHeader = ColumnHeader.AccountingDate  , IsVisible = false } },
        { nameof(ColumnHeader.Type            ), new() { ColumnHeader = ColumnHeader.Type            , IsVisible = false } },
        { nameof(ColumnHeader.IncomeOutcome   ), new() { ColumnHeader = ColumnHeader.IncomeOutcome   , IsVisible = false } },
        { nameof(ColumnHeader.PartnerName     ), new() { ColumnHeader = ColumnHeader.PartnerName     , IsVisible = true  } },
        { nameof(ColumnHeader.PartnerAccountId), new() { ColumnHeader = ColumnHeader.PartnerAccountId, IsVisible = false } },
        { nameof(ColumnHeader.SpendingCategory), new() { ColumnHeader = ColumnHeader.SpendingCategory, IsVisible = false } },
        { nameof(ColumnHeader.Description     ), new() { ColumnHeader = ColumnHeader.Description     , IsVisible = true  } },
        { nameof(ColumnHeader.AccountName     ), new() { ColumnHeader = ColumnHeader.AccountName     , IsVisible = false } },
        { nameof(ColumnHeader.AccountId       ), new() { ColumnHeader = ColumnHeader.AccountId       , IsVisible = true  } },
        { nameof(ColumnHeader.Amount          ), new() { ColumnHeader = ColumnHeader.Amount          , IsVisible = true  } },
        { nameof(ColumnHeader.Currency        ), new() { ColumnHeader = ColumnHeader.Currency        , IsVisible = false } }
      };

      foreach (var (_, column) in Columns)
      {
        var isColumnVisible = await _localSettingsService.ReadSettingAsync<bool?>(SettingColumnIsVisible(column.ColumnHeader));
        if (isColumnVisible.HasValue)
        {
          column.IsVisible = isColumnVisible.Value;
        }
      }

      foreach (var (_, column) in Columns)
      {
        column.PropertyChanged += Column_PropertyChanged;
      }

      OnPropertyChanged(nameof(Columns));
    }

    public async Task LoadExcelDocument(string path)
    {
      foreach (var transaction in await _excelLoader.LoadAsync(path))
      {
        await _ruleEvaluatorService.EvaluateRulesAsync(_rulesService.GetRules(RuleType.Import), transaction, transaction);
        _transactions.Add(transaction);
      }
    }

    private string SettingColumnIsVisible(ColumnHeader columnHeader) => $"Import_Columns_{columnHeader}_IsVisible";

    private async void Column_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
      if (sender is ColumnViewModel senderColumn && e.PropertyName == nameof(ColumnViewModel.IsVisible))
      {
        var visibleColumnCount = Columns!.Count(column => column.Value.IsVisible);
        foreach (var (_, column) in Columns!)
        {
          column.IsEnabled = !(column.IsVisible && visibleColumnCount == 1);
        }
        await _localSettingsService.SaveSettingAsync(SettingColumnIsVisible(senderColumn.ColumnHeader), senderColumn.IsVisible);
      }
    }
  }
}
