using CommunityToolkit.WinUI.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using ZoNo.Contracts.Services;
using ZoNo.Helpers;

namespace ZoNo.Services
{
  public class DialogService : IDialogService
  {
    private readonly IThemeSelectorService _themeSelectorService;

    public DialogService(IThemeSelectorService themeSelectorService)
    {
      _themeSelectorService = themeSelectorService;
    }

    public async Task<bool> ShowDialogAsync<T>(DialogType type, string title, T content, Binding? isPrimaryButtonEnabled = null)
    {
      var margin = 12;
      var dialog = new ContentDialog()
      {
        XamlRoot = App.MainWindow.Content.XamlRoot,
        RequestedTheme = _themeSelectorService.Theme,
        Title = title,
        PrimaryButtonText = "Dialog_OK".GetLocalized(),
        CloseButtonText = type == DialogType.OkCancel ? "Dialog_Cancel".GetLocalized() : string.Empty,
        Content = content
      };
      dialog.Resources["ContentDialogMinWidth"] = 0.0;
      dialog.Resources["ContentDialogMinHeight"] = 0.0;
      dialog.Resources["ContentDialogMaxWidth"] = double.PositiveInfinity;
      dialog.Resources["ContentDialogMaxHeight"] = double.PositiveInfinity;
      dialog.Resources["ContentDialogPadding"] = new Thickness(margin);
      dialog.Loaded += (s, e) =>
      {
        if (dialog.FindDescendant("BackgroundElement") is Border border)
        {
          border.Margin = new Thickness(margin);
        }
      };
      if (isPrimaryButtonEnabled != null)
      {
        dialog.SetBinding(ContentDialog.IsPrimaryButtonEnabledProperty, isPrimaryButtonEnabled);
      }
      var result = await dialog.ShowAsync();
      return result == ContentDialogResult.Primary;
    }
  }
}
