using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoNo.Models
{
  public enum RuleType
  {
    Import,
    Splitwise
  }

  public class Rule
  {
    public Guid Id { get; private set; } = Guid.NewGuid();
    public required RuleType Type { get; set; }
    public required string InputExpression { get; set; }
    public required List<string> OutputExpressions { get; set; }

    public Rule Clone()
    {
      return new Rule()
      {
        Id = Id,
        Type = Type,
        InputExpression = InputExpression,
        OutputExpressions = OutputExpressions.ToList()
      };
    }

    public static bool operator ==(Rule left, Rule right)
    {
      return left.Id == right.Id &&
             left.Type == right.Type &&
             left.InputExpression == right.InputExpression &&
             left.OutputExpressions.SequenceEqual(right.OutputExpressions);
    }

    public static bool operator !=(Rule left, Rule right)
    {
      return !(left == right);
    }
  }
}
