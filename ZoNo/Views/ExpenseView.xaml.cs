using CommunityToolkit.WinUI.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using ZoNo.Models;

namespace ZoNo.Views
{
  public sealed partial class ExpenseView : UserControl
  {
    public static readonly DependencyProperty DateProperty = DependencyProperty.Register(nameof(Date), typeof(DateTimeOffset), typeof(ExpenseView), new PropertyMetadata(DateTimeOffset.MinValue));
    public static readonly DependencyProperty CategoryProperty = DependencyProperty.Register(nameof(Category), typeof(Category), typeof(ExpenseView), new PropertyMetadata(null));
    public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register(nameof(Description), typeof(string), typeof(ExpenseView), null);
    public static readonly DependencyProperty GroupProperty = DependencyProperty.Register(nameof(Group), typeof(string), typeof(ExpenseView), null);
    public static readonly DependencyProperty PaidProperty = DependencyProperty.Register(nameof(Paid), typeof(string), typeof(ExpenseView), null);
    public static readonly DependencyProperty CurrencyProperty = DependencyProperty.Register(nameof(Currency), typeof(Currency), typeof(ExpenseView), new PropertyMetadata(Currency.HUF));
    public static readonly DependencyProperty CostProperty = DependencyProperty.Register(nameof(Cost), typeof(double), typeof(ExpenseView), null);

    public static readonly DependencyProperty CategoriesProperty = DependencyProperty.Register(nameof(Categories), typeof(Category[]), typeof(ExpenseView), new PropertyMetadata(null));

    public DateTimeOffset Date
    {
      get => (DateTimeOffset)GetValue(DateProperty);
      set => SetValue(DateProperty, value);
    }

    public Category Category
    {
      get => (Category)GetValue(CategoryProperty);
      set => SetValue(CategoryProperty, value);
    }

    public string Description
    {
      get => (string)GetValue(DescriptionProperty);
      set => SetValue(DescriptionProperty, value);
    }

    public string Group
    {
      get => (string)GetValue(GroupProperty);
      set => SetValue(GroupProperty, value);
    }

    public string Paid
    {
      get => (string)GetValue(PaidProperty);
      set => SetValue(PaidProperty, value);
    }

    public Currency Currency
    {
      get => (Currency)GetValue(CurrencyProperty);
      set => SetValue(CurrencyProperty, value);
    }

    public double Cost
    {
      get => (double)GetValue(CostProperty);
      set => SetValue(CostProperty, value);
    }

    public Category[] Categories
    {
      get => (Category[])GetValue(CategoriesProperty);
      set => SetValue(CategoriesProperty, value);
    }

    public ExpenseView()
    {
      InitializeComponent();
    }

    private void SubCategoryButton_Click(object sender, RoutedEventArgs e)
    {
      CategoriesFlyout.Hide();
    }

    private void Image_PointerEntered(object sender, PointerRoutedEventArgs e)
    {
      if (sender is Image image)
      {
        image.Opacity = 0.5;
      }
    }

    private void Image_PointerExited(object sender, PointerRoutedEventArgs e)
    {
      if (sender is Image image)
      {
        image.Opacity = 1;
      }
    }

    private void StackPanel_SizeChanged(object sender, SizeChangedEventArgs e)
    {
      if (sender is StackPanel stackPanel &&
          stackPanel.GetValue(Grid.ColumnProperty) is int column &&
          stackPanel.FindAscendants().FirstOrDefault(ascendant => ascendant.GetValue(Helpers.Grid.IsSharedSizeScopeProperty) is true) is DependencyObject element &&
          element.GetValue(Helpers.Grid.SharedSizeScopeProperty) is Dictionary<int, double> sharedSizeScope)
      {
        var maxWidth = sharedSizeScope.TryGetValue(column, out double value) ? value : 0;
        if (stackPanel.ActualWidth > maxWidth)
        {
          sharedSizeScope[column] = maxWidth = stackPanel.ActualWidth;
          foreach (var expense in element.FindDescendants().Where(descendant => descendant is ExpenseView).Cast<ExpenseView>())
          {
            expense.Grid.ColumnDefinitions[column].Width = new GridLength(maxWidth, GridUnitType.Pixel);
          }
        }
        else
        {
          Grid.ColumnDefinitions[column].Width = new GridLength(maxWidth, GridUnitType.Pixel);
        }
      }
    }
  }
}
