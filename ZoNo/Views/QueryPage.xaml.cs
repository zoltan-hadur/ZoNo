using Microsoft.UI.Xaml.Controls;
using ZoNo.ViewModels;

namespace ZoNo.Views
{
  public sealed partial class QueryPage : Page
  {
    public QueryPageViewModel ViewModel { get; }

    public QueryPage()
    {
      ViewModel = App.GetService<QueryPageViewModel>();
      InitializeComponent();
    }
  }
}