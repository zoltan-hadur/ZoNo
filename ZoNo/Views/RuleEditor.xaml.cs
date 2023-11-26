using Microsoft.UI.Xaml.Controls;
using ZoNo.ViewModels;

namespace ZoNo.Views
{
  public sealed partial class RuleEditor : UserControl
  {
    public RuleViewModel ViewModel { get; }

    public RuleEditor(RuleViewModel rule)
    {
      ViewModel = rule;
      InitializeComponent();
    }
  }
}
