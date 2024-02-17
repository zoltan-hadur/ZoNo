namespace ZoNo.Models
{
  public class GlobalType<TInput, TOutput>
  {
    public required string RuleId { get; set; }
    public required TInput Input { get; set; }
    public required TOutput Output { get; set; }
    public required ExecutionType ExecutionType { get; set; }
    public bool RemoveThisElementFromList { get; set; } = false;
  }
}
