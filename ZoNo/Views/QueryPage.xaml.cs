using CommunityToolkit.Common.Collections;
using CommunityToolkit.WinUI.UI;
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

      await ViewModel.LoadAsync();
    }

    private void DataGrid_LoadingRowGroup(object sender, CommunityToolkit.WinUI.UI.Controls.DataGridRowGroupHeaderEventArgs e)
    {
      var expenseGroupByCategory = e.RowGroupHeader.CollectionViewGroup.Group as ReadOnlyObservableGroup<Category, ExpenseViewModel>;
      var expenseGroupByCurrency = expenseGroupByCategory.Select(expense => expense).GroupBy(group => group.Currency);

      e.RowGroupHeader.PropertyValue = $"{expenseGroupByCategory.Key.Name}, Total Cost: {string.Join(", ", expenseGroupByCurrency
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
    }
  }
}