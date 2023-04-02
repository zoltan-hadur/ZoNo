using ZoNo.Models;

namespace ZoNo.Contracts.Services
{
  public interface IRulesService
  {
    Task LoadRulesAsync();
    Task<IEnumerable<Rule>> GetRulesAsync();
    Task SaveRulesAsync(IEnumerable<Rule> rules);
  }

  public interface IImportRulesService : IRulesService
  {

  }

  public interface ISplitwiseRulesService : IRulesService
  {

  }
}
