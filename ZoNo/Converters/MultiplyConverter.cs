using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace ZoNo.Converters
{
  public class MultiplyConverter : IMultiValueConverter
  {
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
      if (values != null && values.All(wValue => wValue is double))
      {
        return values.Select(wValue => System.Convert.ToDouble(wValue)).Aggregate((wLeft, wRight) => wLeft * wRight);
      }
      return DependencyProperty.UnsetValue;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}
