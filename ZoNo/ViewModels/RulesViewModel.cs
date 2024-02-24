using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using ZoNo.Contracts.Services;
using ZoNo.Models;
using ZoNo.Views;

namespace ZoNo.ViewModels
{
  public partial class RulesViewModel(
    IRuleExpressionSyntaxCheckerService ruleExpressionSyntaxCheckerService,
    IDialogService dialogService,
    IRulesService rulesService,
    RuleType ruleType) : ObservableObject
  {
    private readonly IRuleExpressionSyntaxCheckerService _ruleExpressionSyntaxCheckerService = ruleExpressionSyntaxCheckerService;
    private readonly IDialogService _dialogService = dialogService;
    private readonly IRulesService _rulesService = rulesService;
    private readonly RuleType _ruleType = ruleType;

    public ObservableCollection<RuleViewModel> Rules { get; } = [];

    public void Load()
    {
      Rules.CollectionChanged -= Rules_CollectionChanged;
      Rules.Clear();
      var rules = _rulesService.GetRules(_ruleType);
      foreach (var rule in rules.Select(RuleViewModel.FromModel))
      {
        Rules.Add(rule);
      }
      Rules.CollectionChanged += Rules_CollectionChanged;
    }

    private async void Rules_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
      for (int i = 0; i < Rules.Count; ++i)
      {
        Rules[i].Index = i + 1;
      }
      var rules = Rules.Select(RuleViewModel.ToModel).ToArray();
      await _rulesService.SaveRulesAsync(_ruleType, rules);
    }

    private async void Rule_InputExpressionChanged(object sender, string e)
    {
      if (sender is RuleViewModel rule)
      {
        switch (_ruleType)
        {
          case RuleType.Transaction:
          case RuleType.Expense:
            (rule.IsInputSyntaxValid, rule.ErrorMessage) = await _ruleExpressionSyntaxCheckerService.CheckSyntaxAsync<Transaction>(e);
            break;
        }
      }
    }

    private async void Rule_OutputExpressionChanged(object sender, string e)
    {
      if (sender is OutputExpressionViewModel outputExpression)
      {
        switch (_ruleType)
        {
          case RuleType.Transaction:
            (outputExpression.IsSyntaxValid, outputExpression.ErrorMessage) = await _ruleExpressionSyntaxCheckerService.CheckSyntaxAsync<Transaction, Transaction>(e);
            break;
          case RuleType.Expense:
            (outputExpression.IsSyntaxValid, outputExpression.ErrorMessage) = await _ruleExpressionSyntaxCheckerService.CheckSyntaxAsync<Transaction, Expense>(e);
            break;
        }
      }
    }

    [RelayCommand]
    private async Task NewRule()
    {
      var rule = new RuleViewModel()
      {
        Index = Rules.Count + 1,
        OutputExpressions = [new() { Index = 1 }]
      };
      rule.InputExpressionChanged += Rule_InputExpressionChanged;
      rule.OutputExpressionChanged += Rule_OutputExpressionChanged;
      var isPrimaryButtonEnabledBinding = new Binding()
      {
        Path = new PropertyPath(nameof(RuleViewModel.IsSyntaxValid)),
        Mode = BindingMode.OneWay,
        Source = rule
      };
      var result = await _dialogService.ShowDialogAsync(DialogType.SaveCancel, $"New {_ruleType} Rule", new RuleEditor(rule), isPrimaryButtonEnabledBinding);
      if (result)
      {
        Rules.Add(rule);
      }
      rule.InputExpressionChanged -= Rule_InputExpressionChanged;
      rule.OutputExpressionChanged -= Rule_OutputExpressionChanged;
    }

    [RelayCommand]
    private async Task EditRule(RuleViewModel rule)
    {
      var index = Rules.IndexOf(rule);
      var copiedRule = RuleViewModel.FromModel(RuleViewModel.ToModel(rule), index);
      copiedRule.InputExpressionChanged += Rule_InputExpressionChanged;
      copiedRule.OutputExpressionChanged += Rule_OutputExpressionChanged;
      var isPrimaryButtonEnabledBinding = new Binding()
      {
        Path = new PropertyPath(nameof(RuleViewModel.IsSyntaxValid)),
        Mode = BindingMode.OneWay,
        Source = copiedRule
      };
      var result = await _dialogService.ShowDialogAsync(DialogType.SaveCancel, $"Edit {_ruleType} Rule", new RuleEditor(copiedRule), isPrimaryButtonEnabledBinding);
      if (result)
      {
        Rules[index] = copiedRule;
      }
      copiedRule.InputExpressionChanged -= Rule_InputExpressionChanged;
      copiedRule.OutputExpressionChanged -= Rule_OutputExpressionChanged;
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
