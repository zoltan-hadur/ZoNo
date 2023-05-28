using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Splitwise.Models
{
  public record Category(
    int Id,
    string Name,
    string Icon,
    IconTypes IconTypes,
    Category[] Subcategories
  );
}
