using CommunityToolkit.Mvvm.ComponentModel;
using System.Diagnostics;

namespace ZoNo.ViewModels
{
  [DebuggerDisplay("ColumnHeader = {ColumnHeader} IsVisible = {IsVisible} IsEnabled = {IsEnabled}")]
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