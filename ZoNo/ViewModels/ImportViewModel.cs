using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.WinUI.UI;
using ExcelDataReader;
using Microsoft.UI.Xaml;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Data;
using ZoNo.Contracts.Services;
using ZoNo.Models;
using ZoNo.Views;
using static System.TimeZoneInfo;

namespace ZoNo.ViewModels
{
  public partial class ColumnViewModel : ObservableObject
  {
    public ColumnHeader ColumnHeader { get; set; }

    [ObservableProperty]
    private bool _isVisible;
  }

  public enum ColumnHeader
  {
    TransactionTime,
    AccountingDate,
    Type,
    IncomeOutcome,
    PartnerName,
    PartnerAccountId,
    SpendingCategory,
    Description,
    AccountName,
    AccountId,
    Amount,
    Currency
  }

  public partial class ImportViewModel : ObservableRecipient
  {
    private const string SettingColumns = "Import_Columns";

    private readonly ILocalSettingsService _localSettingsService;

    private readonly ImmutableList<string> _excelHeaders = ImmutableList.Create<string>(
      "Tranzakció dátuma",
      "Könyvelés dátuma",
      "Típus",
      "Bejövő/Kimenő",
      "Partner neve",
      "Partner számlaszáma/azonosítója",
      "Költési kategória",
      "Közlemény",
      "Számla név",
      "Számla szám",
      "Összeg",
      "Pénznem"
    );

    private ObservableCollection<Transaction> _transactions = new ObservableCollection<Transaction>();

    public AdvancedCollectionView TransactionsView { get; } = new AdvancedCollectionView();

    [ObservableProperty]
    private List<ColumnViewModel>? _columns;

    public ImportViewModel(ILocalSettingsService localSettingsService)
    {
      _localSettingsService = localSettingsService;
      TransactionsView.Source = _transactions;
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
            await _localSettingsService.SaveSettingAsync(SettingColumns, Columns);
          }
        };
      }
    }

    public async Task LoadExcelDocument(string path)
    {
      var dataSet = await Task.Run(() =>
      {
        using var stream = File.Open(path, FileMode.Open, FileAccess.Read);
        using var reader = ExcelReaderFactory.CreateReader(stream);
        var result = reader.AsDataSet(new ExcelDataSetConfiguration()
        {
          ConfigureDataTable = (_) => new ExcelDataTableConfiguration()
          {
            UseHeaderRow = true
          }
        });
        return result;
      });

      // Check format
      if (dataSet.Tables.Count != 1)
        throw new Exception($"There are more than one worksheet!");
      if (dataSet.Tables[0].TableName != "Tranzakciók")
        throw new Exception($"Worksheet name is not 'Tranzakciók'");
      if (dataSet.Tables[0].Columns.Count != _excelHeaders.Count)
        throw new Exception($"Column count is not {_excelHeaders.Count}, it is {dataSet.Tables[0].Columns.Count}!");
      for (int i = 0; i < dataSet.Tables[0].Columns.Count; ++i)
      {
        if (dataSet.Tables[0].Columns[i].ColumnName != _excelHeaders[i])
        {
          throw new Exception($"Header[{i}] is not '{_excelHeaders[i]}', it is '{dataSet.Tables[0].Columns[i].ColumnName}'!");
        }
      }

      // Read transactions row by row
      foreach (DataRow row in dataSet.Tables[0].Rows)
      {
        await Task.Delay(1);
        _transactions.Add(new Transaction(
          TransactionTime: DateTime.Parse(row.Field<string>(0)!),
          AccountingDate: DateOnly.Parse(row.Field<string>(1)!),
          Type: row.Field<string>(2)!,
          IncomeOutcome: row.Field<string>(3)! == "Kimenő" ? IncomeOutcome.Outcome : IncomeOutcome.Income,
          PartnerName: row.Field<string>(4)!,
          PartnerAccountId: row.Field<string>(5)!,
          SpendingCategory: row.Field<string>(6)!,
          Description: row.Field<string>(7)!,
          AccountName: row.Field<string>(8)!,
          AccountId: row.Field<string>(9)!,
          Amount: row.Field<double>(10)!,
          Currency: Enum.Parse<Currency>(row.Field<string>(11)!)
        ));
      }
    }
  }
}