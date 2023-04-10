using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoNo.Helpers
{
  public static class DependencyObjectExtensions
  {
    public static T? FindChild<T>(this DependencyObject parent, Func<T, bool> predicate) where T : DependencyObject
    {
      ArgumentNullException.ThrowIfNull(parent);

      var queue = new Queue<DependencyObject>(new[] { parent });
      while (queue.Any())
      {
        var reference = queue.Dequeue();
        var count = VisualTreeHelper.GetChildrenCount(reference);
        for (var i = 0; i < count; i++)
        {
          var child = VisualTreeHelper.GetChild(reference, i);
          if (child is T candidate && predicate(candidate))
          {
            return candidate;
          }
          queue.Enqueue(child);
        }
      }
      return null;
    }
  }
}
