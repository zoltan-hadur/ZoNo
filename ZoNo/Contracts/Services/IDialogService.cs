using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoNo.Contracts.Services
{
  public interface IDialogService
  {
    Task<bool> ShowDialogAsync<T>(string title, T content, Binding? isPrimaryButtonEnabled = null);
  }
}
