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
      var result = double.Parse(text.Trim('%'));
      trace.Debug(Format([text, result]));
      return result;
    }

    public long? ParseInt(string text)
    {
      using var trace = _traceFactory.CreateNew();
      var result = long.Parse(text.Trim('%'));
      trace.Debug(Format([text, result]));
      return result;
    }

    public ulong? ParseUInt(string text)
    {
      using var trace = _traceFactory.CreateNew();
      var result = ulong.Parse(text.Trim('%'));
      trace.Debug(Format([text, result]));
      return result;
    }
  }
}
