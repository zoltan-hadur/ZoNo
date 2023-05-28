using Splitwise.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Splitwise.Contracts
{
  /// <summary>
  /// Used for wrapping the Splitwise REST API. <br/>
  /// </summary>
  public interface ISplitwiseService
  {
    /// <summary>
    /// Gets the currently logged in user.
    /// </summary>
    /// <returns></returns>
    Task<User> GetCurrentUserAsync();

    Task<Group[]> GetGroupsAsync();
  }
}
