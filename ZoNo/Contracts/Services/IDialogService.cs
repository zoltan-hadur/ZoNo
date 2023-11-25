using Microsoft.UI.Xaml.Data;

namespace ZoNo.Contracts.Services
{
  public interface IDialogService
  {
    Task<DialogResult> ShowDialogAsync<T>(DialogType type, string title, T content, Binding isPrimaryButtonEnabled = null);
  }
}
