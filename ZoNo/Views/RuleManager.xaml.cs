using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System.Collections.ObjectModel;
using System.Windows.Input;
using ZoNo.ViewModels;

namespace ZoNo.Views
{
  public sealed partial class RuleManager : UserControl
  {
    public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register(nameof(Header), typeof(string), typeof(RuleManager), null);
    public static readonly DependencyProperty RulesProperty = DependencyProperty.Register(nameof(Rules), typeof(ObservableCollection<RuleViewModel>), typeof(RuleManager), null);
    public static readonly DependencyProperty NewRuleCommandProperty = DependencyProperty.Register(nameof(NewRuleCommand), typeof(ICommand), typeof(RuleManager), null);
    public static readonly DependencyProperty EditRuleCommandProperty = DependencyProperty.Register(nameof(EditRuleCommand), typeof(ICommand), typeof(RuleManager), null);
    public static readonly DependencyProperty DuplicateRuleCommandProperty = DependencyProperty.Register(nameof(DuplicateRuleCommand), typeof(ICommand), typeof(RuleManager), null);
    public static readonly DependencyProperty DeleteRuleCommandProperty = DependencyProperty.Register(nameof(DeleteRuleCommand), typeof(ICommand), typeof(RuleManager), null);

    public string Header
    {
      get => (string)GetValue(HeaderProperty);
      set => SetValue(HeaderProperty, value);
    }

    public ObservableCollection<RuleViewModel> Rules
    {
      get => (ObservableCollection<RuleViewModel>)GetValue(RulesProperty);
      set => SetValue(RulesProperty, value);
    }

    public ICommand NewRuleCommand
    {
      get => (ICommand)GetValue(NewRuleCommandProperty);
      set => SetValue(NewRuleCommandProperty, value);
    }

    public ICommand EditRuleCommand
    {
      get => (ICommand)GetValue(EditRuleCommandProperty);
      set => SetValue(EditRuleCommandProperty, value);
    }

    public ICommand DuplicateRuleCommand
    {
      get => (ICommand)GetValue(DuplicateRuleCommandProperty);
      set => SetValue(DuplicateRuleCommandProperty, value);
    }

    public ICommand DeleteRuleCommand
    {
      get => (ICommand)GetValue(DeleteRuleCommandProperty);
      set => SetValue(DeleteRuleCommandProperty, value);
    }

    public RuleManager()
    {
      InitializeComponent();
    }

    private void ListView_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
    {
      if (e.OriginalSource is FrameworkElement element)
      {
        EditRuleCommand?.Execute(element.DataContext);
      }
    }
  }
}
