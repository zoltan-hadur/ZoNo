using CommunityToolkit.Mvvm.ComponentModel;

namespace ZoNo.ViewModels
{
  public partial class OutputExpressionViewModel : ObservableObject
  {
    [ObservableProperty]
    private int _index;

    [ObservableProperty]
    private string _outputExpression = string.Empty;

    [ObservableProperty]
    private bool _isSyntaxValid = false;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    public static OutputExpressionViewModel FromModel(string outputExpression, int index)
    {
      return new OutputExpressionViewModel
      {
        Index = index + 1,
        OutputExpression = outputExpression,
        IsSyntaxValid = true
      };
    }

    public static string ToModel(OutputExpressionViewModel vm)
    {
      return vm.OutputExpression;
    }
  }
}
