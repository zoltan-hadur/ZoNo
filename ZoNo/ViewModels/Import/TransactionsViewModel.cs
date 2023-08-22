using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.WinUI.UI;
using System.Collections.ObjectModel;
using System.ComponentModel;
using ZoNo.Contracts.Services;
using ZoNo.Models;

namespace ZoNo.ViewModels.Import
{
  public partial class TransactionsViewModel : ObservableObject
  {
    private readonly ILocalSettingsService _localSettingsService;
    private readonly IExcelDocumentLoader _excelDocumentLoader;
    private readonly IRulesService _rulesService;
    private readonly IRuleEvaluatorServiceBuilder _ruleEvaluatorServiceBuilder;

    private ObservableCollection<Transaction> _transactions = new ObservableCollection<Transaction>();

    public AdvancedCollectionView TransactionsView { get; } = new AdvancedCollectionView();

    public Dictionary<string, ColumnViewModel>? Columns { get; private set; } = null;

    public event EventHandler? LoadExcelDocumentsStarted;
    public event EventHandler? LoadExcelDocumentsFinished;

    public TransactionsViewModel(
      ILocalSettingsService localSettingsService,
      IExcelDocumentLoader excelDocumentLoader,
      IRulesService rulesService,
      IRuleEvaluatorServiceBuilder ruleEvaluatorService)
    {
      _localSettingsService = localSettingsService;
      _excelDocumentLoader = excelDocumentLoader;
      _rulesService = rulesService;
      _ruleEvaluatorServiceBuilder = ruleEvaluatorService;
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

    [RelayCommand]
    private async Task LoadExcelDocuments(IList<string> paths)
    {
      LoadExcelDocumentsStarted?.Invoke(this, EventArgs.Empty);

      var rules = await _rulesService.GetRulesAsync(RuleType.Import);
      var ruleEvaluatorService = await _ruleEvaluatorServiceBuilder.BuildAsync<Transaction, Transaction>(rules);
      var transactions = new List<Transaction>();
      foreach (var path in paths)
      {
        foreach (var transaction in await _excelDocumentLoader.LoadAsync(path))
        {
          transactions.Add(transaction);
        }
      }
      var deferRefresh = transactions.Count > 30 ? TransactionsView.DeferRefresh() : null;
      try
      {
        foreach (var transaction in transactions)
        {
          var result = await ruleEvaluatorService.EvaluateRulesAsync(input: transaction, output: transaction);
          if (!result.RemoveThisElementFromList)
          {
            TransactionsView.Add(transaction);
          }
        }
      }
      finally
      {
        deferRefresh?.Dispose();
      }

      LoadExcelDocumentsFinished?.Invoke(this, EventArgs.Empty);
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
