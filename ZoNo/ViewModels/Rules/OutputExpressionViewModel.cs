using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoNo.ViewModels.Rules
{
  public partial class OutputExpressionViewModel : ObservableObject
  {
    [ObservableProperty]
    private int _index;

    [ObservableProperty]
    private string _value;
  }
}
