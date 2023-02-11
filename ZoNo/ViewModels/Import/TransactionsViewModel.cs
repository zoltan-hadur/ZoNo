using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.WinUI.UI;
using System.Collections.ObjectModel;
using ZoNo.Contracts.Services;
using ZoNo.Models;

namespace ZoNo.ViewModels.Import
{
  public partial class TransactionsViewModel : ObservableObject
  {
    private const string SettingColumns = "Import_Columns";

    private readonly ILocalSettingsService _localSettingsService;
    private readonly IExcelLoader _excelLoader;

    private ObservableCollection<Transaction> _transactions = new ObservableCollection<Transaction>();

    public AdvancedCollectionView TransactionsView { get; } = new AdvancedCollectionView();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(VisibleColumnCount))]
    private List<ColumnViewModel>? _columns;

    public int VisibleColumnCount => Columns?.Count(column => column.IsVisible) ?? 0;

    public TransactionsViewModel(ILocalSettingsService localSettingsService, IExcelLoader excelLoader)
    {
      _localSettingsService = localSettingsService;
      _excelLoader = excelLoader;
      TransactionsView.Source = _transactions;

      PropertyChanged += (s, e) =>
      {
        if (e.PropertyName == nameof(VisibleColumnCount))
        {
          var count = VisibleColumnCount;
          foreach (var column in Columns!)
          {
            column.IsEnabled = !(column.IsVisible && count == 1);
          }
        }
      };
    }

    public async Task Load()
    {
      Columns = await _localSettingsService.ReadSettingAsync<List<ColumnViewModel>>(SettingColumns) ??
        new List<ColumnViewModel>()
        {
          new ColumnViewModel() { ColumnHeader = ColumnHeader.TransactionTime , IsVisible = true  },
          new ColumnViewModel() { ColumnHeader = ColumnHeader.AccountingDate  , IsVisible = false },
          new ColumnViewModel() { ColumnHeader = ColumnHeader.Type            , IsVisible = false },
          new ColumnViewModel() { ColumnHeader = ColumnHeader.IncomeOutcome   , IsVisible = false },
          new ColumnViewModel() { ColumnHeader = ColumnHeader.PartnerName     , IsVisible = true  },
          new ColumnViewModel() { ColumnHeader = ColumnHeader.PartnerAccountId, IsVisible = false },
          new ColumnViewModel() { ColumnHeader = ColumnHeader.SpendingCategory, IsVisible = false },
          new ColumnViewModel() { ColumnHeader = ColumnHeader.Description     , IsVisible = true  },
          new ColumnViewModel() { ColumnHeader = ColumnHeader.AccountName     , IsVisible = false },
          new ColumnViewModel() { ColumnHeader = ColumnHeader.AccountId       , IsVisible = true  },
          new ColumnViewModel() { ColumnHeader = ColumnHeader.Amount          , IsVisible = true  },
          new ColumnViewModel() { ColumnHeader = ColumnHeader.Currency        , IsVisible = false }
        };

      foreach (var column in Columns)
      {
        column.PropertyChanged += async (s, e) =>
        {
          if (e.PropertyName == nameof(ColumnViewModel.IsVisible))
          {
            OnPropertyChanged(nameof(VisibleColumnCount));
            await _localSettingsService.SaveSettingAsync(SettingColumns, Columns);
          }
        };
      }
    }

    public async Task LoadExcelDocument(string path)
    {
      foreach (var transaction in await _excelLoader.LoadAsync(path))
      {
        _transactions.Add(transaction);
      }
    }
  }
}
