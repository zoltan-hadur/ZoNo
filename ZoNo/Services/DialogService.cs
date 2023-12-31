﻿using CommunityToolkit.WinUI.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using ZoNo.Contracts.Services;

namespace ZoNo.Services
{
  public class DialogService(
    IThemeSelectorService themeSelectorService) : IDialogService
  {
    private readonly IThemeSelectorService _themeSelectorService = themeSelectorService;

    public async Task<DialogResult> ShowDialogAsync<T>(DialogType type, string title, T content, Binding isPrimaryButtonEnabled = null)
    {
      var dialog = new ContentDialog()
      {
        XamlRoot = App.MainWindow.Content.XamlRoot,
        RequestedTheme = _themeSelectorService.Theme,
        Title = title,
        PrimaryButtonText = "OK",
        CloseButtonText = type == DialogType.OkCancel ? "Cancel" : string.Empty,
        Content = content
      };

      dialog.SizeChanged += (s, e) =>
      {
        var border = dialog.FindDescendant("Container") as Border;
        var groups = VisualStateManager.GetVisualStateGroups(border);
        var states = groups.Single(x => x.Name == "DialogShowingStates");
        var transition = states.Transitions.Single(x => x.To == "DialogShowing");
        transition.Storyboard.Begin();
      };

      dialog.Resources["ContentDialogMinWidth"] = 0.0;
      dialog.Resources["ContentDialogMinHeight"] = 0.0;
      dialog.Resources["ContentDialogMaxWidth"] = double.PositiveInfinity;
      dialog.Resources["ContentDialogMaxHeight"] = double.PositiveInfinity;
      dialog.Resources["ContentDialogPadding"] = new Thickness(12);
      dialog.Loaded += (s, e) =>
      {
        if (dialog.FindDescendant("LayoutRoot") is Grid grid)
        {
          grid.Margin = new Thickness(6, 40, 6, 6);
        }
      };
      if (isPrimaryButtonEnabled != null)
      {
        dialog.SetBinding(ContentDialog.IsPrimaryButtonEnabledProperty, isPrimaryButtonEnabled);
      }
      var result = await dialog.ShowAsync();
      return result == ContentDialogResult.Primary ? DialogResult.Ok : DialogResult.Cancel;
    }
  }
}
