using Prism.Events;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace ZoNo.Contracts.ViewModels
{
  public interface IVMBase : INotifyPropertyChanged
  {
    IEventAggregator EventAggregator { get; }
    ISettings Settings { get; }
  }
}
