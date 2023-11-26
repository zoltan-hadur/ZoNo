using CommunityToolkit.WinUI.UI;
using CommunityToolkit.WinUI.UI.Animations;
using CommunityToolkit.WinUI.UI.Controls;
using CommunityToolkit.WinUI.UI.Controls.Primitives;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Animation;
using System.Collections;
using System.Collections.Specialized;
using System.Numerics;
using System.Reflection;
using System.Windows.Input;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation.Collections;
using Windows.System;
using ZoNo.Helpers;
using ZoNo.Models;
using ZoNo.ViewModels;
using Grid = Microsoft.UI.Xaml.Controls.Grid;

namespace ZoNo.Views
{
  public sealed partial class TransactionsView : UserControl
  {
    private readonly Dictionary<Transaction, DateTimeOffset> _loadTimes = [];
    private readonly HashSet<DataGridRow> _rows = [];
    private ScrollBar _scrollBar;
    private bool _isLoaded = false;
    private bool _isSorting = false;
    private bool _isPointerWheelScrolled = false;

    public static readonly DependencyProperty TransactionsProperty = DependencyProperty.Register(nameof(Transactions), typeof(AdvancedCollectionView), typeof(TransactionsView), new PropertyMetadata(null, OnTransactionsPropertyChanged));
    public static readonly DependencyProperty ColumnsProperty = DependencyProperty.Register(nameof(Columns), typeof(Dictionary<string, ColumnViewModel>), typeof(TransactionsView), null);
    public static readonly DependencyProperty LoadExcelDocumentsCommandProperty = DependencyProperty.Register(nameof(LoadExcelDocumentsCommand), typeof(ICommand), typeof(TransactionsView), null);
    public static readonly DependencyProperty DeleteTransactionsCommandProperty = DependencyProperty.Register(nameof(DeleteTransactionsCommand), typeof(ICommand), typeof(TransactionsView), null);
    public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register(nameof(SelectedItem), typeof(Transaction), typeof(TransactionsView), new PropertyMetadata(null, OnSelectedItemChanged));

    public AdvancedCollectionView Transactions
    {
      get => (AdvancedCollectionView)GetValue(TransactionsProperty);
      set => SetValue(TransactionsProperty, value);
    }

    public Dictionary<string, ColumnViewModel> Columns
    {
      get => (Dictionary<string, ColumnViewModel>)GetValue(ColumnsProperty);
      set => SetValue(ColumnsProperty, value);
    }

    public ICommand LoadExcelDocumentsCommand
    {
      get => (ICommand)GetValue(LoadExcelDocumentsCommandProperty);
      set => SetValue(LoadExcelDocumentsCommandProperty, value);
    }

    public ICommand DeleteTransactionsCommand
    {
      get => (ICommand)GetValue(DeleteTransactionsCommandProperty);
      set => SetValue(DeleteTransactionsCommandProperty, value);
    }

    public Transaction SelectedItem
    {
      get => (Transaction)GetValue(SelectedItemProperty);
      set => SetValue(SelectedItemProperty, value);
    }

    public TransactionsView()
    {
      InitializeComponent();
    }

    private static void OnTransactionsPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
      if (sender is TransactionsView view)
      {
        if (e.OldValue is AdvancedCollectionView oldTransactions &&
            oldTransactions.Source is INotifyCollectionChanged oldSource)
        {
          oldSource.CollectionChanged -= view.Transactions_CollectionChanged_BeforeDataContextChanges;
          oldSource.CollectionChanged -= view.Transactions_CollectionChanged_AfterDataContextChanges;
        }
        if (e.NewValue is AdvancedCollectionView newTransactions &&
            newTransactions.Source is INotifyCollectionChanged newSource)
        {
          newSource.CollectionChanged += view.Transactions_CollectionChanged_BeforeDataContextChanges;
          newTransactions.Source = Array.Empty<Transaction>();
          newTransactions.Source = newSource as IList;
          newSource.CollectionChanged += view.Transactions_CollectionChanged_AfterDataContextChanges;
        }
        if (view.DataGrid.IsLoaded)
        {
          view.DataGrid.ItemsSource = e.NewValue as IEnumerable;
        }
      }
    }

    private void Transactions_CollectionChanged_BeforeDataContextChanges(object sender, NotifyCollectionChangedEventArgs e)
    {
      foreach (Transaction transaction in e.OldItems.OrEmpty())
      {
        _loadTimes.Remove(transaction);
        var rowToBeDeleted = _rows.FirstOrDefault(row => row.DataContext == transaction);
        if (rowToBeDeleted != null)
        {
          if (_scrollBar.Value > _scrollBar.Maximum - rowToBeDeleted.ActualHeight && _scrollBar.Maximum != 0)
          {
            foreach (var row in _rows.Where(x => x.ActualOffset.Y <= rowToBeDeleted.ActualOffset.Y).OrderBy(x => x.ActualOffset.Y))
            {
              AnimationBuilder.Create().Translation(
                from: new Vector2(0, -(float)row.ActualHeight),
                to: new Vector2(0, 0),
                easingMode: EasingMode.EaseOut,
                layer: FrameworkLayer.Composition
              ).Start(row);
            }
          }
          else
          {
            foreach (var row in _rows.Where(x => x.ActualOffset.Y >= rowToBeDeleted.ActualOffset.Y).OrderBy(x => x.ActualOffset.Y))
            {
              AnimationBuilder.Create().Translation(
                from: new Vector2(0, (float)row.ActualHeight),
                to: new Vector2(0, 0),
                easingMode: EasingMode.EaseOut,
                layer: FrameworkLayer.Composition
              ).Start(row);
            }
          }
        }
      }
      foreach (Transaction transaction in e.NewItems.OrEmpty())
      {
        _loadTimes[transaction] = DateTimeOffset.Now;
      }
    }

    private async void Transactions_CollectionChanged_AfterDataContextChanges(object sender, NotifyCollectionChangedEventArgs e)
    {
      foreach (Transaction transaction in e.NewItems.OrEmpty())
      {
        var addedRow = _rows.FirstOrDefault(row => row.DataContext == transaction);
        if (addedRow != null)
        {
          using var semaphore = new SemaphoreSlim(0, 1);
          void handler(object s, object e)
          {
            semaphore.Release();
          }
          addedRow.LayoutUpdated += handler;
          await semaphore.WaitAsync();
          addedRow.LayoutUpdated -= handler;
          if (_scrollBar.Value > _scrollBar.Maximum - addedRow.ActualHeight && _scrollBar.Maximum != 0)
          {
            foreach (var row in _rows.Where(x => x.ActualOffset.Y < addedRow.ActualOffset.Y).OrderBy(x => x.ActualOffset.Y))
            {
              AnimationBuilder.Create().Translation(
                from: new Vector2(0, (float)row.ActualHeight),
                to: new Vector2(0, 0),
                easingMode: EasingMode.EaseOut,
                layer: FrameworkLayer.Composition
              ).Start(row);
            }
          }
          else
          {
            foreach (var row in _rows.Where(x => x.ActualOffset.Y > addedRow.ActualOffset.Y).OrderBy(x => x.ActualOffset.Y))
            {
              AnimationBuilder.Create().Translation(
                from: new Vector2(0, -(float)row.ActualHeight),
                to: new Vector2(0, 0),
                easingMode: EasingMode.EaseOut,
                layer: FrameworkLayer.Composition
              ).Start(row);
            }
          }
        }
      }
    }

    private void ScrollBar_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
    {
      if (!_isPointerWheelScrolled) return;
      foreach (var row in _rows)
      {
        AnimationBuilder.Create().Translation(
          from: new Vector2(0, (float)(e.NewValue - e.OldValue)),
          to: new Vector2(0, 0),
          easingMode: EasingMode.EaseOut,
          layer: FrameworkLayer.Composition
        ).Start(row);
      }
      _isPointerWheelScrolled = false;
    }

    private void DataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
    {
      var now = DateTimeOffset.Now;
      if (e.Row.DataContext is Transaction transaction &&
          _loadTimes.TryGetValue(transaction, out var loadTime) &&
          (now - loadTime).TotalMilliseconds < 100)
      {
        AnimationBuilder.Create().Opacity(
          from: 0,
          to: 1,
          layer: FrameworkLayer.Composition
        ).Start(e.Row);
        _loadTimes.Remove(transaction);
      }

      if (_rows.Add(e.Row))
      {
        e.Row.Loaded += Row_Loaded;
        e.Row.Unloaded += Row_Unloaded;
      }
    }

    private void Row_Loaded(object sender, RoutedEventArgs e)
    {
      AnimationBuilder.Create().Opacity(
        from: 0,
        to: 1,
        layer: FrameworkLayer.Composition
      ).Start((DataGridRow)sender);
    }

    private void Row_Unloaded(object sender, RoutedEventArgs e)
    {
      _rows.Remove((DataGridRow)sender);
    }

    private void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
      if (_isLoaded)
      {
        DataGrid.ReloadThemeResources();
        foreach (var dataGridRow in DataGrid.FindDescendants().Where(descendant => descendant is DataGridRow).Cast<DataGridRow>())
        {
          _rows.Add(dataGridRow);
        }
        return;
      }

      // To set the disabled state border to collapsed as text opacity is used to determine if the data grid is enabled or not
      var border = DataGrid.FindDescendant<Border>(border => border.Name == "DisabledVisualElement") ?? throw new Exception($"Border with name \"DisabledVisualElement\" does not exist");
      border.Visibility = Visibility.Collapsed;

      // Set default DataGridTextOpacity
      DataGrid_IsEnabledChanged(null, null);

      DataGrid.LoadingRow += DataGrid_LoadingRow;
      _scrollBar = DataGrid.FindDescendant("VerticalScrollBar") as ScrollBar;
      _scrollBar.ValueChanged += ScrollBar_ValueChanged;
      var grid = DataGrid.FindDescendant("Root") as Grid;
      grid.PointerWheelChanged += Grid_PointerWheelChanged;

      // Default sort by transaction time
      Transactions.SortDescriptions.Clear();
      Transactions.SortDescriptions.Add(new SortDescription(Transaction.GetProperty(ColumnHeader.TransactionTime), SortDirection.Ascending));
      Transactions.SortDescriptions.Add(new SortDescription(Transaction.GetProperty(ColumnHeader.TransactionTime), SortDirection.Ascending));

      Transactions.VectorChanged += (s, e) =>
      {
        if (_isSorting && e.CollectionChange == CollectionChange.Reset)
        {
          _isSorting = false;
        }
      };

      _isLoaded = true;
    }

    private void Grid_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
    {
      if (_scrollBar.Minimum < _scrollBar.Value && _scrollBar.Value < _scrollBar.Maximum)
      {
        _isPointerWheelScrolled = true;
      }
      else
      {
        var delta = e.GetCurrentPoint(sender as UIElement).Properties.MouseWheelDelta;
        if (delta < 0 && _scrollBar.Value == _scrollBar.Minimum ||
            delta > 0 && _scrollBar.Value == _scrollBar.Maximum)
        {
          _isPointerWheelScrolled = true;
        }
      }
    }

    private void Grid_DragOver(object sender, DragEventArgs e)
    {
      e.AcceptedOperation = DataPackageOperation.Copy;
      e.DragUIOverride.Caption = "Add";
    }

    private async void Grid_Drop(object sender, DragEventArgs e)
    {
      if (e.DataView.Contains(StandardDataFormats.StorageItems))
      {
        var items = await e.DataView.GetStorageItemsAsync();
        var paths = items.Select(item => item.Path).ToArray();
        LoadExcelDocumentsCommand?.Execute(paths);
      }
    }

    private static DataGridColumnHeader GetHeader(DataGridColumn column)
    {
      var propertyInfo = typeof(DataGridColumn).GetProperty("HeaderCell", BindingFlags.NonPublic | BindingFlags.Instance) ?? throw new Exception($"HeaderCell is not a property of {typeof(DataGridColumn)}!");
      var value = propertyInfo.GetValue(column) ?? throw new Exception($"Property 'HeaderCell' is null!");
      var header = value as DataGridColumnHeader;
      return header ?? throw new Exception($"Property values type is '{value.GetType()} instead of {typeof(DataGridColumnHeader)}!");
    }

    private void DataGrid_Sorting(object sender, DataGridColumnEventArgs e)
    {
      _isSorting = true;

      GetHeader(e.Column).Padding = new Thickness(12, 0, 0, 0);

      switch (e.Column.SortDirection)
      {
        // Descending after Ascending
        case DataGridSortDirection.Ascending:
          e.Column.SortDirection = DataGridSortDirection.Descending;
          Transactions.SortDescriptions[0] = new SortDescription(Transaction.GetProperty((ColumnHeader)e.Column.Tag), SortDirection.Descending);
          break;

        // Default after Descending
        case DataGridSortDirection.Descending:
          e.Column.SortDirection = null;
          GetHeader(e.Column).Padding = new Thickness(12, 0, -20, 0);
          Transactions.SortDescriptions[0] = new SortDescription(Transaction.GetProperty(ColumnHeader.TransactionTime), SortDirection.Ascending);
          break;

        // Ascending after default
        default:
          e.Column.SortDirection = DataGridSortDirection.Ascending;
          Transactions.SortDescriptions[0] = new SortDescription(Transaction.GetProperty((ColumnHeader)e.Column.Tag), SortDirection.Ascending);
          break;
      }

      // Null out every other column sort direction
      foreach (var column in DataGrid.Columns.Where(column => column.Tag != e.Column.Tag))
      {
        column.SortDirection = null;
        GetHeader(column).Padding = new Thickness(12, 0, -20, 0);
      }
    }

    private void DataGrid_KeyDown(object sender, KeyRoutedEventArgs e)
    {
      if (e.Key == VirtualKey.Delete)
      {
        DeleteTransactionsCommand?.Execute(DataGrid.SelectedItems.Cast<Transaction>().ToList());
      }
    }

    private static async void OnSelectedItemChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
      if (sender is TransactionsView view)
      {
        if (view._isSorting && e.NewValue is null)
        {
          while (view._isSorting) await Task.Delay(10);
          view.DataGrid.SelectedItem = e.OldValue;
          view.DataGrid.ScrollIntoView(e.OldValue, null);
        }
        else
        {
          view.DataGrid.ScrollIntoView(e.NewValue, null);
        }
      }
    }

    private void DataGrid_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
      _ = DataGrid.Resources["DataGridTextOpacity"] as double? ?? throw new Exception("Double with key \"DataGridTextOpacity\" does not exist");
      DataGrid.Resources["DataGridTextOpacity"] = DataGrid.IsEnabled ? 1 : 0.3;

      DataGrid.ReloadThemeResources();
    }

    private void MenuFlyoutItem_Delete_Click(object sender, RoutedEventArgs e)
    {
      if (sender is MenuFlyoutItem menuFlyoutItem &&
          menuFlyoutItem.DataContext is Transaction transaction)
      {
        if (DataGrid.SelectedItems.Contains(transaction))
        {
          DeleteTransactionsCommand?.Execute(DataGrid.SelectedItems.Cast<Transaction>().ToList());
        }
        else
        {
          DeleteTransactionsCommand?.Execute(new List<Transaction>() { transaction });
        }
      }
    }
  }
}
