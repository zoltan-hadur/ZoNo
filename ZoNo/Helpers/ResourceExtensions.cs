using Microsoft.Windows.ApplicationModel.Resources;

namespace ZoNo.Helpers
{
  public static class ResourceExtensions
  {
    private static readonly ResourceLoader _resourceLoader = new ResourceLoader();

    public static string GetLocalized(this string resourceKey)
    {
      return _resourceLoader.GetString(resourceKey);
    }
  }
}