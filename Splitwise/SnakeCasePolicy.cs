using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Splitwise
{
  /// <summary>
  /// Naming policy to serialize / deserialize requests and responses to and from splitwise.
  /// </summary>
  internal class SnakeCasePolicy : JsonNamingPolicy
  {
    public override string ConvertName(string name)
    {
      // If current char is upper and next char is lower, then swap it with underscore and lower it
      var result = string.Concat(name.Select((c, i) => (i > 0 && i < name.Length - 1 && char.IsUpper(c) && char.IsLower(name[i - 1]) ? $"_{c}" : $"{c}").ToLower()));
      return result;
    }
  }
}
