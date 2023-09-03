using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using ZoNo.Converters;
using ZoNo.Models;
using ZoNo.ViewModels.Import;

namespace ZoNo.Views.Import
{
  public sealed partial class ExpenseEditor : UserControl
  {
    public ExpenseViewModel ViewModel { get; }

    public static readonly DependencyProperty CategoriesProperty = DependencyProperty.Register(nameof(Categories), typeof(Category[]), typeof(ExpenseView), new PropertyMetadata(null));
    public static readonly DependencyProperty GroupsProperty = DependencyProperty.Register(nameof(Groups), typeof(Group[]), typeof(ExpenseView), new PropertyMetadata(null));

    public Category[] Categories
    {
      get => (Category[])GetValue(CategoriesProperty);
      set => SetValue(CategoriesProperty, value);
    }

    public Group[] Groups
    {
      get => (Group[])GetValue(GroupsProperty);
      set => SetValue(GroupsProperty, value);
    }

    public ExpenseEditor(ExpenseViewModel expenseViewModel)
    {
      ViewModel = expenseViewModel;
      InitializeComponent();
    }

    private void MenuFlyout_Opening(object sender, object e)
    {
      if (sender is MenuFlyout menuFlyout)
      {
        var dataContext = menuFlyout.Target?.DataContext ?? (menuFlyout.Target as ContentControl)?.Content;
        foreach (var item in menuFlyout.Items)
        {
          item.DataContext = dataContext;
        }
      }
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
