using CommunityToolkit.Mvvm.Messaging.Messages;
using ZoNo.Models;

namespace ZoNo.Messages
{
  public class UserLoggedOutMessage : ValueChangedMessage<User>
  {
    public UserLoggedOutMessage(User value) : base(value)
    {
    }
  }
}
