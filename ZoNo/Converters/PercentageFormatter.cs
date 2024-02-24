using Tracer.Contracts;
using Windows.Globalization.NumberFormatting;

namespace ZoNo.Converters
{
  public class PercentageFormatter : INumberFormatter2, INumberParser
  {
    private readonly ITraceFactory _traceFactory = App.GetService<ITraceFactory>();

    public string FormatDouble(double value)
    {
      using var trace = _traceFactory.CreateNew();
      var result = $"{value:0.00} %";
      trace.Debug(Format([value, result]));
      return result;
    }

    public string FormatInt(long value)
    {
      using var trace = _traceFactory.CreateNew();
      var result = $"{value:0.00} %";
      trace.Debug(Format([value, result]));
      return result;
    }

    public string FormatUInt(ulong value)
    {
      using var trace = _traceFactory.CreateNew();
      var result = $"{value:0.00} %";
      trace.Debug(Format([value, result]));
      return result;
    }

    public double? ParseDouble(string text)
    {
      using var trace = _traceFactory.CreateNew();
      var success = double.TryParse(text.Trim('%'), out var result);
      trace.Debug(Format([success, text, result]));
      return success ? result : null;
    }

    public long? ParseInt(string text)
    {
      using var trace = _traceFactory.CreateNew();
      var success = long.TryParse(text.Trim('%'), out var result);
      trace.Debug(Format([success, text, result]));
      return success ? result : null;
    }

    public ulong? ParseUInt(string text)
    {
      using var trace = _traceFactory.CreateNew();
      var success = ulong.TryParse(text.Trim('%'), out var result);
      trace.Debug(Format([success, text, result]));
      return success ? result : null;
    }
  }
}
