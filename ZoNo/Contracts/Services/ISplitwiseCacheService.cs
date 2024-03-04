namespace ZoNo.Contracts.Services
{
  public interface ISplitwiseCacheService
  {
    IReadOnlyCollection<Splitwise.Models.Group> SplitwiseGroups { get; }
    IReadOnlyCollection<Splitwise.Models.Category> SplitwiseCategories { get; }
    IReadOnlyCollection<ZoNo.Models.Group> ZoNoGroups { get; }
    IReadOnlyCollection<ZoNo.Models.Category> ZoNoCategories { get; }
    ZoNo.Models.User ZoNoUser { get; }

    Task InitializeAsync();
  }
}
