using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZoNo.Contracts.Services;
using ZoNo.Models;

namespace ZoNo.ViewModels.Rules
{
  public partial class RulesViewModel : ObservableObject
  {
    private readonly IRulesService _rulesService;

    public ObservableCollection<RuleViewModel> ImportRules { get; }
    public ObservableCollection<RuleViewModel> SplitwiseRules { get; }

    public RulesViewModel(IRulesService rulesService)
    {
      _rulesService = rulesService;
      ImportRules = new ObservableCollection<RuleViewModel>(_rulesService.GetRules(RuleType.Import).Select((rule, index) => new RuleViewModel(rule)
      {
        Index = index + 1
      }));
      SplitwiseRules = new ObservableCollection<RuleViewModel>(_rulesService.GetRules(RuleType.Splitwise).Select((rule, index) => new RuleViewModel(rule)
      {
        Index = index + 1
      }));

      ImportRules.CollectionChanged += ImportRules_CollectionChanged;
      SplitwiseRules.CollectionChanged += SplitwiseRules_CollectionChanged;
    }

    private void ImportRules_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
      for (int i = 0; i < ImportRules.Count; ++i)
      {
        ImportRules[i].Index = i + 1;
      }
    }

    private void SplitwiseRules_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
      for (int i = 0; i < SplitwiseRules.Count; ++i)
      {
        SplitwiseRules[i].Index = i + 1;
      }
    }

    [RelayCommand]
    private async void NewRule(RuleType type)
    {

    }

    [RelayCommand]
    private async void EditRule(RuleViewModel rule)
    {

    }

    [RelayCommand]
    private async void DeleteRule(RuleViewModel rule)
    {

    }
  }
}
