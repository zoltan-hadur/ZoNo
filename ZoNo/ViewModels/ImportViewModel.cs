using CommunityToolkit.Mvvm.ComponentModel;
using ExcelDataReader;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Data;
using ZoNo.Models;

namespace ZoNo.ViewModels
{
  public partial class ImportViewModel : ObservableRecipient
  {
    private readonly ImmutableList<string> _headers = ImmutableList.Create<string>(
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

    [ObservableProperty]
    private ObservableCollection<Transaction> _transactions = new();

    public ImportViewModel()
    {

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
      if (dataSet.Tables[0].Columns.Count != _headers.Count)
        throw new Exception($"Column count is not {_headers.Count}, it is {dataSet.Tables[0].Columns.Count}!");
      for (int i = 0; i < dataSet.Tables[0].Columns.Count; ++i)
      {
        if (dataSet.Tables[0].Columns[i].ColumnName != _headers[i])
        {
          throw new Exception($"Header[{i}] is not '{_headers[i]}', it is '{dataSet.Tables[0].Columns[i].ColumnName}'!");
        }
      }

      // Read transactions row by row
      foreach (DataRow row in dataSet.Tables[0].Rows)
      {
        await Task.Delay(1);
        Transactions.Add(new Transaction(
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