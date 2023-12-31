namespace ZoNo.Models
{
  public class Group
  {
    public string Picture { get; set; } = "invalid";
    public string Name { get; set; }
    public User[] Members { get; set; } = [];

    public static Group FromSplitwiseModel(Splitwise.Models.Group group)
    {
      return new Group()
      {
        Picture = group.Avatar.Medium,
        Name = group.Name,
        Members = group.Members.Select(User.FromSplitwiseModel).ToArray()
      };
    }
  }
}
