using Splitwise.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Splitwise.Contracts
{
  /// <summary>
  /// Used for authorization and getting access token from Splitwise.
  /// </summary>
  public interface ISplitwiseAuthorizationService
  {
    /// <summary>
    /// URL that will be used to login.
    /// </summary>
    string LoginURL { get; }

    /// <summary>
    /// URL that will be used to authorize the logged in user.
    /// </summary>
    string AuthorizationURL { get; }

    /// <summary>
    /// Extracts the authorization code from the URL where the user got redirected after successful authentication and consent.
    /// </summary>
    /// <param name="url">The URL where the user gots redirected after successful authentication and consent.</param>
    /// <param name="authorizationCode">The extracted authorization code.</param>
    /// <returns>True if the extraction was successful, otherwise false.</returns>
    bool ExtractAuthorizationCode(string url, out string authorizationCode);

    /// <summary>
    /// Detects when the credentials are wrong.
    /// </summary>
    bool IsWrongCredentials(string url);

    /// <summary>
    /// Requests token from Splitwise with the given authorization code.
    /// </summary>
    /// <param name="authorizationCode">Authorization code from Splitwise.</param>
    /// <returns>The bearer token.</returns>
    Task<Token> GetTokenAsync(string authorizationCode);
  }
}
