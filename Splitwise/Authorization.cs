using RestSharp;
using Splitwise.Models;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Splitwise
{
  /// <summary>
  /// Class used for authorization and getting access token from Splitwise.
  /// </summary>
  public class Authorization
  {
    /// <summary>
    /// URL used to request token from Splitwise.
    /// </summary>
    public static string TokenURL     => "https://secure.splitwise.com/oauth/token";

    /// <summary>
    /// URL used in authorization flow.
    /// </summary>
    public static string AuthorizeURL => "https://secure.splitwise.com/oauth/authorize";

    /// <summary>
    /// Consumer Key after registering as developer on Splitwise.
    /// </summary>
    public string ConsumerKey { get; }

    /// <summary>
    /// Consumer Secret after registering as developer on Splitwise.
    /// </summary>
    public string ConsumerSecret { get; }

    /// <summary>
    /// URL that will be used to logon the user and authorize.
    /// </summary>
    public string LoginURL => $"{AuthorizeURL}?response_type=code&client_id={ConsumerKey}";

    /// <summary>
    /// Creates an object of type <see cref="Authorization"/> which can be used to authorize the user and get a <see cref="Token"/>.
    /// </summary>
    /// <param name="consumerKey">Consumer Key from Splitwise.</param>
    /// <param name="consumerSecret">Consumer Secret from Splitwise.</param>
    public Authorization(string consumerKey, string consumerSecret)
    {
      ConsumerKey = consumerKey;
      ConsumerSecret = consumerSecret;
    }

    /// <summary>
    /// Extracts the authorization code from the URL where the user got redirected after successful authentication and consent.
    /// </summary>
    /// <param name="url">The URL where the user gots redirected after successful authentication and consent.</param>
    /// <returns></returns>
    public string ExtractAuthorizationCode(string url)
    {
      var wMatch = Regex.Match(url, @"https:\/\/secure\.splitwise\.com\/\?code=(?<code>.*)&state=.*");
      return wMatch.Success ? wMatch.Groups["code"].Value : null;
    }

    /// <summary>
    /// Requests token from Splitwise with the given authorization code.
    /// </summary>
    /// <param name="authorizationCode">Authorization code from Splitwise.</param>
    /// <returns></returns>
    public Token GetToken(string authorizationCode)
    {
      var wClient = new RestClient(TokenURL);
      var wRequest = new RestRequest(Method.POST);
      wRequest.AddHeaders(new Dictionary<string, string>()
      {
        { "cache-control", "no-cache"                          },
        { "content-type" , "application/x-www-form-urlencoded" }
      });
      wRequest.AddParameter("application/x-www-form-urlencoded", $"grant_type=authorization_code&code={authorizationCode}&redirect_uri=&client_id={ConsumerKey}&client_secret={ConsumerSecret}", ParameterType.RequestBody);
      var wResponse = wClient.Execute<Token>(wRequest);
      return wResponse.Data;
    }
  }
}
