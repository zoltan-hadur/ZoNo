using CommunityToolkit.Mvvm.ComponentModel;

namespace ZoNo.ViewModels.Import
{
  public partial class ColumnViewModel : ObservableObject
  {
    [ObservableProperty]
    private ColumnHeader _columnHeader;

    [ObservableProperty]
    private bool _isVisible;

    [ObservableProperty]
    private bool _isEnabled = true;
  }
}