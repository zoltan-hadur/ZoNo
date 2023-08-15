using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.Xaml.Interactivity;

namespace ZoNo.Controls
{
  public class ValidationTextBox : TextBox
  {
    private object? _defaultTextControlBorderBrushDisabled = null;
    private object? _defaultTextControlBorderBrushPointerOver = null;
    private object? _defaultTextControlBorderBrushFocused = null;

    public static readonly DependencyProperty IsValidProperty = DependencyProperty.Register(nameof(IsValid), typeof(bool), typeof(ValidationTextBox), new PropertyMetadata(false, OnIsValidChanged));
    public static readonly DependencyProperty ErrorMessageProperty = DependencyProperty.Register(nameof(ErrorMessage), typeof(string), typeof(ValidationTextBox), new PropertyMetadata(string.Empty, OnErrorMessageChanged));

    public bool IsValid
    {
      get => (bool)GetValue(IsValidProperty);
      set => SetValue(IsValidProperty, value);
    }

    public string ErrorMessage
    {
      get => (string)GetValue(ErrorMessageProperty);
      set => SetValue(ErrorMessageProperty, value);
    }

    public ValidationTextBox()
    {
      Loaded += ValidationTextBox_Loaded;
    }

    private void ValidationTextBox_Loaded(object sender, RoutedEventArgs e)
    {
      var visualStateGroups = VisualStateUtilities.GetVisualStateGroups(this);
      var visualStateGroup = visualStateGroups.First(visualStateGroup => visualStateGroup.Name == "CommonStates");
      var visualStateDisabled = visualStateGroup.States.First(state => state.Name == "Disabled");
      var visualStatePointerOver = visualStateGroup.States.First(state => state.Name == "PointerOver");
      var visualStateFocused = visualStateGroup.States.First(state => state.Name == "Focused");

      _defaultTextControlBorderBrushDisabled = GetBorderBrushFromVisualState(visualStateDisabled);
      _defaultTextControlBorderBrushPointerOver = GetBorderBrushFromVisualState(visualStatePointerOver);
      _defaultTextControlBorderBrushFocused = GetBorderBrushFromVisualState(visualStateFocused);

      UpdateBorderColor();
      UpdateToolTip();
    }

    private static void OnIsValidChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      if (d is ValidationTextBox validationTextBox && validationTextBox.IsLoaded)
      {
        validationTextBox.UpdateBorderColor();
        validationTextBox.UpdateToolTip();
      }
    }

    private static void OnErrorMessageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      if (d is ValidationTextBox validationTextBox && validationTextBox.IsLoaded)
      {
        validationTextBox.UpdateToolTip();
      }
    }

    private void UpdateBorderColor()
    {
      if (!IsValid)
      {
        BorderBrush = new SolidColorBrush(Colors.Red);
        Resources["TextControlBorderBrushDisabled"] = new SolidColorBrush(Colors.Red) { Opacity = 0.5 };
        Resources["TextControlBorderBrushPointerOver"] = new SolidColorBrush(Colors.Red);
        Resources["TextControlBorderBrushFocused"] = new SolidColorBrush(Colors.Red);
      }
      else
      {
        ClearValue(BorderBrushProperty);
        Resources["TextControlBorderBrushDisabled"] = _defaultTextControlBorderBrushDisabled;
        Resources["TextControlBorderBrushPointerOver"] = _defaultTextControlBorderBrushPointerOver;
        Resources["TextControlBorderBrushFocused"] = _defaultTextControlBorderBrushFocused;
      }

      // Force resource reload
      if (ActualTheme == ElementTheme.Light)
      {
        RequestedTheme = ElementTheme.Dark;
        RequestedTheme = ElementTheme.Light;
      }
      else if (ActualTheme == ElementTheme.Dark)
      {
        RequestedTheme = ElementTheme.Light;
        RequestedTheme = ElementTheme.Dark;
      }
      else
      {
        RequestedTheme = ElementTheme.Light;
        RequestedTheme = ElementTheme.Dark;
        RequestedTheme = ElementTheme.Default;
      }
    }

    private void UpdateToolTip()
    {
      if (!IsValid && !string.IsNullOrEmpty(ErrorMessage))
      {
        SetValue(ToolTipService.ToolTipProperty, ErrorMessage);
      }
      else
      {
        ClearValue(ToolTipService.ToolTipProperty);
      }
    }

    private object GetBorderBrushFromVisualState(VisualState visualState)
    {
      return ((ObjectAnimationUsingKeyFrames)visualState.Storyboard.Children.First(child =>
        child is ObjectAnimationUsingKeyFrames animation &&
        animation.GetValue(Storyboard.TargetNameProperty) is string targetName &&
        targetName == "BorderElement" &&
        animation.GetValue(Storyboard.TargetPropertyProperty) is string targetProperty &&
        targetProperty == nameof(Border.BorderBrush)
      )).KeyFrames.First().Value;
    }
  }
}
