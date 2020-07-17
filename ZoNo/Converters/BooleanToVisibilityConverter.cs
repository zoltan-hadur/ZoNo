using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace ZoNo.Converters
{
  public class BooleanToVisibilityConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value is bool wValue)
      {
        if (parameter is "inverse")
        {
          return wValue ? Visibility.Collapsed : Visibility.Visible;
        }
        return wValue ? Visibility.Visible : Visibility.Collapsed;
      }
      return DependencyProperty.UnsetValue;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value is Visibility wValue)
      {
        if (parameter is "inverse")
        {
          return wValue != Visibility.Visible;
        }
        return wValue == Visibility.Visible;
      }
      return DependencyProperty.UnsetValue;
    }
  }
}
