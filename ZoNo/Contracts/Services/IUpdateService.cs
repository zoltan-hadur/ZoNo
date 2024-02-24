namespace ZoNo.Contracts.Services
{
  public interface IUpdateService
  {
    Task CheckForUpdateAsync();
    Task UpdateAsync();
  }
}
