using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using ZoNo.Models;
using ZoNo.ViewModels;

namespace ZoNo.Views
{
  public sealed partial class ExpenseEditor : UserControl
  {
    public ExpenseViewModel ViewModel { get; }

    public static readonly DependencyProperty CategoriesProperty = DependencyProperty.Register(nameof(Categories), typeof(IReadOnlyCollection<Category>), typeof(ExpenseView), new PropertyMetadata(null));
    public static readonly DependencyProperty GroupsProperty = DependencyProperty.Register(nameof(Groups), typeof(IReadOnlyCollection<Group>), typeof(ExpenseView), new PropertyMetadata(null));

    public IReadOnlyCollection<Category> Categories
    {
      get => (IReadOnlyCollection<Category>)GetValue(CategoriesProperty);
      set => SetValue(CategoriesProperty, value);
    }

    public IReadOnlyCollection<Group> Groups
    {
      get => (IReadOnlyCollection<Group>)GetValue(GroupsProperty);
      set => SetValue(GroupsProperty, value);
    }

    public ExpenseEditor(ExpenseViewModel expenseViewModel)
    {
      ViewModel = expenseViewModel;
      InitializeComponent();
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

    private void MenuFlyout_Categories_Opening(object sender, object e)
    {
      if (sender is MenuFlyout menuFlyout && menuFlyout.Items.Count == 0)
      {
        foreach (var category in Categories)
        {
          var mainCategoryMenuItem = new MenuFlyoutSubItem()
          {
            Text = category.Name
          };
          foreach (var subCategory in category.SubCategories)
          {
            mainCategoryMenuItem.Items.Add(new MenuFlyoutItem()
            {
              Text = subCategory.Name,
              Command = ViewModel.SwitchCategoryCommand,
              CommandParameter = subCategory
            });
          }
          menuFlyout.Items.Add(mainCategoryMenuItem);
        }
      }
    }

    private void Button_AddUser_Click(object sender, RoutedEventArgs e)
    {
      if (sender is Button addUserButton && addUserButton.DataContext is User user)
      {
        ViewModel.AddUserCommand?.Execute(user);
        AddUserFlyout.Hide();
      }
    }

    private void MenuFlyoutItem_DeleteShare_Click(object sender, RoutedEventArgs e)
    {
      if (sender is MenuFlyoutItem deleteUserMenuFlyoutItem && deleteUserMenuFlyoutItem.DataContext is ShareViewModel share)
      {
        ViewModel.DeleteShareCommand?.Execute(share);
      }
    }
  }
}
