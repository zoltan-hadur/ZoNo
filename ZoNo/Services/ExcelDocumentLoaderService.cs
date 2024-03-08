using ExcelDataReader;
using System.Collections.Immutable;
using System.Data;
using Tracer.Contracts;
using ZoNo.Contracts.Services;
using ZoNo.Models;

namespace ZoNo.Services
{
  public class ExcelDocumentLoaderService(
    ITraceFactory _traceFactory) : IExcelDocumentLoaderService
  {
    private const int _expectedWorksheetCount = 1;
    private const string _expectedWorksheetName = "Tranzakciók";
    private readonly ImmutableArray<string> _expectedHeaders =
    [
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
    ];

    public async Task<IList<Transaction>> LoadDocumentAsync(string path)
    {
      using var trace = _traceFactory.CreateNew();
      trace.Debug(Format([path]));
      return await Task.Run(() =>
      {
        var dataSet = GetDataSet(path);
        Validate(dataSet);
        trace.Info("Transform data rows to transactions");
        return dataSet.Tables[0].Rows.Cast<DataRow>().Select(TransformDataRowToTransaction).ToArray();
      });
    }

    private DataSet GetDataSet(string path)
    {
      using var trace = _traceFactory.CreateNew();

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
    }

    private void Validate(DataSet dataSet)
    {
      using var trace = _traceFactory.CreateNew();

      var actualWorksheetCount = dataSet.Tables.Count;
      trace.Debug(Format([actualWorksheetCount]));
      if (actualWorksheetCount != _expectedWorksheetCount)
      {
        throw new Exception($"Worksheet count is not {_expectedWorksheetCount}, it is {actualWorksheetCount}!");
      }

      var actualWorksheetName = dataSet.Tables[0].TableName;
      trace.Debug(Format([actualWorksheetName]));
      if (actualWorksheetName != _expectedWorksheetName)
      {
        throw new Exception($"Worksheet name is not {_expectedWorksheetName}, it is {actualWorksheetName}");
      }

      var actualHeaderCount = dataSet.Tables[0].Columns.Count;
      trace.Debug(Format([actualHeaderCount]));
      if (actualHeaderCount != _expectedHeaders.Length)
      {
        throw new Exception($"Column count is not {_expectedHeaders.Length}, it is {actualHeaderCount}!");
      }

      for (int i = 0; i < actualHeaderCount; ++i)
      {
        var actualHeader = dataSet.Tables[0].Columns[i].ColumnName;
        trace.Debug(Format([i, actualHeader]));
        if (actualHeader != _expectedHeaders[i])
        {
          throw new Exception($"Header[{i}] is not '{_expectedHeaders[i]}', it is '{actualHeader}'!");
        }
      }
    }

    private Transaction TransformDataRowToTransaction(DataRow row)
    {
      return new Transaction
      {
        Id = Guid.NewGuid(),
        TransactionTime = DateTimeOffset.Parse(row.Field<string>(0)),
        AccountingDate = string.IsNullOrEmpty(row.Field<string>(1)) ? null : DateOnly.Parse(row.Field<string>(1)),
        Type = row.Field<string>(2),
        IncomeOutcome = row.Field<string>(3) == "Kimenő" ? IncomeOutcome.Outcome : IncomeOutcome.Income,
        PartnerName = row.Field<string>(4),
        PartnerAccountId = row.Field<string>(5),
        SpendingCategory = row.Field<string>(6),
        Description = row.Field<string>(7),
        AccountName = row.Field<string>(8),
        AccountId = row.Field<string>(9),
        Amount = row.Field<double>(10),
        Currency = Enum.Parse<Currency>(row.Field<string>(11))
      };
    }
  }
}
