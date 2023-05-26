using Splitwise.Contracts;
using Splitwise.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Splitwise
{
  /// <summary>
  /// Class used for authorization and getting access token from Splitwise. <br/>
  /// Implementation of <see cref="ISplitwiseAuthorizationService"/>.
  /// </summary>
  public class SplitwiseAuthorizationService : ISplitwiseAuthorizationService
  {
    private static readonly string _baseURL = "https://secure.splitwise.com";
    private static readonly string _tokenResource = "oauth/token";
    private static readonly string _authorizeResource = "oauth/authorize";

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _consumerKey;
    private readonly string _consumerSecret;
    private readonly string _state;

    public string LoginURL => $"{_baseURL}/login";
    public string AuthorizationURL => $"{_baseURL}/{_authorizeResource}?response_type=code&client_id={_consumerKey}&state={_state}";

    /// <summary>
    /// Creates an object of type <see cref="SplitwiseAuthorizationService"/> which can be used to authorize the user and get a <see cref="Token"/>.
    /// </summary>
    /// <param name="consumerKey">Consumer Key from Splitwise.</param>
    /// <param name="consumerSecret">Consumer Secret from Splitwise.</param>
    public SplitwiseAuthorizationService(IHttpClientFactory httpClientFactory, string consumerKey, string consumerSecret)
    {
      _httpClientFactory = httpClientFactory;
      _consumerKey = consumerKey;
      _consumerSecret = consumerSecret;
      _state = Guid.NewGuid().ToString("N");
    }

    public bool ExtractAuthorizationCode(string url, out string authorizationCode)
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

    public async Task<Token> GetTokenAsync(string authorizationCode)
    {
      var request = new HttpRequestMessage(
        HttpMethod.Post,
        $"{_baseURL}/{_tokenResource}?client_id={_consumerKey}&client_secret={_consumerSecret}&grant_type=authorization_code&code={authorizationCode}&redirect_uri=");
      var response = await _httpClientFactory.CreateClient().SendAsync(request);
      if (response.IsSuccessStatusCode)
      {
        using var contentStream = await response.Content.ReadAsStreamAsync();
        var token = await JsonSerializer.DeserializeAsync<Token>(contentStream, new JsonSerializerOptions()
        {
          PropertyNamingPolicy = new SnakeCasePolicy(),
          Converters = { new JsonStringEnumConverter() }
        });
        return token;
      }
      else
      {
        throw new Exception($"Could not get token from Splitwise, status code: {response.StatusCode}");
      }
    }
  }
}
