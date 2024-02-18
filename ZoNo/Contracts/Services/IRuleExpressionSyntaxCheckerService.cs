namespace ZoNo.Contracts.Services
{
  public interface IRuleExpressionSyntaxCheckerService
  {
    Task<(bool IsValid, string ErrorMessage)> CheckSyntaxAsync<TInput>(string inputExpression);
    Task<(bool IsValid, string ErrorMessage)> CheckSyntaxAsync<TInput, TOutput>(string outputExpression);
  }
}
