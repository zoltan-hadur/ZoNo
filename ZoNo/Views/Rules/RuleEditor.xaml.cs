using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using ZoNo.ViewModels.Rules;

namespace ZoNo.Views.Rules
{
  public sealed partial class RuleEditor : UserControl
  {
    public RuleViewModel ViewModel { get; }

    public RuleEditor(RuleViewModel rule)
    {
      ViewModel = rule;
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
  }
}
