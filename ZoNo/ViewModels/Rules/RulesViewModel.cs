using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Globalization.DateTimeFormatting;
using ZoNo.Contracts.Services;
using ZoNo.Helpers;
using ZoNo.Models;
using ZoNo.Services;
using ZoNo.Views.Rules;

namespace ZoNo.ViewModels.Rules
{
  public partial class RulesViewModel : ObservableObject
  {
    private readonly IDialogService _dialogService;
    private readonly IRulesService _rulesService;
    private readonly RuleType _ruleType;

    private bool _isLoaded = false;

    public ObservableCollection<RuleViewModel> Rules { get; } = new ObservableCollection<RuleViewModel>();

    public RulesViewModel(IDialogService dialogService, IRulesService rulesService, RuleType ruleType)
    {
      _dialogService = dialogService;
      _rulesService = rulesService;
      _ruleType = ruleType;
    }

    public async Task Load()
    {
      if (_isLoaded) return;

      var rules = await _rulesService.GetRulesAsync(_ruleType);
      foreach (var rule in rules.Select(RuleViewModel.FromModel))
      {
        Rules.Add(rule);
      }
      Rules.CollectionChanged += Rules_CollectionChanged;

      _isLoaded = true;
    }

    private async void Rules_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
      for (int i = 0; i < Rules.Count; ++i)
      {
        Rules[i].Index = i + 1;
      }
      var rules = Rules.Select(RuleViewModel.ToModel).ToArray();
      await _rulesService.SaveRulesAsync(_ruleType, rules);
    }

    [RelayCommand]
    private async void NewRule()
    {
      var rule = new RuleViewModel()
      {
        Index = Rules.Count + 1,
        OutputExpressions = new ObservableCollection<OutputExpressionViewModel> { new OutputExpressionViewModel() { Index = 1 } }
      };
      var ok = await _dialogService.ShowDialogAsync($"RuleEditor_{_ruleType}_New".GetLocalized(), new RuleEditor(rule));
      if (ok)
      {
        Rules.Add(rule);
      }
    }

    [RelayCommand]
    private async void EditRule(RuleViewModel rule)
    {
      var index = Rules.IndexOf(rule);
      var copiedRule = RuleViewModel.FromModel(RuleViewModel.ToModel(rule), index);
      var ok = await _dialogService.ShowDialogAsync($"RuleEditor_{_ruleType}_Edit".GetLocalized(), new RuleEditor(copiedRule));
      if (ok)
      {
        Rules[index] = copiedRule;
      }
    }

    [RelayCommand]
    private void DuplicateRule(RuleViewModel rule)
    {
      var index = Rules.IndexOf(rule);
      var copiedRule = RuleViewModel.FromModel(RuleViewModel.ToModel(rule), index);
      copiedRule.Id = Guid.NewGuid();
      Rules.Insert(index + 1, copiedRule);
    }

    [RelayCommand]
    private void DeleteRule(RuleViewModel rule)
    {
      Rules.Remove(rule);
    }
  }
}
