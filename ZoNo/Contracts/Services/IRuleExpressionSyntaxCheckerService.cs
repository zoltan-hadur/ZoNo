namespace ZoNo.Contracts.Services
{
  public interface IRuleExpressionSyntaxCheckerService
  {
    Task<(bool IsValid, string ErrorMessage)> CheckSyntaxAsync<Input>(string inputExpression);
    Task<(bool IsValid, string ErrorMessage)> CheckSyntaxAsync<Input, Output>(string outputExpression);
  }
}
