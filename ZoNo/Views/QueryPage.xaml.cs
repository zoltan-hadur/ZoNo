using CommunityToolkit.Common.Collections;
using CommunityToolkit.WinUI.UI;
using CommunityToolkit.WinUI.UI.Controls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using ZoNo.Converters;
using ZoNo.Helpers;
using ZoNo.Models;
using ZoNo.ViewModels;

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
      var expenseGroupByCurrency = expenseGroup.Select(expense => expense).GroupBy(group => group.Currency);

      e.RowGroupHeader.PropertyName = ViewModel.QueryGroupBy.ToString();
      e.RowGroupHeader.PropertyValue = $"{_groupByFormatter[ViewModel.QueryGroupBy](expenseGroup.Key),-30} Total Cost: {string.Join(", ", expenseGroupByCurrency
        .Select(group => $"{_thousandsSeparatorConverter.Convert(group.Sum(expense => expense.Cost), null, null, null)} {group.Key}"))}";
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
  }
}