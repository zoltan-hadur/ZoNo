using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using Unity;
using ZoNo.Contracts.ViewModels;
using ZoNo.Contracts.Views;

namespace ZoNo.Views
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindowV : Window, VBase<IMainWindowVM>, IMainWindowV
  {
    public MainWindowV()
    {
      InitializeComponent();
      //EventAggregator.GetEvent<UserLoggedInEvent >().Subscribe(e => SwitchPages(login: true ));
      //EventAggregator.GetEvent<UserLoggedOutEvent>().Subscribe(e => SwitchPages(login: false));
    }

    [Dependency]
    public IMainWindowVM ViewModel
    {
      get => (IMainWindowVM)DataContext;
      set => DataContext = value;
    }

    //private void SwitchPages(bool login)
    //{
    //  Login.RenderTransform = new TranslateTransform();
    //  Login.RenderTransform.BeginAnimation(TranslateTransform.YProperty, new DoubleAnimation()
    //  {
    //    From = login ? 0 : -ActualHeight,
    //    To = login ? -ActualHeight : 0,
    //    DecelerationRatio = login ? 0.3 : 0.7,
    //    Duration = TimeSpan.FromSeconds(0.7)
    //  });

    //  Home.BeginAnimation(OpacityProperty, new DoubleAnimation()
    //  {
    //    From = login ? 0 : 1,
    //    To = login ? 1 : 0,
    //    DecelerationRatio = 0.7,
    //    Duration = TimeSpan.FromSeconds(0.7)
    //  });
    //}
  }
}
