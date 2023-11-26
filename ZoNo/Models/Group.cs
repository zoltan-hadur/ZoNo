namespace ZoNo.Models
{
  public class Group
  {
    public string Picture { get; set; } = "invalid";
    public string Name { get; set; }
    public User[] Members { get; set; } = [];
  }
}
