using RestSharp;
using RestSharp.Authenticators.OAuth2;
using RestSharp.Serializers.Json;
using Splitwise.Models;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Splitwise
{
  /// <summary>
  /// Class used for wrapping the Splitwise REST API.
  /// </summary>
  public class Client : IDisposable
  {
    private static readonly string _baseURL = "https://www.splitwise.com/api/v3.0";
    private static readonly string _getCurrentUserResource = "get_current_user";

    private RestClient _client;

    /// <summary>
    /// Creates an object of type <see cref="Client"/> which can be used to call the Splitwise REST API.
    /// </summary>
    /// <param name="token">Access token from Splitwise.</param>
    public Client(Token token)
    {
      _client = new RestClient(_baseURL)
        .UseSystemTextJson(new JsonSerializerOptions()
        {
          PropertyNamingPolicy = new SnakeCasePolicy(),
          Converters = { new JsonStringEnumConverter() }
        })
        .UseAuthenticator(new OAuth2AuthorizationRequestHeaderAuthenticator(token.AccessToken, token.TokenType.ToString()));
    }

    public void Dispose()
    {
      _client.Dispose();
    }

    private record class UserWrapper(User User);

    /// <summary>
    /// Gets the currently logged in user.
    /// </summary>
    /// <returns></returns>
    public async Task<User> GetCurrentUserAsync()
    {
      var request = new RestRequest(_getCurrentUserResource);
      var result = await _client.GetAsync<UserWrapper>(request);
      return result.User;
    }
  }
}
