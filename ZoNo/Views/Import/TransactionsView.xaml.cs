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
using System.Diagnostics;
using System.Numerics;
using System.Reflection;
using System.Windows.Input;
using Windows.ApplicationModel.DataTransfer;
using Windows.System;
using ZoNo.Models;
using ZoNo.ViewModels.Import;

namespace ZoNo.Views.Import
{
  public sealed partial class TransactionsView : UserControl
  {
    private Dictionary<Transaction, DateTime> _loadTimes = new Dictionary<Transaction, DateTime>();
    private HashSet<DataGridRow> _rows = new HashSet<DataGridRow>();
    private ScrollBar? _scrollBar;
    private Stopwatch _lastSort = Stopwatch.StartNew();
    private Stopwatch _lastAddition = Stopwatch.StartNew();
    private Stopwatch _lastDeletion = Stopwatch.StartNew();

    public static readonly DependencyProperty TransactionsProperty = DependencyProperty.Register(nameof(Transactions), typeof(AdvancedCollectionView), typeof(TransactionsView), new PropertyMetadata(null, OnTransactionsPropertyChanged));
    public static readonly DependencyProperty ColumnsProperty = DependencyProperty.Register(nameof(Columns), typeof(Dictionary<string, ColumnViewModel>), typeof(TransactionsView), null);
    public static readonly DependencyProperty LoadExcelDocumentsCommandProperty = DependencyProperty.Register(nameof(LoadExcelDocumentsCommand), typeof(ICommand), typeof(TransactionsView), null);
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

    private void Transactions_CollectionChanged_BeforeDataContextChanges(object? sender, NotifyCollectionChangedEventArgs e)
    {
      foreach (Transaction transaction in e.OldItems ?? Array.Empty<Transaction>())
      {
        _lastDeletion.Restart();
        _loadTimes.Remove(transaction);
        var rowToBeDeleted = _rows.FirstOrDefault(row => row.DataContext == transaction);
        if (rowToBeDeleted != null)
        {
          if (_scrollBar!.Value > _scrollBar.Maximum - rowToBeDeleted.ActualHeight && _scrollBar.Maximum != 0)
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
      foreach (Transaction transaction in e.NewItems ?? Array.Empty<Transaction>())
      {
        _lastAddition.Restart();
        _loadTimes[transaction] = DateTime.Now;
      }
    }

    private async void Transactions_CollectionChanged_AfterDataContextChanges(object? sender, NotifyCollectionChangedEventArgs e)
    {
      foreach (Transaction transaction in e.NewItems ?? Array.Empty<Transaction>())
      {
        var addedRow = _rows.FirstOrDefault(row => row.DataContext == transaction);
        if (addedRow != null)
        {
          using var semaphore = new SemaphoreSlim(0, 1);
          EventHandler<object> handler = (s, e) =>
          {
            semaphore.Release();
          };
          addedRow.LayoutUpdated += handler;
          await semaphore.WaitAsync();
          addedRow.LayoutUpdated -= handler;
          if (_scrollBar!.Value > _scrollBar.Maximum - addedRow.ActualHeight && _scrollBar.Maximum != 0)
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
      var lastSort = _lastSort.ElapsedMilliseconds;
      var lastAddition = _lastAddition.ElapsedMilliseconds;
      var lastDeletion = _lastDeletion.ElapsedMilliseconds;
      if (lastSort < 100 || lastAddition < 100 || lastDeletion < 100)
      {
        return;
      }
      foreach (var row in _rows)
      {
        AnimationBuilder.Create().Translation(
          from: new Vector2(0, (float)(e.NewValue - e.OldValue)),
          to: new Vector2(0, 0),
          easingMode: EasingMode.EaseOut,
          layer: FrameworkLayer.Composition
        ).Start(row);
      }
    }

    private void DataGrid_LoadingRow(object? sender, DataGridRowEventArgs e)
    {
      var now = DateTime.Now;
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

    private async void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
      DataGrid.LoadingRow += DataGrid_LoadingRow;
      _scrollBar = DataGrid.FindDescendant("VerticalScrollBar") as ScrollBar;
      _scrollBar!.ValueChanged += ScrollBar_ValueChanged;

      await Task.Delay(1);
      DataGrid.ItemsSource = Transactions;

      // Default sort by transaction time
      Transactions.SortDescriptions.Clear();
      Transactions.SortDescriptions.Add(new SortDescription(Transaction.GetProperty(ColumnHeader.TransactionTime), SortDirection.Ascending));
    }

    private void UserControl_Unloaded(object sender, RoutedEventArgs e)
    {
      Transactions = new AdvancedCollectionView();
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
      _lastSort.Restart();

      Transactions.SortDescriptions.Clear();

      GetHeader(e.Column).Padding = new Thickness(12, 0, 0, 0);

      switch (e.Column.SortDirection)
      {
        // Descending after Ascending
        case DataGridSortDirection.Ascending:
          e.Column.SortDirection = DataGridSortDirection.Descending;
          Transactions.SortDescriptions.Add(new SortDescription(Transaction.GetProperty((ColumnHeader)e.Column.Tag), SortDirection.Descending));
          break;

        // Default after Descending
        case DataGridSortDirection.Descending:
          e.Column.SortDirection = null;
          GetHeader(e.Column).Padding = new Thickness(12, 0, -20, 0);
          Transactions.SortDescriptions.Add(new SortDescription(Transaction.GetProperty(ColumnHeader.TransactionTime), SortDirection.Ascending));
          break;

        // Ascending after default
        default:
          e.Column.SortDirection = DataGridSortDirection.Ascending;
          Transactions.SortDescriptions.Add(new SortDescription(Transaction.GetProperty((ColumnHeader)e.Column.Tag), SortDirection.Ascending));
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
            Transactions.Source.Remove(selectedItem);
          }
          catch (ArgumentOutOfRangeException)
          {
            // When deleting last item, there is an exception
            Transactions.Refresh();
          }
          await Task.Delay(1);
        }
      }
    }

    private static async void OnSelectedItemChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
      if (sender is TransactionsView view && e.NewValue is Transaction transaction)
      {
        await Task.Delay(100);
        view.DataGrid.ScrollIntoView(transaction, null);
      }
    }

    private void DataGrid_Loaded(object sender, RoutedEventArgs e)
    {
      // To set the disabled state border to collapsed as text opacity is used to determine if the data grid is enabled or not
      var border = DataGrid.FindDescendant<Border>(border => border.Name == "DisabledVisualElement");
      if (border == null)
      {
        throw new Exception($"Border with name \"DisabledVisualElement\" does not exist");
      }
      border.Visibility = Visibility.Collapsed;

      // Set default DataGridTextOpacity
      DataGrid_IsEnabledChanged(null, null);
    }

    private void DataGrid_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
      var opacity = DataGrid.Resources["DataGridTextOpacity"] as double?;
      if (opacity == null)
      {
        throw new Exception("Double with key \"DataGridTextOpacity\" does not exist");
      }
      DataGrid.Resources["DataGridTextOpacity"] = DataGrid.IsEnabled ? 1 : 0.3;

      // Force resource reload
      if (DataGrid.ActualTheme == ElementTheme.Light)
      {
        DataGrid.RequestedTheme = ElementTheme.Dark;
        DataGrid.RequestedTheme = ElementTheme.Light;
      }
      else if (DataGrid.ActualTheme == ElementTheme.Dark)
      {
        DataGrid.RequestedTheme = ElementTheme.Light;
        DataGrid.RequestedTheme = ElementTheme.Dark;
      }
      else
      {
        DataGrid.RequestedTheme = ElementTheme.Light;
        DataGrid.RequestedTheme = ElementTheme.Dark;
        DataGrid.RequestedTheme = ElementTheme.Default;
      }
    }
  }
}
