using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ZoNo.ViewModels
{
  public partial class NotificationViewModel : ObservableObject
  {
    public string Title { get; init; }
    public string Description { get; init; }
    public Action Action { get; init; }

    [RelayCommand]
    private void OnClick()
    {
      Action?.Invoke();
    }
  }
}
