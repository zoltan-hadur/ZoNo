using Microsoft.UI.Xaml.Data;

namespace ZoNo2.Helpers
{
  public class InverseBooleanConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, string language)
    {
      if (value is bool input)
      {
        return !input;
      }
      throw new ArgumentException("Parameter is not bool", nameof(value));
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
      return Convert(value, targetType, parameter, language);
    }
  }
}
