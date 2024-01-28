using Microsoft.UI.Xaml.Data;
using Tracer.Contracts;

namespace ZoNo.Converters
{
  public class ThousandsSeparatorConverter : IValueConverter
  {
    private readonly ITraceFactory _traceFactory = App.GetService<ITraceFactory>();

    public object Convert(object value, Type targetType, object parameter, string language)
    {
      using var trace = _traceFactory.CreateNew();

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
        var result = System.Convert.ToDouble(value).ToString("N");
        trace.Debug(Format([value, result]));
        return result;
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
