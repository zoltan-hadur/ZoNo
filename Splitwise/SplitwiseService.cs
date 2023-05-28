using Splitwise.Contracts;
using Splitwise.Models;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Splitwise
{
  /// <summary>
  /// Class used for wrapping the Splitwise REST API. <br/>
  /// Implementation of <see cref="ISplitwiseAuthorizationService"/>.
  /// </summary>
  public class SplitwiseService : ISplitwiseService
  {
    private static readonly string _baseURL = "https://www.splitwise.com/api/v3.0";

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly Token _token;

    /// <summary>
    /// Creates an object of type <see cref="SplitwiseService"/> which can be used to call the Splitwise REST API.
    /// </summary>
    /// <param name="token">Access token from Splitwise.</param>
    public SplitwiseService(IHttpClientFactory httpClientFactory, Token token)
    {
      _httpClientFactory = httpClientFactory;
      _token = token;
    }

    private record class UserWrapper(User User);
    public async Task<User> GetCurrentUserAsync() => (await SendRequest<UserWrapper>(HttpMethod.Get, "get_current_user")).User;

    private record GroupsWrapper(Group[] Groups);
    public async Task<Group[]> GetGroupsAsync() => (await SendRequest<GroupsWrapper>(HttpMethod.Get, "get_groups")).Groups;

    private record CategoriesWrapper(Category[] Categories);
    public async Task<Category[]> GetCategoriesAsync() => (await SendRequest<CategoriesWrapper>(HttpMethod.Get, "get_categories")).Categories;

    private async Task<T> SendRequest<T>(HttpMethod method, string resource)
    {
      var request = new HttpRequestMessage(method, $"{_baseURL}/{resource}");
      var client = _httpClientFactory.CreateClient();
      client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(_token.TokenType.ToString(), _token.AccessToken);
      var response = await client.SendAsync(request);
      if (response.IsSuccessStatusCode)
      {
        using var contentStream = await response.Content.ReadAsStreamAsync();
        var result = await JsonSerializer.DeserializeAsync<T>(contentStream, new JsonSerializerOptions()
        {
          PropertyNamingPolicy = new SnakeCasePolicy(),
          Converters = { new JsonStringEnumConverter() }
        });
        return result;
      }
      else
      {
        throw new Exception($"Could not execute '{request}', status code: {response.StatusCode}");
      }
    }
  }
}
