using Splitwise.Contracts;
using Splitwise.Models;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
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

    public async Task<User> GetCurrentUserAsync()
    {
      return await SendRequest(HttpMethod.Get, "get_current_user", (node, options) =>
      {
        return node["user"].Deserialize<User>(options);
      });
    }

    public async Task<Group[]> GetGroupsAsync()
    {
      return await SendRequest(HttpMethod.Get, "get_groups", (node, options) =>
      {
        return node["groups"].Deserialize<Group[]>(options);
      });
    }

    public async Task<Category[]> GetCategoriesAsync()
    {
      return await SendRequest(HttpMethod.Get, "get_categories", (node, options) =>
      {
        return node["categories"].Deserialize<Category[]>(options);
      });
    }

    public async Task<Expense[]> CreateExpense(Expense expense)
    {
      return await SendRequest(HttpMethod.Post, "create_expense", (node, options) =>
      {
        if (node["errors"]?.AsObject().Count != 0)
        {
          throw new Exception(node["errors"].ToJsonString());
        }
        else
        {
          return node["expenses"].Deserialize<Expense[]>(options);
        }
      },
      $$$""""
      {
        "cost": "{{{expense.Cost}}}",
        "description": "{{{expense.Description}}}",
        "date": "{{{expense.Date.ToString("o")}}}",
        "currency_code": "{{{expense.CurrencyCode}}}",
        "category_id": {{{expense.CategoryId}}},
        "group_id": {{{expense.GroupId}}},
      {{{string.Join($",{Environment.NewLine}", expense.Users.Select((user, index) =>
        $$"""
          "users__{{index}}__user_id": {{user.UserId}},
          "users__{{index}}__paid_share": "{{user.PaidShare}}",
          "users__{{index}}__owed_share": "{{user.OwedShare}}"
        """))}}}
      }
      """");
    }

    private async Task<T> SendRequest<T>(HttpMethod method, string resource, Func<JsonNode, JsonSerializerOptions, T> deserialize, string content = null)
    {
      var client = _httpClientFactory.CreateClient();
      client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(_token.TokenType.ToString(), _token.AccessToken);
      var request = new HttpRequestMessage(method, $"{_baseURL}/{resource}")
      {
        Content = content != null ? new StringContent(content, Encoding.UTF8, "application/json") : null
      };
      var response = await client.SendAsync(request);
      using var contentStream = await response.Content.ReadAsStreamAsync();
      using var streamReader = new StreamReader(contentStream);
      var responseString = streamReader.ReadToEnd();
      if (response.IsSuccessStatusCode)
      {
        try
        {
          var options = new JsonSerializerOptions()
          {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            Converters = { new JsonStringEnumConverter() }
          };
          var node = JsonNode.Parse(responseString);
          var result = deserialize(node, options);
          return result;
        }
        catch (Exception e)
        {
          throw new Exception($"Could not execute '{response.RequestMessage}', status code: {response.StatusCode}, response: {responseString}", e);
        }
      }
      else
      {
        throw new Exception($"Could not execute '{response.RequestMessage}', status code: {response.StatusCode}, response: {responseString}");
      }
    }
  }
}
