using System;
using System.Collections.Generic;
using System.Text;
using Unity;
using ZoNo.Contracts.ViewModels;

namespace ZoNo.ViewModels
{
  public class MainWindowVM : VMBase, IMainWindowVM
  {
    private ILoginVM mLoginVM;
    [Dependency]
    public ILoginVM LoginVM
    {
      get => mLoginVM;
      set => Set(ref mLoginVM, value);
    }

    private IHomeVM mHomeVM;
    [Dependency]
    public IHomeVM HomeVM
    {
      get => mHomeVM;
      set => Set(ref mHomeVM, value);
    }
  }
}
