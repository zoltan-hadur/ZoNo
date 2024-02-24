using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.WinUI.UI;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Tracer;
using Tracer.Contracts;
using ZoNo.Contracts.Services;
using ZoNo.Helpers;
using ZoNo.Models;

namespace ZoNo.ViewModels
{
  public partial class TransactionsViewModel(
    ILocalSettingsService localSettingsService,
    IExcelDocumentLoaderService excelDocumentLoaderService,
    ITransactionProcessorService transactionProcessorService,
    ITraceFactory traceFactory) : ObservableObject
  {
    private readonly ILocalSettingsService _localSettingsService = localSettingsService;
    private readonly IExcelDocumentLoaderService _excelDocumentLoaderService = excelDocumentLoaderService;
    private readonly ITransactionProcessorService _transactionProcessorService = transactionProcessorService;
    private readonly ITraceFactory _traceFactory = traceFactory;

    private bool _isLoaded = false;
    private readonly SemaphoreSlim _guard = new(initialCount: 1, maxCount: 1);

    public AdvancedCollectionView TransactionsView { get; } = new AdvancedCollectionView(new ObservableCollection<Transaction>());

    public Dictionary<string, ColumnViewModel> Columns { get; private set; } = null;

    public async Task LoadAsync()
    {
      using var trace = _traceFactory.CreateNew();
      using var guard = await LockGuard.CreateAsync(_guard, TimeSpan.Zero);
      trace.Debug(Format([_isLoaded]));
      if (_isLoaded) return;

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

      foreach (var column in Columns.Values)
      {
        var isColumnVisible = await _localSettingsService.ReadSettingAsync<bool?>(SettingColumnIsVisible(column.ColumnHeader));
        if (isColumnVisible.HasValue)
        {
          column.IsVisible = isColumnVisible.Value;
        }
        trace.Debug(Format([column.ColumnHeader, column.IsVisible]));
      }

      foreach (var column in Columns.Values)
      {
        column.PropertyChanged += Column_PropertyChanged;
      }

      OnPropertyChanged(nameof(Columns));

      _isLoaded = true;
    }

    [RelayCommand]
    private async Task LoadExcelDocuments(IList<string> paths)
    {
      using var trace = _traceFactory.CreateNew();
      using var guard = await LockGuard.CreateAsync(_guard, TimeSpan.FromSeconds(5));

      await _transactionProcessorService.InitializeAsync();

      var transactions = (await Task
        .WhenAll(paths.Select(path => TraceFactory.HandleAsAsyncVoid(() => _excelDocumentLoaderService.LoadDocumentAsync(path)))))
        .SelectMany(transactions => transactions);

      foreach (var transaction in transactions)
      {
        await _transactionProcessorService.ProcessAsync(transaction);
      }
    }

    [RelayCommand]
    private async Task DeleteTransactionsAsync(List<Transaction> transactions)
    {
      using var trace = _traceFactory.CreateNew();
      using var guard = await LockGuard.CreateAsync(_guard, TimeSpan.FromSeconds(5));

      var deferRefresh = transactions.Count > 30 ? TransactionsView.DeferRefresh() : null;
      try
      {
        foreach (var transaction in transactions)
        {
          try
          {
            TransactionsView.Source.Remove(transaction);
          }
          catch (ArgumentOutOfRangeException)
          {
            // When deleting last item, there is an exception
            TransactionsView.Refresh();
          }
        }
      }
      finally
      {
        deferRefresh?.Dispose();
      }
    }

    private static string SettingColumnIsVisible(ColumnHeader columnHeader) => $"Import_Columns_{columnHeader}_IsVisible";

    private async void Column_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      using var trace = _traceFactory.CreateNew();
      if (sender is ColumnViewModel senderColumn && e.PropertyName == nameof(ColumnViewModel.IsVisible))
      {
        var visibleColumnCount = Columns.Values.Count(column => column.IsVisible);
        trace.Debug(Format([senderColumn.ColumnHeader, senderColumn.IsVisible, visibleColumnCount]));
        foreach (var column in Columns.Values)
        {
          column.IsEnabled = !(column.IsVisible && visibleColumnCount == 1);
          trace.Debug(Format([column.ColumnHeader, column.IsEnabled]));
        }
        await _localSettingsService.SaveSettingAsync(SettingColumnIsVisible(senderColumn.ColumnHeader), senderColumn.IsVisible);
      }
    }
  }
}
