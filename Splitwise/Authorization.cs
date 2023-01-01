using RestSharp;
using RestSharp.Serializers.Json;
using Splitwise.Models;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Splitwise
{
  /// <summary>
  /// Class used for authorization and getting access token from Splitwise.
  /// </summary>
  public class Authorization
  {
    private static readonly string _baseURL = "https://secure.splitwise.com";
    private static readonly string _tokenResource = "oauth/token";
    private static readonly string _authorizeResource = "oauth/authorize";

    private readonly string _consumerKey;
    private readonly string _consumerSecret;
    private readonly string _state;

    /// <summary>
    /// URL that will be used to login.
    /// </summary>
    public string LoginURL => $"{_baseURL}/login";

    /// <summary>
    /// URL that will be used to authorize the logged in user.
    /// </summary>
    public string AuthorizationURL => $"{_baseURL}/{_authorizeResource}?response_type=code&client_id={_consumerKey}&state={_state}";

    /// <summary>
    /// Creates an object of type <see cref="Authorization"/> which can be used to authorize the user and get a <see cref="Token"/>.
    /// </summary>
    /// <param name="consumerKey">Consumer Key from Splitwise.</param>
    /// <param name="consumerSecret">Consumer Secret from Splitwise.</param>
    public Authorization(string consumerKey, string consumerSecret)
    {
      _consumerKey = consumerKey;
      _consumerSecret = consumerSecret;
      _state = Guid.NewGuid().ToString("N");
    }

    /// <summary>
    /// Extracts the authorization code from the URL where the user got redirected after successful authentication and consent.
    /// </summary>
    /// <param name="url">The URL where the user gots redirected after successful authentication and consent.</param>
    /// <param name="authorizationCode">The extracted authorization code.</param>
    /// <returns>True if the extraction was successful, otherwise false.</returns>
    public bool IsAccessGranted(string url, out string authorizationCode)
    {
      if (url.StartsWith(_baseURL))
      {
        var match = Regex.Match(url, "state=(?<state>[^&#]*)");
        if (match.Success && match.Groups["state"].Success && match.Groups["state"].Value == _state)
        {
          match = Regex.Match(url, "code=(?<code>[^&#]*)");
          if (match.Success && match.Groups["code"].Success)
          {
            authorizationCode = match.Groups["code"].Value;
            return true;
          }
        }
      }
      authorizationCode = null;
      return false;
    }

    public bool IsWrongCredentials(string url)
    {
      return url == $"{_baseURL}/authentication";
    }

    /// <summary>
    /// Requests token from Splitwise with the given authorization code.
    /// </summary>
    /// <param name="authorizationCode">Authorization code from Splitwise.</param>
    /// <returns>The bearer token.</returns>
    public async Task<Token> GetTokenAsync(string authorizationCode)
    {
      using (var client = new RestClient(_baseURL)
        .UseSystemTextJson(new JsonSerializerOptions()
        {
          PropertyNamingPolicy = new SnakeCasePolicy(),
          Converters = { new JsonStringEnumConverter() }
        }))
      {
        var request = new RestRequest(_tokenResource)
          .AddParameter("client_id", _consumerKey)
          .AddParameter("client_secret", _consumerSecret)
          .AddParameter("grant_type", "authorization_code")
          .AddParameter("code", authorizationCode)
          .AddParameter("redirect_uri", string.Empty);
        return await client.PostAsync<Token>(request);
      }
    }
  }
}
