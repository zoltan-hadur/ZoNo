namespace ZoNo.Models
{
  public class User
  {
    public string Picture { get; set; } = "invalid";
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }

    public static User FromSplitwiseModel(Splitwise.Models.User user)
    {
      return new User()
      {
        Picture = user.Picture.Medium,
        FirstName = user.FirstName,
        LastName = user.LastName,
        Email = user.Email
      };
    }
  }
}
