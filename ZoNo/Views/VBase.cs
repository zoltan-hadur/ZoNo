using System;
using System.Collections.Generic;
using System.Text;

namespace ZoNo.Views
{
  public interface VBase<T> where T : class
  {
    T ViewModel { get; set; }
  }
}
