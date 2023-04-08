using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZoNo.Models;

namespace ZoNo.ViewModels.Rules
{
  public partial class RuleViewModel : ObservableObject
  {
    private Guid _id;

    [ObservableProperty]
    private int _index;

    [ObservableProperty]
    private string _inputExpression = string.Empty;

    [ObservableProperty]
    private ObservableCollection<OutputExpressionViewModel> _outputExpressions = new ObservableCollection<OutputExpressionViewModel>();

    public static RuleViewModel FromModel(Rule model, int index)
    {
      return new RuleViewModel()
      {
        _id = model.Id,
        Index = index + 1,
        InputExpression = model.InputExpression,
        OutputExpressions = new ObservableCollection<OutputExpressionViewModel>(model.OutputExpressions.Select(OutputExpressionViewModel.FromModel))
      };
    }

    public static Rule ToModel(RuleViewModel vm)
    {
      return new Rule()
      {
        Id = vm._id,
        InputExpression = vm.InputExpression,
        OutputExpressions = vm.OutputExpressions.Select(OutputExpressionViewModel.ToModel).ToArray()
      };
    }
  }
}
