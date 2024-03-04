using Splitwise.Contracts;
using Tracer;
using Tracer.Contracts;
using ZoNo.Contracts.Services;
using ZoNo.Helpers;

namespace ZoNo.Services
{
  public class SplitwiseCacheService(
    ITraceFactory _traceFactory,
    ISplitwiseService _splitwiseService) : ISplitwiseCacheService
  {
    public IReadOnlyCollection<Splitwise.Models.Group> SplitwiseGroups { get; private set; }
    public IReadOnlyCollection<Splitwise.Models.Category> SplitwiseCategories { get; private set; }
    public IReadOnlyCollection<ZoNo.Models.Group> ZoNoGroups { get; private set; }
    public IReadOnlyCollection<ZoNo.Models.Category> ZoNoCategories { get; private set; }
    public ZoNo.Models.User ZoNoUser { get; private set; }

    public async Task InitializeAsync()
    {
      using var trace = _traceFactory.CreateNew();

      var taskGroups = TraceFactory.HandleAsAsyncVoid(_splitwiseService.GetGroupsAsync);
      var taskCategories = TraceFactory.HandleAsAsyncVoid(_splitwiseService.GetCategoriesAsync);
      var taskUser = TraceFactory.HandleAsAsyncVoid(_splitwiseService.GetCurrentUserAsync);
      await Task.WhenAll([taskGroups, taskCategories, taskUser]);

      SplitwiseGroups = taskGroups.Result;
      SplitwiseCategories = taskCategories.Result.OrderBy(category => category.Name).ToArray();

      ZoNoGroups = SplitwiseGroups.Select(SplitwiseGroupToModel).ToArray();
      ZoNoCategories = SplitwiseCategories.Select(SplitwiseCategoryToModel).ToArray();
      ZoNoUser = SplitwiseUserToModel(taskUser.Result);
    }

    private ZoNo.Models.Category SplitwiseCategoryToModel(Splitwise.Models.Category category)
    {
      return new ZoNo.Models.Category()
      {
        Name = category.Name,
        Picture = category.IconTypes.Square.Large,
        SubCategories = category.Subcategories.OrEmpty().Select(SplitwiseCategoryToModel).ToArray()
      };
    }

    private ZoNo.Models.Group SplitwiseGroupToModel(Splitwise.Models.Group group)
    {
      return new ZoNo.Models.Group()
      {
        Picture = group.Avatar.Medium,
        Name = group.Name,
        Members = group.Members.Select(SplitwiseUserToModel).ToArray()
      };
    }

    private ZoNo.Models.User SplitwiseUserToModel(Splitwise.Models.User user)
    {
      return new ZoNo.Models.User()
      {
        Picture = user.Picture.Medium,
        FirstName = user.FirstName,
        LastName = user.LastName,
        Email = user.Email,
        DefaultCurrency = Enum.Parse<ZoNo.Models.Currency>(user.DefaultCurrency.ToString())
      };
    }
  }
}
