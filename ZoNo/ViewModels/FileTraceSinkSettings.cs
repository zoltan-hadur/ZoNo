using CommunityToolkit.Mvvm.ComponentModel;
using Tracer;

namespace ZoNo.ViewModels
{
  public partial class FileTraceSinkSettings : ObservableObject
  {
    [ObservableProperty]
    private bool _isEnabled;
    [ObservableProperty]
    private TraceLevel _level;
    [ObservableProperty]
    private string _path;
  }
}