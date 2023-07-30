using CommunityToolkit.WinUI.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using Splitwise.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using ZoNo.ViewModels.Import;

namespace ZoNo.Views.Import
{
  public sealed partial class ExpenseView : UserControl
  {
    public static readonly DependencyProperty DateProperty = DependencyProperty.Register(nameof(Date), typeof(DateTime), typeof(ExpenseView), new PropertyMetadata(DateTime.MinValue));
    public static readonly DependencyProperty CategoryProperty = DependencyProperty.Register(nameof(Category), typeof(Category), typeof(ExpenseView), new PropertyMetadata(null));
    public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register(nameof(Description), typeof(string), typeof(ExpenseView), null);
    public static readonly DependencyProperty GroupProperty = DependencyProperty.Register(nameof(Group), typeof(string), typeof(ExpenseView), null);
    public static readonly DependencyProperty PaidProperty = DependencyProperty.Register(nameof(Paid), typeof(string), typeof(ExpenseView), null);
    public static readonly DependencyProperty CurrencyProperty = DependencyProperty.Register(nameof(Currency), typeof(CurrencyCode), typeof(ExpenseView), new PropertyMetadata(CurrencyCode.HUF));
    public static readonly DependencyProperty CostProperty = DependencyProperty.Register(nameof(Cost), typeof(double), typeof(ExpenseView), null);

    public static readonly DependencyProperty CategoriesProperty = DependencyProperty.Register(nameof(Categories), typeof(Category[]), typeof(ExpenseView), new PropertyMetadata(null));

    public DateTime Date
    {
      get => (DateTime)GetValue(DateProperty);
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

    public CurrencyCode Currency
    {
      get => (CurrencyCode)GetValue(CurrencyProperty);
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
  }
}
