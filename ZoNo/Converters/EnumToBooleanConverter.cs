using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Tracer.Contracts;

namespace ZoNo.Converters
{
  public class EnumToBooleanConverter : IValueConverter
  {
    private readonly ITraceFactory _traceFactory = App.GetService<ITraceFactory>();

    public object Convert(object value, Type targetType, object parameter, string language)
    {
      using var trace = _traceFactory.CreateNew();

      if (parameter is string enumString)
      {
        if (!Enum.IsDefined(typeof(ElementTheme), value))
        {
          throw new ArgumentException("ExceptionEnumToBooleanConverterValueMustBeAnEnum");
        }

        var enumValue = Enum.Parse(typeof(ElementTheme), enumString);

        trace.Debug(Format([value, parameter]));
        return enumValue.Equals(value);
      }

      throw new ArgumentException("ExceptionEnumToBooleanConverterParameterMustBeAnEnumName");
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
      using var trace = _traceFactory.CreateNew();

      if (parameter is string enumString)
      {
        var result = Enum.Parse(typeof(ElementTheme), enumString);
        trace.Debug(Format([parameter, value]));
        return result;
      }

      throw new ArgumentException("ExceptionEnumToBooleanConverterParameterMustBeAnEnumName");
    }
  }
}