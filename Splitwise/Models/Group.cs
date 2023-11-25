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
}
