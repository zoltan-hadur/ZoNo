using CommunityToolkit.Common.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Data;
using Splitwise.Contracts;
using System.ComponentModel;
using Tracer.Contracts;
using ZoNo.Contracts.Services;
using ZoNo.Converters;
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
    private const string SettingQueryGroupBy = "Query_QueryGroupBy";

    private bool _isLoaded = false;
    private readonly Dictionary<QueryGroupBy, Func<ExpenseViewModel, object>> _groupBySelectors = new()
    {
      { QueryGroupBy.MainCategory, expense => expense.Category.ParentCategory },
      { QueryGroupBy.SubCategory, expense => expense.Category }
    };
    private ThousandsSeparatorConverter _thousandsSeparatorConverter = new();

    [ObservableProperty]
    private DateTimeOffset _dateTime = DateTimeOffset.Now;

    [ObservableProperty]
    private Group _group;

    [ObservableProperty]
    private QueryGroupBy _queryGroupBy;
    public QueryGroupBy[] QueryGroupBies = Enum.GetValues<QueryGroupBy>();

    [ObservableProperty]
    private bool _isLoading = false;

    public DateTimeOffset MinYear = new(2011, 1, 1, 0, 0, 0, TimeSpan.Zero);
    public DateTimeOffset MaxYear = DateTimeOffset.Now;

    public IReadOnlyCollection<Group> Groups => _splitwiseCacheService.ZoNoGroups;
    public CollectionViewSource ExpenseGroups = new() { IsSourceGrouped = true };

    public string TotalCost => $"{string.Join(", ",
      (ExpenseGroups.Source as ReadOnlyObservableGroupedCollection<object, ExpenseViewModel>)
        ?.SelectMany(group => group.Select(expense => expense))
        ?.GroupBy(expense => expense.Currency)
        ?.OrderBy(group => group.Key)
        ?.Select(group => $"{_thousandsSeparatorConverter.Convert(group.Sum(expense => expense.Cost), null, null, null)} {group.Key}")
        ?? []
    )}";

    public int NumberOfCurrencies => (ExpenseGroups.Source as ReadOnlyObservableGroupedCollection<object, ExpenseViewModel>)
      ?.SelectMany(group => group.Select(expense => expense))
      ?.DistinctBy(expense => expense.Currency)
      ?.Count() ?? 0;

    public async Task LoadAsync()
    {
      using var trace = _traceFactory.CreateNew();
      trace.Debug(Format([_isLoaded]));
      if (_isLoaded) return;

      var groupName = await _localSettingsService.ReadSettingAsync<string>(SettingGroupName);
      Group = Groups.SingleOrDefault(group => group.Name == groupName) ?? Groups.First();

      QueryGroupBy = await _localSettingsService.ReadSettingAsync<QueryGroupBy?>(SettingQueryGroupBy) ?? QueryGroupBy.SubCategory;

      PropertyChanged += QueryPageViewModel_PropertyChanged;
      await LoadExpenses();

      _isLoaded = true;
    }

    private async void QueryPageViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      using var trace = _traceFactory.CreateNew();
      trace.Debug(Format([e.PropertyName]));

      switch (e.PropertyName)
      {
        case nameof(Group):
          await _localSettingsService.SaveSettingAsync(SettingGroupName, Group.Name);
          break;
        case nameof(QueryGroupBy):
          await _localSettingsService.SaveSettingAsync(SettingQueryGroupBy, QueryGroupBy);
          break;
      }
    }

    [RelayCommand]
    private async Task LoadExpenses()
    {
      using var trace = _traceFactory.CreateNew();
      IsLoading = true;

      var splitwiseGroup = _splitwiseCacheService.SplitwiseGroups.Single(group => group.Id == Group.Id);
      var datedAfter = new DateTimeOffset(DateTime.Year, DateTime.Month, 1, 0, 0, 0, DateTime.Offset);
      var datedBefore = datedAfter.AddMonths(1).AddSeconds(-1);

      var splitwiseExpenses = await _splitwiseService.GetExpensesInGroupAsync(splitwiseGroup.Id, datedAfter, datedBefore, 1000, 0);
      splitwiseExpenses = splitwiseExpenses.Where(expense => expense.DeletedAt is null).ToArray();
      var expenses = splitwiseExpenses.Select(_converterService.SplitwiseExpenseToViewModel).ToArray();
      var expenseGroups = expenses.GroupBy(_groupBySelectors[QueryGroupBy]).OrderBy(group => group.Key);
      ExpenseGroups.Source = new ReadOnlyObservableGroupedCollection<object, ExpenseViewModel>(expenseGroups);
      OnPropertyChanged(nameof(TotalCost));
      IsLoading = false;
    }
  }
}