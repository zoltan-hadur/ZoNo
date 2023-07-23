namespace Splitwise.Models
{
  public class Group
  {
    public int Id { get; set; }
    public string Name { get; set; }
    public GroupType GroupType { get; set; }
    public bool SimplifyByDefault { get; set; }
    public User[] Members { get; set; }
    public Avatar Avatar { get; set; }
    public bool CustomAvatar { get; set; }
    public CoverPhoto CoverPhoto { get; set; }
  }

  public enum GroupType
  {
    Apartment,
    House,
    Trip,
    Other
  }

  public class Avatar
  {
    public string Original { get; set; }
    public string XXLarge { get; set; }
    public string XLarge { get; set; }
    public string Large { get; set; }
    public string Medium { get; set; }
    public string Small { get; set; }
  }

  public class CoverPhoto
  {
    public string XXLarge { get; set; }
    public string XLarge { get; set; }
  }
}
