using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZoNo.Models;

namespace ZoNo.Contracts.Services
{
  public interface IRuleEvaluatorService
  {
    Task EvaluateRulesAsync<Input, Output>(IEnumerable<Rule> rules, Input input, Output output);
  }
}
