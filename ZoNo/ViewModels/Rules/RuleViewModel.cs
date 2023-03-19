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
    public Guid Id { get; }

    [ObservableProperty]
    private RuleType _type;

    [ObservableProperty]
    private int _index;

    [ObservableProperty]
    private string _inputExpression;

    [ObservableProperty]
    private ObservableCollection<OutputExpressionViewModel> _outputExpressions;

    public RuleViewModel(Rule rule)
    {
      Id = rule.Id;
      _type = rule.Type;
      _inputExpression = rule.InputExpression;
      _outputExpressions = new ObservableCollection<OutputExpressionViewModel>(rule.OutputExpressions.Select((expression, index) => new OutputExpressionViewModel()
      {
        Index = index + 1,
        Value = expression
      }));
    }
  }
}
