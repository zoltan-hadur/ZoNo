using Splitwise.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ZoNo.Events
{
  public class UserLoggedInEventArgs
  {
    public User User { get; set; }
    public Token Token { get; set; }
  }
}
