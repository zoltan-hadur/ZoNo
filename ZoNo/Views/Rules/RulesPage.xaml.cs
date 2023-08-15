using Microsoft.UI.Xaml.Controls;
using ZoNo.ViewModels.Rules;

namespace ZoNo.Views.Rules
{
  public sealed partial class RulesPage : Page
  {
    public RulesPageViewModel ViewModel { get; }

    public RulesPage()
    {
      ViewModel = App.GetService<RulesPageViewModel>();
      InitializeComponent();
    }

    private async void Page_Loading(Microsoft.UI.Xaml.FrameworkElement sender, object args)
    {
      await ViewModel.ImportRulesViewModel.Load();
      await ViewModel.SplitwiseRulesViewModel.Load();
    }
  }
}
