using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using RestSharp;
using Splitwise.Models;
using Splitwise.Models.Wrappers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Splitwise
{
  /// <summary>
  /// Class used for wrapping the Splitwise REST API.
  /// </summary>
  public class Client
  {
    private Token mToken;
    private RestClient mClient;
    private JsonSerializerSettings mJsonSerializerSettings = new JsonSerializerSettings
    {
      ContractResolver = new DefaultContractResolver
      {
        NamingStrategy = new SnakeCaseNamingStrategy { ProcessDictionaryKeys = true }
      },
      Formatting = Formatting.Indented
    };

    /// <summary>
    /// Creates an object of type <see cref="Client"/> which can be used to call the Splitwise REST API.
    /// </summary>
    /// <param name="token">Access token from Splitwise.</param>
    public Client(Token token)
    {
      mToken = token;
      mClient = new RestClient();
      mClient.AddDefaultHeaders(new Dictionary<string, string>()
      {
        { "authorization", $"{mToken.TokenType} {mToken.AccessToken}" },
        { "cache-control", "no-cache" }
      });
    }

    /// <summary>
    /// Gets the currently logged in user.
    /// </summary>
    /// <returns></returns>
    public User GetCurrentUser()
    {
      var wRequest = new RestRequest("https://www.splitwise.com/api/v3.0/get_current_user", Method.GET);
      return mClient.Execute<UserWrapper>(wRequest).Data.User;
      //var wResult = mClient.Execute(wRequest);
      //return JsonConvert.DeserializeObject<User>(JObject.Parse(wResult.Content)["user"].ToString(), mJsonSerializerSettings);
    }
  }
}
