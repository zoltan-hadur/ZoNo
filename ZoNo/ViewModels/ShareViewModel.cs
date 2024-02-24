using CommunityToolkit.Mvvm.ComponentModel;
using ZoNo.Models;

namespace ZoNo.ViewModels
{
  public partial class ShareViewModel : ObservableObject
  {
    public User User { get; set; }

    [ObservableProperty]
    private double _percentage;
  }
}
