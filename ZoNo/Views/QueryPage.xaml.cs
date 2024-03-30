using CommunityToolkit.Common.Collections;
using CommunityToolkit.WinUI.UI;
using CommunityToolkit.WinUI.UI.Controls;
using CommunityToolkit.WinUI.UI.Controls.Primitives;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System.Diagnostics;
using ZoNo.Converters;
using ZoNo.Helpers;
using ZoNo.Models;
using ZoNo.ViewModels;
using Grid = Microsoft.UI.Xaml.Controls.Grid;

namespace ZoNo.Views
{
  public sealed partial class QueryPage : Page
  {
    private ThousandsSeparatorConverter _thousandsSeparatorConverter = new();
    private Dictionary<QueryGroupBy, Func<object, string>> _groupByFormatter = new()
    {
      { QueryGroupBy.MainCategory, x => $"{(x as Category).Name}" },
      { QueryGroupBy.SubCategory, x => $"{(x as Category).ParentCategory.Name} - {(x as Category).Name}" }
    };

    public QueryPageViewModel ViewModel { get; }

    public QueryPage()
    {
      ViewModel = App.GetService<QueryPageViewModel>();
      InitializeComponent();
    }

    private async void Page_Loaded(object sender, RoutedEventArgs e)
    {
      // To set the disabled state border to collapsed as text opacity is used to determine if the data grid is enabled or not
      var border = DataGrid.FindDescendant<Border>(border => border.Name == "DisabledVisualElement") ?? throw new Exception($"Border with name \"DisabledVisualElement\" does not exist");
      border.Visibility = Visibility.Collapsed;

      // Set default DataGridTextOpacity
      DataGrid_IsEnabledChanged(null, null);

      ViewModel.PropertyChanged -= ViewModel_PropertyChanged;
      ViewModel.PropertyChanged += ViewModel_PropertyChanged;

      await ViewModel.LoadAsync();
    }

    private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
      if (e.PropertyName == nameof(ViewModel.IsLoading) && !ViewModel.IsLoading)
      {
        DataGrid.SelectedItem = null;
        DataGrid.LayoutUpdated += DataGrid_LayoutUpdated;
      }
    }

    private void DataGrid_LayoutUpdated(object sender, object e)
    {
      ExpandCollapseAll(isExpand: false);
      DataGrid.LayoutUpdated -= DataGrid_LayoutUpdated;
    }

    private void DataGrid_LoadingRowGroup(object sender, DataGridRowGroupHeaderEventArgs e)
    {
      var expenseGroup = e.RowGroupHeader.CollectionViewGroup.Group as ReadOnlyObservableGroup<object, ExpenseViewModel>;
      var expenseGroupByCurrency = expenseGroup
        .Select(expense => expense)
        .GroupBy(group => group.Currency)
        .OrderBy(group => group.Key);

      var groupCount = expenseGroupByCurrency.Count();
      var numberOfCurrencies = ViewModel.NumberOfCurrencies;
      var costs = new List<(double TotalCost, Currency Currency)?>();
      for (int i = 0; i < numberOfCurrencies; ++i)
      {
        if (i < groupCount)
        {
          var group = expenseGroupByCurrency.ElementAt(i);
          costs.Add((
            TotalCost: group.Sum(expense => expense.Cost),
            Currency: group.Key
          ));
        }
        else
        {
          costs.Add(null);
        }
      }

      void RowGroupHeader_Loaded(object sender, RoutedEventArgs e2)
      {
        var dataGridFrozenGrid = e.RowGroupHeader.FindDescendant("RowGroupHeaderRoot") as DataGridFrozenGrid;
        if (dataGridFrozenGrid is null)
        {
          return;
        }
        e.RowGroupHeader.Loaded -= RowGroupHeader_Loaded;

        var grid = new Grid();
        grid.SetValue(Grid.ColumnProperty, 3);
        grid.ColumnDefinitions.Add(new() { Width = new GridLength(200) });
        for (int i = 0; i < numberOfCurrencies; ++i)
        {
          grid.ColumnDefinitions.Add(new() { Width = new GridLength(130) });
        }
        grid.ColumnDefinitions.Add(new() { Width = new GridLength(200) });

        int index = 0;
        var groupByValueText = new TextBlock() { Text = _groupByFormatter[ViewModel.QueryGroupBy](expenseGroup.Key) };
        groupByValueText.SetValue(Grid.ColumnProperty, index++);
        grid.Children.Add(groupByValueText);

        for (int i = 0; i < numberOfCurrencies; ++i)
        {
          if (costs[i] is not null)
          {
            var totalCostValueText = new TextBlock()
            {
              Text = $"{_thousandsSeparatorConverter.Convert(costs[i].Value.TotalCost, null, null, null) as string} {costs[i].Value.Currency}",
              HorizontalAlignment = HorizontalAlignment.Right,
              IsTextSelectionEnabled = true
            };
            totalCostValueText.SetValue(Grid.ColumnProperty, index);
            grid.Children.Add(totalCostValueText);
          }
          index++;
        }

        var groupedItemCountText = new TextBlock() { Text = expenseGroup.Count == 1 ? "(1 item)" : $"({expenseGroup.Count} items)" };
        groupedItemCountText.SetValue(Grid.ColumnProperty, index++);
        grid.Children.Add(groupedItemCountText);

        dataGridFrozenGrid.Children[2] = grid;
      }

      e.RowGroupHeader.Loaded += RowGroupHeader_Loaded;
      RowGroupHeader_Loaded(null, null);
    }

    private void DataGrid_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
      _ = DataGrid.Resources["DataGridTextOpacity"] as double? ?? throw new Exception("Double with key \"DataGridTextOpacity\" does not exist");
      DataGrid.Resources["DataGridTextOpacity"] = DataGrid.IsEnabled ? 1 : 0.3;

      DataGrid.Resources["DataGridRowGroupHeaderForegroundBrush"] = DataGrid.IsEnabled ?
        new SolidColorBrush((DataGrid.Resources["DataGridRowGroupHeaderColorPrimary"] as SolidColorBrush).Color) :
        new SolidColorBrush((DataGrid.Resources["DataGridRowGroupHeaderFillColorDisabled"] as SolidColorBrush).Color);

      DataGrid.ReloadThemeResources();

      foreach (Image image in DataGrid.FindDescendants().Where(x => x is Image))
      {
        image.Opacity = DataGrid.IsEnabled ? 1 : 0.3;
      }
    }

    private void ExpandCollapse_Click(object sender, RoutedEventArgs e)
    {
      if (ExpandCollapseButton.Content is "Expand All")
      {
        ExpandCollapseAll(isExpand: true);
      }
      else
      {
        ExpandCollapseAll(isExpand: false);
      }
    }

    private void ExpandCollapseAll(bool isExpand)
    {
      ExpandCollapseButton.Content = isExpand ? "Collapse All" : "Expand All";
      foreach (var item in ViewModel.ExpenseGroups.View)
      {
        var group = DataGrid.GetGroupFromItem(item, 0);
        if (isExpand)
        {
          DataGrid.ExpandRowGroup(group, true);
        }
        else
        {
          DataGrid.CollapseRowGroup(group, true);
        }
      }
    }

    private void DatePicker_Loaded(object sender, RoutedEventArgs e)
    {
      if (sender is DatePicker datePicker)
      {
        if (datePicker.FindDescendant("HeaderContentPresenter") is ContentPresenter headerContentPresenter)
        {
          headerContentPresenter.Height = 20;
          headerContentPresenter.Margin = new Thickness(0, 0, 0, 8);
        }
        if (datePicker.FindDescendant("FlyoutButton") is Button flyoutButton)
        {
          if (flyoutButton.FindDescendant("FlyoutButtonContentGrid") is Grid flyoutButtonContentGrid)
          {
            flyoutButtonContentGrid.Height = 30;
          }
          if (flyoutButton.FindDescendant("MonthTextBlock") is TextBlock monthTextBlock)
          {
            monthTextBlock.VerticalAlignment = VerticalAlignment.Bottom;
          }
          if (flyoutButton.FindDescendant("YearTextBlock") is TextBlock yearTextBlock)
          {
            yearTextBlock.VerticalAlignment = VerticalAlignment.Bottom;
          }
        }
      }
    }
  }
}