using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoNo.Models
{
  public class Rule
  {
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string Description { get; init; }
    public required string InputExpression { get; init; }
    public required IReadOnlyList<string> OutputExpressions { get; init; }

    public Rule Clone()
    {
      return new Rule()
      {
        Id = Id,
        Description = Description,
        InputExpression = InputExpression,
        OutputExpressions = OutputExpressions.ToArray()
      };
    }
  }
}
