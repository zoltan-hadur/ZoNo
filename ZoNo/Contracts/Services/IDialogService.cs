using Microsoft.UI.Xaml.Data;

namespace ZoNo.Contracts.Services
{
  public enum DialogType
  {
    Ok,
    OkCancel
  }

  public enum DialogResult
  {
    Ok,
    Cancel
  }

  public interface IDialogService
  {
    Task<DialogResult> ShowDialogAsync<T>(DialogType type, string title, T content, Binding isPrimaryButtonEnabled = null);
  }
}
