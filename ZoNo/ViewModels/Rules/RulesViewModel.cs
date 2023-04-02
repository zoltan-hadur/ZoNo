using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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

    public ObservableCollection<RuleViewModel>? Rules { get; private set; }

    public RulesViewModel(IRulesService rulesService)
    {
      _rulesService = rulesService;
    }

    public async Task Load()
    {
      var rules = await _rulesService.GetRulesAsync();
      Rules = new ObservableCollection<RuleViewModel>(rules.Select(RuleViewModel.FromModel));
      Rules.CollectionChanged += Rules_CollectionChanged;
    }

    private async void Rules_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
      for (int i = 0; i < Rules!.Count; ++i)
      {
        Rules[i].Index = i + 1;
      }
      await _rulesService.SaveRulesAsync(Rules!.Select(RuleViewModel.ToModel));
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
      Rules!.Remove(rule);
    }
  }
}
