using Microsoft.UI.Xaml.Data;

namespace ZoNo.Contracts.Services
{
  public interface IDialogService
  {
    Task<bool> ShowDialogAsync<T>(DialogType dialogType, string title, T content, Binding isPrimaryButtonEnabled = null, Func<Task<bool>> shouldCloseDialogOnPrimaryButtonClick = null);
  }
}
