using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ZoNo.Converters
{
  public class BooleanToVisibilityConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value is bool visible)
      {
        if (parameter is "inverse")
        {
          visible = !visible;
        }
        return visible ? Visibility.Visible : Visibility.Collapsed;
      }
      return DependencyProperty.UnsetValue;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value is Visibility visible)
      {
        if (parameter is "inverse")
        {
          return visible != Visibility.Visible;
        }
        return visible == Visibility.Visible;
      }
      return DependencyProperty.UnsetValue;
    }
  }
}
