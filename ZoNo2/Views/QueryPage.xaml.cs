using Microsoft.UI.Xaml.Controls;

using ZoNo2.ViewModels;

namespace ZoNo2.Views;

public sealed partial class QueryPage : Page
{
  public QueryViewModel ViewModel { get; }

  public QueryPage()
  {
    ViewModel = App.GetService<QueryViewModel>();
    InitializeComponent();
  }
}
