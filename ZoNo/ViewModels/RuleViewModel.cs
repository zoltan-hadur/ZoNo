using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using ZoNo.Helpers;
using ZoNo.Models;

namespace ZoNo.ViewModels
{
  public partial class RuleViewModel : ObservableObject
  {
    public Guid Id { get; set; } = Guid.NewGuid();

    [ObservableProperty]
    private int _index;

    [ObservableProperty]
    private string _description = string.Empty;

    [ObservableProperty]
    private string _inputExpression = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsSyntaxValid))]
    private bool _isInputSyntaxValid = false;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    public bool IsOutputExpressionsSyntaxValid => OutputExpressions?.All(outputExpression => outputExpression.IsSyntaxValid) ?? false;
    public bool IsSyntaxValid => IsInputSyntaxValid && IsOutputExpressionsSyntaxValid;

    [ObservableProperty]
    private ObservableCollection<OutputExpressionViewModel> _outputExpressions = [];

    public event EventHandler<string> InputExpressionChanged;
    public event EventHandler<string> OutputExpressionChanged;

    partial void OnInputExpressionChanged(string value)
    {
      InputExpressionChanged?.Invoke(this, value);
    }

    partial void OnOutputExpressionsChanged(ObservableCollection<OutputExpressionViewModel> oldValue, ObservableCollection<OutputExpressionViewModel> newValue)
    {
      if (oldValue != null)
      {
        oldValue.CollectionChanged -= OutputExpressions_CollectionChanged;
        foreach (var outputExpression in oldValue)
        {
          outputExpression.PropertyChanged -= OutputExpression_PropertyChanged;
        }
      }
      if (newValue != null)
      {
        newValue.CollectionChanged += OutputExpressions_CollectionChanged;
        foreach (var outputExpression in newValue)
        {
          outputExpression.PropertyChanged += OutputExpression_PropertyChanged;
        }
      }
      OnPropertyChanged(nameof(IsOutputExpressionsSyntaxValid));
      OnPropertyChanged(nameof(IsSyntaxValid));
    }

    private void OutputExpressions_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
      foreach (OutputExpressionViewModel outputExpression in e.OldItems.OrEmpty())
      {
        outputExpression.PropertyChanged -= OutputExpression_PropertyChanged;
      }
      foreach (OutputExpressionViewModel outputExpression in e.NewItems.OrEmpty())
      {
        outputExpression.PropertyChanged += OutputExpression_PropertyChanged;
      }
      for (int i = 0; i < OutputExpressions.Count; ++i)
      {
        OutputExpressions[i].Index = i + 1;
      }
      OnPropertyChanged(nameof(IsOutputExpressionsSyntaxValid));
      OnPropertyChanged(nameof(IsSyntaxValid));
    }

    private void OutputExpression_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      switch (e.PropertyName)
      {
        case nameof(OutputExpressionViewModel.OutputExpression):
          {
            OutputExpressionChanged?.Invoke(sender, (sender as OutputExpressionViewModel).OutputExpression);
          }
          break;
        case nameof(OutputExpressionViewModel.IsSyntaxValid):
          {
            OnPropertyChanged(nameof(IsOutputExpressionsSyntaxValid));
            OnPropertyChanged(nameof(IsSyntaxValid));
          }
          break;
      }
    }

    public static RuleViewModel FromModel(Rule model, int index)
    {
      return new RuleViewModel()
      {
        Id = model.Id,
        Index = index + 1,
        Description = model.Description,
        InputExpression = model.InputExpression,
        IsInputSyntaxValid = true,
        OutputExpressions = new ObservableCollection<OutputExpressionViewModel>(model.OutputExpressions.Select(OutputExpressionViewModel.FromModel))
      };
    }

    public static Rule ToModel(RuleViewModel vm)
    {
      return new Rule()
      {
        Id = vm.Id,
        Description = vm.Description,
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
