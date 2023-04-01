using CommunityToolkit.WinUI.UI;
using CommunityToolkit.WinUI.UI.Controls;
using CommunityToolkit.WinUI.UI.Controls.Primitives;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System.Reflection;
using Windows.ApplicationModel.DataTransfer;
using Windows.System;
using ZoNo.Helpers;
using ZoNo.Models;
using ZoNo.ViewModels.Import;

namespace ZoNo.Views.Import
{
  public sealed partial class TransactionsView : UserControl
  {
    public TransactionsViewModel ViewModel { get; }

    public TransactionsView()
    {
      ViewModel = App.GetService<TransactionsViewModel>();
      InitializeComponent();
    }

    private async void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
      await ViewModel.Load();

      // Default sort by transaction time
      ViewModel.TransactionsView.SortDescriptions.Clear();
      ViewModel.TransactionsView.SortDescriptions.Add(new SortDescription(Transaction.GetProperty(ColumnHeader.TransactionTime), SortDirection.Ascending));
    }

    private void Grid_DragOver(object sender, DragEventArgs e)
    {
      e.AcceptedOperation = DataPackageOperation.Copy;
      e.DragUIOverride.Caption = "Import_Add".GetLocalized();
    }

    private async void Grid_Drop(object sender, DragEventArgs e)
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

    private DataGridColumnHeader GetHeader(DataGridColumn column)
    {
      var propertyInfo = typeof(DataGridColumn).GetProperty("HeaderCell", BindingFlags.NonPublic | BindingFlags.Instance);
      if (propertyInfo == null)
      {
        throw new Exception($"HeaderCell is not a property of {typeof(DataGridColumn)}!");
      }
      var value = propertyInfo.GetValue(column);
      if (value == null)
      {
        throw new Exception($"Property 'HeaderCell' is null!");
      }
      var header = value as DataGridColumnHeader;
      if (header == null)
      {
        throw new Exception($"Property values type is '{value.GetType()} instead of {typeof(DataGridColumnHeader)}!");
      }
      return header;
    }

    private void DataGrid_Sorting(object sender, DataGridColumnEventArgs e)
    {
      ViewModel.TransactionsView.SortDescriptions.Clear();

      GetHeader(e.Column).Padding = new Thickness(12, 0, 0, 0);

      switch (e.Column.SortDirection)
      {
        // Descending after Ascending
        case DataGridSortDirection.Ascending:
          e.Column.SortDirection = DataGridSortDirection.Descending;
          ViewModel.TransactionsView.SortDescriptions.Add(new SortDescription(Transaction.GetProperty((ColumnHeader)e.Column.Tag), SortDirection.Descending));
          break;

        // Default after Descending
        case DataGridSortDirection.Descending:
          e.Column.SortDirection = null;
          GetHeader(e.Column).Padding = new Thickness(12, 0, -20, 0);
          ViewModel.TransactionsView.SortDescriptions.Add(new SortDescription(Transaction.GetProperty(ColumnHeader.TransactionTime), SortDirection.Ascending));
          break;

        // Ascending after default
        default:
          e.Column.SortDirection = DataGridSortDirection.Ascending;
          ViewModel.TransactionsView.SortDescriptions.Add(new SortDescription(Transaction.GetProperty((ColumnHeader)e.Column.Tag), SortDirection.Ascending));
          break;
      }

      // Null out every other column sort direction
      foreach (var column in DataGrid.Columns.Where(column => column.Tag != e.Column.Tag))
      {
        column.SortDirection = null;
        GetHeader(column).Padding = new Thickness(12, 0, -20, 0);
      }
    }

    private async void DataGrid_KeyDown(object sender, KeyRoutedEventArgs e)
    {
      if (e.Key == VirtualKey.Delete)
      {
        var selectedItems = DataGrid.SelectedItems.Cast<object>().ToList();
        foreach (var selectedItem in selectedItems)
        {
          try
          {
            ViewModel.TransactionsView.Source.Remove(selectedItem);
          }
          catch (ArgumentOutOfRangeException ex)
          {
            // When deleting last item, there is an exception
            ViewModel.TransactionsView.Refresh();
          }
          await Task.Delay(1);
        }
      }
    }
  }
}
