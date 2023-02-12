using CommunityToolkit.Mvvm.ComponentModel;
using System.Diagnostics;

namespace ZoNo.ViewModels.Import
{
  [DebuggerDisplay("ColumnHeader = {ColumnHeader} IsVisible = {IsVisible} IsEnabled = {IsEnabled}")]
  public partial class ColumnViewModel : ObservableObject
  {
    public required ColumnHeader ColumnHeader { get; init; }

    [ObservableProperty]
    private bool _isVisible;

    [ObservableProperty]
    private bool _isEnabled = true;
  }
}