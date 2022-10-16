using CommunityToolkit.Mvvm.ComponentModel;

namespace ZoNo2.ViewModels;

public class ImportViewModel : ObservableRecipient
{
  public string GUID = Guid.NewGuid().ToString();

  public ImportViewModel()
  {

  }
}
