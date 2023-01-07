using CommunityToolkit.WinUI.UI;
using CommunityToolkit.WinUI.UI.Controls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.ApplicationModel.DataTransfer;
using ZoNo.Helpers;
using ZoNo.Models;
using ZoNo.ViewModels;

namespace ZoNo.Views
{
  public sealed partial class ImportPage : Page
  {
    public ImportViewModel ViewModel { get; }

    public ImportPage()
    {
      ViewModel = App.GetService<ImportViewModel>();
      InitializeComponent();
    }

    private void Border_DragOver(object sender, DragEventArgs e)
    {
      e.AcceptedOperation = DataPackageOperation.Copy;
      e.DragUIOverride.Caption = "Import_Add".GetLocalized();
    }

    private async void Border_Drop(object sender, DragEventArgs e)
    {
      if (e.DataView.Contains(StandardDataFormats.StorageItems))
      {
        var items = await e.DataView.GetStorageItemsAsync();
        foreach (var path in items.Select(item => item.Path))
        {
          await ViewModel.LoadExcelDocument(path);
        }
      }
    }

    private Dictionary<ColumnHeader, string> _mapToProperty = new Dictionary<ColumnHeader, string>()
    {
      { ColumnHeader.TransactionTime , nameof(Transaction.TransactionTime ) },
      { ColumnHeader.AccountingDate  , nameof(Transaction.AccountingDate  ) },
      { ColumnHeader.Type            , nameof(Transaction.Type            ) },
      { ColumnHeader.IncomeOutcome   , nameof(Transaction.IncomeOutcome   ) },
      { ColumnHeader.PartnerName     , nameof(Transaction.PartnerName     ) },
      { ColumnHeader.PartnerAccountId, nameof(Transaction.PartnerAccountId) },
      { ColumnHeader.SpendingCategory, nameof(Transaction.SpendingCategory) },
      { ColumnHeader.Description     , nameof(Transaction.Description     ) },
      { ColumnHeader.AccountName     , nameof(Transaction.AccountName     ) },
      { ColumnHeader.AccountId       , nameof(Transaction.AccountId       ) },
      { ColumnHeader.Amount          , nameof(Transaction.Amount          ) },
      { ColumnHeader.Currency        , nameof(Transaction.Currency        ) }
    };

    private void DataGrid_Sorting(object sender, DataGridColumnEventArgs e)
    {
      ViewModel.TransactionsView.SortDescriptions.Clear();

      switch (e.Column.SortDirection)
      {
        // Descending after Ascending
        case DataGridSortDirection.Ascending:
          e.Column.SortDirection = DataGridSortDirection.Descending;
          ViewModel.TransactionsView.SortDescriptions.Add(new SortDescription(_mapToProperty[(ColumnHeader)(int)e.Column.Tag], SortDirection.Descending));
          break;

        // Default after Descending
        case DataGridSortDirection.Descending:
          e.Column.SortDirection = null;
          ViewModel.TransactionsView.SortDescriptions.Add(new SortDescription(_mapToProperty[ColumnHeader.TransactionTime], SortDirection.Ascending));
          break;

        // Ascending after default
        default:
          e.Column.SortDirection = DataGridSortDirection.Ascending;
          ViewModel.TransactionsView.SortDescriptions.Add(new SortDescription(_mapToProperty[(ColumnHeader)(int)e.Column.Tag], SortDirection.Ascending));
          break;
      }

      // Null out every other column sort direction
      foreach (var column in (sender as DataGrid)!.Columns.Where(column => column.Tag != e.Column.Tag))
      {
        column.SortDirection = null;
      }
    }
  }
}