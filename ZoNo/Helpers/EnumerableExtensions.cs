using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoNo.Helpers
{
  public static class EnumerableExtensions
  {
    public static IEnumerable OrEmpty(this IEnumerable? enumerable)
    {
      return enumerable ?? Enumerable.Empty<object>();
    }
  }
}
