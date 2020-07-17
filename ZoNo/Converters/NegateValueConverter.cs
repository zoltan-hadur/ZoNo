using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace ZoNo.Converters
{
  public class NegateValueConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      switch (value)
      {
        case sbyte   wSByte  : return -wSByte;
        case byte    wByte   : return -wByte;
        case short   wShort  : return -wShort;
        case ushort  wUShort : return -wUShort;
        case int     wInt    : return -wInt;
        case uint    wUInt   : return -wUInt;
        case long    wLong   : return -wLong;
        case ulong   wULong  : return -System.Convert.ToInt64(wULong);
        case float   wFloat  : return -wFloat;
        case double  wDouble : return -wDouble;
        case decimal wDecimal: return -wDecimal;
        default: return DependencyProperty.UnsetValue;
      }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      return Convert(value, targetType, parameter, culture);
    }
  }
}
