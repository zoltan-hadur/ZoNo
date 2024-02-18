using CommunityToolkit.WinUI.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Tracer.Contracts;
using ZoNo.Contracts.Services;

namespace ZoNo.Services
{
  public class DialogService(
    IThemeSelectorService themeSelectorService,
    ITraceFactory traceFactory) : IDialogService
  {
    private readonly IThemeSelectorService _themeSelectorService = themeSelectorService;
    private readonly ITraceFactory _traceFactory = traceFactory;

    public async Task<bool> ShowDialogAsync<T>(DialogType dialogType, string title, T content, Binding isPrimaryButtonEnabled = null, Func<Task<bool>> shouldCloseDialogOnPrimaryButtonClick = null)
    {
      using var trace = _traceFactory.CreateNew();
      trace.Debug(Format([dialogType, title, content, isPrimaryButtonEnabled, shouldCloseDialogOnPrimaryButtonClick]));

      var dialog = new ContentDialog()
      {
        XamlRoot = App.MainWindow.Content.XamlRoot,
        RequestedTheme = _themeSelectorService.Theme,
        Title = title,
        Content = content,
      };

      switch (dialogType)
      {
        case DialogType.Ok:
          dialog.PrimaryButtonText = "OK";
          break;
        case DialogType.Close:
          dialog.PrimaryButtonText = "Close";
          break;
        case DialogType.OkCancel:
          dialog.PrimaryButtonText = "OK";
          dialog.CloseButtonText = "Cancel";
          break;
        case DialogType.SaveCancel:
          dialog.PrimaryButtonText = "Save";
          dialog.CloseButtonText = "Cancel";
          break;
        case DialogType.SaveClose:
          dialog.PrimaryButtonText = "Save";
          dialog.CloseButtonText = "Close";
          break;
        default:
          throw new ArgumentException($"Value {dialogType} is not valid", nameof(dialogType));
      }

      // Fix for show animation
      void ContentDialog_SizeChanged(object sender, SizeChangedEventArgs e)
      {
        if (sender is ContentDialog dialog)
        {
          trace.Debug("Start dialog show animation");
          var border = dialog.FindDescendant("Container") as Border;
          var groups = VisualStateManager.GetVisualStateGroups(border);
          var states = groups.Single(x => x.Name == "DialogShowingStates");
          var transition = states.Transitions.Single(x => x.To == "DialogShowing");
          transition.Storyboard.Begin();
          dialog.SizeChanged -= ContentDialog_SizeChanged;
        }
      }
      dialog.SizeChanged += ContentDialog_SizeChanged;

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

      if (isPrimaryButtonEnabled is not null)
      {
        dialog.SetBinding(ContentDialog.IsPrimaryButtonEnabledProperty, isPrimaryButtonEnabled);
      }

      if (!string.IsNullOrEmpty(dialog.PrimaryButtonText) && !string.IsNullOrEmpty(dialog.CloseButtonText) && shouldCloseDialogOnPrimaryButtonClick is not null)
      {
        dialog.Closing += async (s, e) =>
        {
          if (e.Result == ContentDialogResult.Primary)
          {
            trace.Debug("Dialog closing");
            e.Cancel = true;
            var deferral = e.GetDeferral();
            var shouldClose = await shouldCloseDialogOnPrimaryButtonClick();
            e.Cancel = !shouldClose;
            deferral.Complete();
            trace.Debug(Format([shouldClose]));
          }
        };
      }

      var result = await dialog.ShowAsync();
      trace.Debug(Format([result]));
      return result == ContentDialogResult.Primary;
    }
  }
}
