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
    private string _outputExpression = string.Empty;

    public static OutputExpressionViewModel FromModel(string outputExpression, int index)
    {
      return new OutputExpressionViewModel
      {
        Index = index + 1,
        OutputExpression = outputExpression
      };
    }

    public static string ToModel(OutputExpressionViewModel vm)
    {
      return vm.OutputExpression;
    }
  }
}
