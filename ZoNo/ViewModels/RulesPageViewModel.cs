using CommunityToolkit.Mvvm.ComponentModel;

namespace ZoNo.ViewModels
{
  public class RulesPageViewModel : ObservableObject
  {
    public RulesViewModel ImportRulesViewModel { get; }
    public RulesViewModel SplitwiseRulesViewModel { get; }

    public RulesPageViewModel(RulesViewModel importRulesViewModel, RulesViewModel splitwiseRulesViewModel)
    {
      ImportRulesViewModel = importRulesViewModel;
      SplitwiseRulesViewModel = splitwiseRulesViewModel;
    }
  }
}
