using CommunityToolkit.Common.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Data;
using Splitwise.Contracts;
using System.ComponentModel;
using Tracer.Contracts;
using ZoNo.Contracts.Services;
using ZoNo.Models;

namespace ZoNo.ViewModels
{
  public partial class QueryPageViewModel(
    ITraceFactory _traceFactory,
    ISplitwiseCacheService _splitwiseCacheService,
    ISplitwiseService _splitwiseService,
    IConverterService _converterService,
    ILocalSettingsService _localSettingsService) : ObservableObject
  {
    private const string SettingGroupName = "Query_GroupName";

    private bool _isLoaded = false;

    [ObservableProperty]
    private DateTimeOffset _dateTime = DateTimeOffset.Now;

    [ObservableProperty]
    private Group _group;

    [ObservableProperty]
    private CollectionViewSource _expenseGroups;

    [ObservableProperty]
    private bool _isLoading = false;

    public DateTimeOffset MinYear = new(2011, 1, 1, 0, 0, 0, TimeSpan.Zero);
    public DateTimeOffset MaxYear = DateTimeOffset.Now;

    public IReadOnlyCollection<Group> Groups => _splitwiseCacheService.ZoNoGroups;

    public async Task LoadAsync()
    {
      using var trace = _traceFactory.CreateNew();
      trace.Debug(Format([_isLoaded]));
      if (_isLoaded) return;

      var groupName = await _localSettingsService.ReadSettingAsync<string>(SettingGroupName);
      Group = Groups.SingleOrDefault(group => group.Name == groupName) ?? Groups.First();

      PropertyChanged += QueryPageViewModel_PropertyChanged;
      await LoadExpensesAsync();

      _isLoaded = true;
    }

    private async void QueryPageViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      using var trace = _traceFactory.CreateNew();
      trace.Debug(Format([e.PropertyName]));
      if (e.PropertyName != nameof(DateTime) && e.PropertyName != nameof(Group)) return;

      if (e.PropertyName == nameof(Group))
      {
        await _localSettingsService.SaveSettingAsync(SettingGroupName, Group.Name);
      }
      await LoadExpensesAsync();
    }

    private async Task LoadExpensesAsync()
    {
      using var trace = _traceFactory.CreateNew();
      IsLoading = true;

      var splitwiseGroup = _splitwiseCacheService.SplitwiseGroups.Single(group => group.Id == Group.Id);
      var datedAfter = new DateTimeOffset(DateTime.Year, DateTime.Month, 1, 0, 0, 0, DateTime.Offset);
      var datedBefore = datedAfter.AddMonths(1).AddSeconds(-1);

      var splitwiseExpenses = await _splitwiseService.GetExpensesInGroupAsync(splitwiseGroup.Id, datedAfter, datedBefore, 1000, 0);
      splitwiseExpenses = splitwiseExpenses.Where(expense => expense.DeletedAt is null).ToArray();
      var expenses = splitwiseExpenses.Select(_converterService.SplitwiseExpenseToViewModel).ToArray();
      var expenseGroups = expenses.GroupBy(expense => expense.Category.ParentCategory);
      ExpenseGroups = new CollectionViewSource()
      {
        Source = new ReadOnlyObservableGroupedCollection<Category, ExpenseViewModel>(expenseGroups),
        IsSourceGrouped = true
      };
      IsLoading = false;
    }
  }
}