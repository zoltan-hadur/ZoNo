using CommunityToolkit.Mvvm.Messaging.Messages;
using ZoNo.Models;

namespace ZoNo.Messages
{
  public class UserLoggedInMessage : ValueChangedMessage<User>
  {
    public UserLoggedInMessage(User value) : base(value)
    {
    }
  }
}
