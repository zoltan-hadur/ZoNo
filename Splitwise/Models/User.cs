using System;
using System.Collections.Generic;
using System.Text;

namespace Splitwise.Models
{
  public class User
  {
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public Picture Picture { get; set; }
    public bool CustomPicture { get; set; }
    public string Email { get; set; }
    public RegistrationStatus RegistrationStatus { get; set; }
    public object ForceRefreshAt { get; set; }
    public string Locale { get; set; }
    public CountryCode CountryCode { get; set; }
    public string DateFormat { get; set; }
    public CurrencyCode DefaultCurrency { get; set; }
    public int DefaultGroupId { get; set; }
    public DateTime NotificationsRead { get; set; }
    public int NotificationsCount { get; set; }
    public Dictionary<string, bool> Notifications { get; set; }
  }
}
