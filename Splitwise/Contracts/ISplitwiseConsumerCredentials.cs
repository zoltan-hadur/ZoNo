namespace Splitwise.Contracts
{
  /// <summary>
  /// The Consumer Key and Secret are the most fundamental credentials required to access the Splitwise API.<br/>
  /// These credentials act as the username and password for the App, and are used by the Splitwise API to understand which App requests are coming from. 
  /// </summary>
  public interface ISplitwiseConsumerCredentials
  {
    string ConsumerKey { get; }
    string ConsumerSecret { get; }
  }
}
