using Prism.Events;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using ZoNo.Contracts;
using ZoNo.Contracts.ViewModels;

namespace ZoNo.ViewModels
{
  public abstract class VMBase : IVMBase
  {
    public IEventAggregator EventAggregator { get; set; }
    public ISettings Settings { get; set; }

    public event PropertyChangedEventHandler PropertyChanged;

    protected void Set<T>(ref T target, T value, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = "")
    {
      if (Equals(target, value)) return;
      target = value;
      NotifyPropertyChanged(propertyName);
    }

    protected void NotifyPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = "")
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
  }
}
