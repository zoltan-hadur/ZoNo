namespace Splitwise.Models
{
  public record Group
  {
    public int Id { get; init; }
    public string Name { get; init; }
    public GroupType GroupType { get; init; }
    public bool SimplifyByDefault { get; init; }
    public User[] Members { get; init; }
    public Avatar Avatar { get; init; }
    public bool CustomAvatar { get; init; }
    public CoverPhoto CoverPhoto { get; init; }
  }
}
