using Splitwise.Contracts;
using Splitwise.Models;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Splitwise
{
  /// <summary>
  /// Class used for authorization and getting bearer token from Splitwise. <br/>
  /// Implementation of <see cref="ISplitwiseAuthorizationService"/>.
  /// </summary>
  public class SplitwiseAuthorizationService(
    IHttpClientFactory httpClientFactory,
    ISplitwiseConsumerCredentials splitwiseCredentials) : ISplitwiseAuthorizationService
  {
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    private readonly ISplitwiseConsumerCredentials _splitwiseCredentials = splitwiseCredentials;
    private readonly string _state = Guid.NewGuid().ToString("N");

    private const string _baseURL = "https://secure.splitwise.com";
    private const string _tokenResource = "oauth/token";
    private const string _authorizeResource = "oauth/authorize";

    public string LoginURL => $"{_baseURL}/login";
    public string AuthorizationURL => $"{_baseURL}/{_authorizeResource}?response_type=code&client_id={_splitwiseCredentials.ConsumerKey}&state={_state}";

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
      var request = new HttpRequestMessage(HttpMethod.Post, $"{_baseURL}/{_tokenResource}?client_id={_splitwiseCredentials.ConsumerKey}&client_secret={_splitwiseCredentials.ConsumerSecret}&grant_type=authorization_code&code={authorizationCode}&redirect_uri=");
      var response = await _httpClientFactory.CreateClient().SendAsync(request);
      if (response.IsSuccessStatusCode)
      {
        using var contentStream = await response.Content.ReadAsStreamAsync();
        var token = await JsonSerializer.DeserializeAsync<Token>(contentStream, new JsonSerializerOptions()
        {
          PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
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
