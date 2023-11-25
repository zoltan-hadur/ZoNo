using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
