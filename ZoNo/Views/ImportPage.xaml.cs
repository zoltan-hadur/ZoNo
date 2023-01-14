using CommunityToolkit.WinUI.UI;
using CommunityToolkit.WinUI.UI.Controls;
using CommunityToolkit.WinUI.UI.Controls.Primitives;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Markup;
using System.Linq;
using System.Reflection;
using Windows.ApplicationModel.DataTransfer;
using Windows.System;
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

    private async void Page_Loading(FrameworkElement sender, object args)
    {
      await ViewModel.Load();

      foreach (var column in ViewModel.Columns!)
      {
        // Add items to column header context menu, so the user can show/hide columns
        var menuItem = new ToggleMenuFlyoutItem()
        {
          Text = $"Import_{column.ColumnHeader}".GetLocalized()
        };
        menuItem.SetBinding(ToggleMenuFlyoutItem.IsCheckedProperty, new Binding()
        {
          Source = column,
          Path = new PropertyPath(nameof(column.IsVisible)),
          Mode = BindingMode.TwoWay,
          UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
        });
        DataGridHeaderContextMenu.Items.Add(menuItem);

        // Add columns to data grid
        var cellDataTemplateXaml = $$"""
                    <DataTemplate xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
                      <TextBlock
                        Text="{Binding {{_mapToProperty[column.ColumnHeader]}}}"
                        ToolTipService.ToolTip="{Binding {{_mapToProperty[column.ColumnHeader]}}}"/>
                    </DataTemplate>
                    """;
        var cellDataTemplate = XamlReader.Load(cellDataTemplateXaml) as DataTemplate;
        var dataGridColumn = new DataGridTemplateColumn()
        {
          Header = $"Import_{column.ColumnHeader}".GetLocalized(),
          Visibility = column.IsVisible ? Visibility.Visible : Visibility.Collapsed,
          Tag = column.ColumnHeader,
          IsReadOnly = true,
          CellTemplate = cellDataTemplate,
        };
        column.PropertyChanged += (s, e) =>
        {
          if (e.PropertyName == nameof(ColumnViewModel.IsVisible))
          {
            dataGridColumn.Visibility = column.IsVisible ? Visibility.Visible : Visibility.Collapsed;
          }
        };
        DataGrid.Columns.Add(dataGridColumn);
      }

      // Set horizontal alignment to the right for column Amount
      var amountColumn = DataGrid.Columns.Single(column => (ColumnHeader)(int)column.Tag == ColumnHeader.Amount);
      var style = new Style(typeof(DataGridCell));
      style.Setters.Add(new Setter(DataGridCell.HorizontalContentAlignmentProperty, HorizontalAlignment.Right));
      amountColumn.CellStyle = style;
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
          ViewModel.TransactionsView.SortDescriptions.Add(new SortDescription(_mapToProperty[(ColumnHeader)(int)e.Column.Tag], SortDirection.Descending));
          break;

        // Default after Descending
        case DataGridSortDirection.Descending:
          e.Column.SortDirection = null;
          GetHeader(e.Column).Padding = new Thickness(12, 0, -20, 0);
          ViewModel.TransactionsView.SortDescriptions.Add(new SortDescription(_mapToProperty[ColumnHeader.TransactionTime], SortDirection.Ascending));
          break;

        // Ascending after default
        default:
          e.Column.SortDirection = DataGridSortDirection.Ascending;
          ViewModel.TransactionsView.SortDescriptions.Add(new SortDescription(_mapToProperty[(ColumnHeader)(int)e.Column.Tag], SortDirection.Ascending));
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