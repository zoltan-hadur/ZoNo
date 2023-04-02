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
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string InputExpression { get; set; }
    public required List<string> OutputExpressions { get; set; }

    public Rule Clone()
    {
      return new Rule()
      {
        Id = Id,
        InputExpression = InputExpression,
        OutputExpressions = OutputExpressions.ToList()
      };
    }
  }
}
