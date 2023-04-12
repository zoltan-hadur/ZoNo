using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoNo.Contracts.Services
{
  public enum DialogType
  {
    Ok,
    OkCancel
  }

  public interface IDialogService
  {
    Task<bool> ShowDialogAsync<T>(DialogType type, string title, T content, Binding? isPrimaryButtonEnabled = null);
  }
}
