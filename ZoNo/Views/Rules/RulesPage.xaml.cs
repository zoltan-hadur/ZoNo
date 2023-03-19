using CommunityToolkit.WinUI.UI;
using CommunityToolkit.WinUI.UI.Animations;
using Microsoft.UI;
using Microsoft.UI.Composition;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Xml.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Foundation.Metadata;
using Windows.UI;
using ZoNo.ViewModels;
using ZoNo.ViewModels.Rules;

namespace ZoNo.Views.Rules
{
  public sealed partial class RulesPage : Page
  {
    public RulesViewModel ViewModel { get; }

    public RulesPage()
    {
      ViewModel = App.GetService<RulesViewModel>();
      InitializeComponent();

      var time = 600;
      importRulesPlaceholder.SizeChanged += (s, e) =>
      {
        AnimationBuilder.Create()
          .Size(
            to: new Vector2(importRulesPlaceholder.ActualSize.X, importRulesPlaceholder.ActualSize.Y),
            duration: TimeSpan.FromMilliseconds(time),
            easingMode: EasingMode.EaseOut,
            layer: FrameworkLayer.Xaml)
          .Offset(
            to: new Vector2(importRulesPlaceholder.ActualOffset.X, importRulesPlaceholder.ActualOffset.Y),
            easingMode: EasingMode.EaseOut,
            duration: TimeSpan.FromMilliseconds(time))
          .Start(importRules);
      };
      splitwiseRulesPlaceholder.SizeChanged += (s, e) =>
      {
        AnimationBuilder.Create()
          .Size(
            to: new Vector2(splitwiseRulesPlaceholder.ActualSize.X, splitwiseRulesPlaceholder.ActualSize.Y),
            duration: TimeSpan.FromMilliseconds(time),
            easingMode: EasingMode.EaseOut,
            layer: FrameworkLayer.Xaml)
          .Offset(
            to: new Vector2(splitwiseRulesPlaceholder.ActualOffset.X, splitwiseRulesPlaceholder.ActualOffset.Y),
            easingMode: EasingMode.EaseOut,
            duration: TimeSpan.FromMilliseconds(time))
          .Start(splitwiseRules);
      };
    }

    private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
    {
      if (e.NewSize.Width > 1280)
      {
        VisualStateManager.GoToState(this, "WideState", false);
      }
      else
      {
        VisualStateManager.GoToState(this, "DefaultState", false);
      }
    }
  }
}
