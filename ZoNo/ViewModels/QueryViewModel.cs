using CommunityToolkit.Mvvm.ComponentModel;

namespace ZoNo.ViewModels
{
  public class QueryViewModel : ObservableRecipient
  {
    private static int _id = 0;
    public int Id { get; } = _id++;

    public QueryViewModel()
    {

    }
  }
}