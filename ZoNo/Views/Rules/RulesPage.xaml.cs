using Microsoft.UI.Xaml.Controls;
using ZoNo.ViewModels.Rules;

namespace ZoNo.Views.Rules
{
  public sealed partial class RulesPage : Page
  {
    public RulesViewModel ViewModel { get; }

    public RulesPage()
    {
      ViewModel = App.GetService<RulesViewModel>();
      InitializeComponent();
    }
  }
}
