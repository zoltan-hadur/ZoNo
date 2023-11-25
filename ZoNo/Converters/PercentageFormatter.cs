using Windows.Globalization.NumberFormatting;

namespace ZoNo.Converters
{
  public class PercentageFormatter : INumberFormatter2, INumberParser
  {
    public string FormatDouble(double value) => $"{value.ToString("0.00")} %";
    public string FormatInt(long value) => $"{value.ToString("0.00")} %";
    public string FormatUInt(ulong value) => $"{value.ToString("0.00")} %";

    public double? ParseDouble(string text) => double.Parse(text.Trim('%'));
    public long? ParseInt(string text) => long.Parse(text.Trim('%'));
    public ulong? ParseUInt(string text) => ulong.Parse(text.Trim('%'));
  }
}
