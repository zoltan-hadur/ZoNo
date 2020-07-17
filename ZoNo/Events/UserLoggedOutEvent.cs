using Prism.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace ZoNo.Events
{
  public class UserLoggedOutEvent : PubSubEvent<UserLoggedOutEventArgs>
  {
  }
}
