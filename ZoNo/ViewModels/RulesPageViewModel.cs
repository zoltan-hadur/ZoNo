using CommunityToolkit.Mvvm.ComponentModel;

namespace ZoNo.ViewModels
{
  public class RulesPageViewModel(RulesViewModel importRulesViewModel, RulesViewModel splitwiseRulesViewModel) : ObservableObject
  {
    public RulesViewModel ImportRulesViewModel { get; } = importRulesViewModel;
    public RulesViewModel SplitwiseRulesViewModel { get; } = splitwiseRulesViewModel;
  }
}
