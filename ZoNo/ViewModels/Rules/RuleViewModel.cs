using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZoNo.Models;

namespace ZoNo.ViewModels.Rules
{
  public partial class RuleViewModel : ObservableObject
  {
    private Guid _id = Guid.NewGuid();

    [ObservableProperty]
    private int _index;

    [ObservableProperty]
    private string _inputExpression = string.Empty;

    [ObservableProperty]
    private ObservableCollection<OutputExpressionViewModel> _outputExpressions = new ObservableCollection<OutputExpressionViewModel>();

    partial void OnOutputExpressionsChanging(ObservableCollection<OutputExpressionViewModel> value)
    {
      if (OutputExpressions != null)
      {
        OutputExpressions.CollectionChanged -= OutputExpressions_CollectionChanged;
      }
    }

    partial void OnOutputExpressionsChanged(ObservableCollection<OutputExpressionViewModel> value)
    {
      if (OutputExpressions != null)
      {
        OutputExpressions.CollectionChanged += OutputExpressions_CollectionChanged;
      }
    }

    private void OutputExpressions_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
      for (int i = 0; i < OutputExpressions.Count; ++i)
      {
        OutputExpressions[i].Index = i + 1;
      }
    }

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

    [RelayCommand]
    private void NewOutputExpression()
    {
      OutputExpressions.Add(new OutputExpressionViewModel()
      {
        Index = OutputExpressions.Count + 1
      });
    }

    [RelayCommand]
    private void DeleteOutputExpression(OutputExpressionViewModel outputExpression)
    {
      OutputExpressions.Remove(outputExpression);
    }
  }
}
