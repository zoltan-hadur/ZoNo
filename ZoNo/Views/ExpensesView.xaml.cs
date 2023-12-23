using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System.Collections.ObjectModel;
using System.Windows.Input;
using ZoNo.Models;
using ZoNo.ViewModels;

namespace ZoNo.Views
{
  public sealed partial class ExpensesView : Page
  {
    public static readonly DependencyProperty CategoriesProperty = DependencyProperty.Register(nameof(Expenses), typeof(IEnumerable<Category>), typeof(ExpensesView), null);
    public static readonly DependencyProperty ExpensesProperty = DependencyProperty.Register(nameof(Expenses), typeof(ObservableCollection<ExpenseViewModel>), typeof(ExpensesView), null);
    public static readonly DependencyProperty EditExpenseCommandProperty = DependencyProperty.Register(nameof(EditExpenseCommand), typeof(ICommand), typeof(ExpensesView), null);
    public static readonly DependencyProperty DeleteExpenseCommandProperty = DependencyProperty.Register(nameof(DeleteExpenseCommand), typeof(ICommand), typeof(ExpensesView), null);
    public static readonly DependencyProperty SelectedExpenseProperty = DependencyProperty.Register(nameof(SelectedExpense), typeof(ExpenseViewModel), typeof(ExpensesView), null);

    public IEnumerable<Category> Categories
    {
      get => (IEnumerable<Category>)GetValue(CategoriesProperty);
      set => SetValue(CategoriesProperty, value);
    }

    public ObservableCollection<ExpenseViewModel> Expenses
    {
      get => (ObservableCollection<ExpenseViewModel>)GetValue(ExpensesProperty);
      set => SetValue(ExpensesProperty, value);
    }

    public ICommand EditExpenseCommand
    {
      get => (ICommand)GetValue(EditExpenseCommandProperty);
      set => SetValue(EditExpenseCommandProperty, value);
    }

    public ICommand DeleteExpenseCommand
    {
      get => (ICommand)GetValue(DeleteExpenseCommandProperty);
      set => SetValue(DeleteExpenseCommandProperty, value);
    }

    public ExpenseViewModel SelectedExpense
    {
      get => (ExpenseViewModel)GetValue(SelectedExpenseProperty);
      set => SetValue(SelectedExpenseProperty, value);
    }

    public ExpensesView()
    {
      InitializeComponent();
    }

    private void ExpensesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      if (e.AddedItems.Count == 0) return;
      ExpensesList.ScrollIntoView(e.AddedItems[0]);
    }

    private void ExpensesList_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
    {
      if (e.OriginalSource is FrameworkElement element)
      {
        EditExpenseCommand?.Execute(element.DataContext);
      }
    }
  }
}
