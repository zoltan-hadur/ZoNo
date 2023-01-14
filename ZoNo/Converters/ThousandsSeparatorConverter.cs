using Microsoft.UI.Xaml.Data;

namespace ZoNo.Converters
{
  public class ThousandsSeparatorConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, string language)
    {
      if (value is sbyte
       || value is byte
       || value is short
       || value is ushort
       || value is int
       || value is uint
       || value is long
       || value is ulong
       || value is float
       || value is double
       || value is decimal)
      {
        return System.Convert.ToDouble(value).ToString("N");
      }
      else
      {
        throw new ArgumentException("Value is not a number!", nameof(value));
      }
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
      throw new NotSupportedException();
    }
  }
}
