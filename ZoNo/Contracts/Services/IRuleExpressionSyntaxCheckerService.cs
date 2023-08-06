namespace ZoNo.Contracts.Services
{
  public interface IRuleExpressionSyntaxCheckerService
  {
    Task<(bool IsValid, string ErrorMessage)> TryCheckSyntaxAsync<Input>(string inputExpression);
    Task<(bool IsValid, string ErrorMessage)> TryCheckSyntaxAsync<Input, Output>(string outputExpression);
  }
}
