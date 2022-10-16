using CommunityToolkit.Mvvm.ComponentModel;

namespace ZoNo2.ViewModels;

public class QueryViewModel : ObservableRecipient
{
  public string GUID = Guid.NewGuid().ToString();

  public QueryViewModel()
  {

  }
}
