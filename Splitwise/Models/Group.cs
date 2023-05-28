namespace Splitwise.Models
{
  public record Group(
    int Id,
    string Name,
    GroupType GroupType,
    bool SimplifyByDefault,
    User[] Members,
    Avatar Avatar,
    bool CustomAvatar,
    CoverPhoto CoverPhoto
  );
}
