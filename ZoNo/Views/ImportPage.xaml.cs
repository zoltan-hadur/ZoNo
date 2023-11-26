﻿using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ZoNo.ViewModels;

namespace ZoNo.Views
{
  public sealed partial class ImportPage : Page
  {
    public ImportPageViewModel ViewModel { get; }

    public ImportPage()
    {
      ViewModel = App.GetService<ImportPageViewModel>();
      InitializeComponent();
    }

    private async void Page_Loading(FrameworkElement sender, object args)
    {
      await ViewModel.TransactionsViewModel.Load();
      await ViewModel.ExpensesViewModel.Load();
    }

    private void Expenses_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      if (e.AddedItems.Count == 0) return;
      Expenses.ScrollIntoView(e.AddedItems[0]);
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
  }
}