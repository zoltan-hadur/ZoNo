using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
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
using ZoNo.Models;

namespace ZoNo.ViewModels.Rules
{
  public partial class RulesViewModel : ObservableObject
  {
    private readonly IRulesService _rulesService;
    private readonly RuleType _ruleType;

    private bool _isLoaded = false;

    public ObservableCollection<RuleViewModel> Rules { get; } = new ObservableCollection<RuleViewModel>();

    public RulesViewModel(IRulesService rulesService, RuleType ruleType)
    {
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
    private void NewRule()
    {

    }

    [RelayCommand]
    private void EditRule(RuleViewModel rule)
    {

    }

    [RelayCommand]
    private void DeleteRule(RuleViewModel rule)
    {
      Rules.Remove(rule);
    }
  }
}
