using Splitwise.Models;
using System.Threading.Tasks;

namespace Splitwise.Contracts
{
  /// <summary>
  /// Used for wrapping the Splitwise REST API. <br/>
  /// </summary>
  public interface ISplitwiseService
  {
    /// <summary>
    /// Bearer token.
    /// </summary>
    Token Token { get; set; }

    /// <summary>
    /// Get information about the current user.
    /// </summary>
    /// <returns></returns>
    Task<User> GetCurrentUserAsync();

    /// <summary>
    /// List the current user's groups. <br/>
    /// <b>Note:</b> Expenses that are not associated with a group are listed in a group with ID 0.
    /// </summary>
    /// <returns></returns>
    Task<Group[]> GetGroupsAsync();

    /// <summary>
    /// Returns a list of all categories Splitwise allows for expenses.
    /// There are parent categories that represent groups of categories with subcategories for more specific categorization.
    /// When creating expenses, you must use a subcategory, not a parent category.
    /// If you intend for an expense to be represented by the parent category and nothing more specific, please use the "Other" subcategory.
    /// </summary>
    /// <returns></returns>
    Task<Category[]> GetCategoriesAsync();

    /// <summary>
    /// Creates an expense.
    /// </summary>
    /// <param name="expense"></param>
    /// <returns></returns>
    Task<Expense[]> CreateExpense(Expense expense);
  }
}
