﻿using CommunityToolkit.Mvvm.ComponentModel;
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
    public Guid Id { get; private set; }

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
        Id = model.Id,
        Index = index + 1,
        InputExpression = model.InputExpression,
        OutputExpressions = new ObservableCollection<OutputExpressionViewModel>(model.OutputExpressions.Select(OutputExpressionViewModel.FromModel))
      };
    }

    public static Rule ToModel(RuleViewModel vm)
    {
      return new Rule()
      {
        Id = vm.Id,
        InputExpression = vm.InputExpression,
        OutputExpressions = vm.OutputExpressions.Select(OutputExpressionViewModel.ToModel).ToList()
      };
    }
  }
}