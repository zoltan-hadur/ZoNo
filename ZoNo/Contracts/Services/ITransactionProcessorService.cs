using ZoNo.Models;

namespace ZoNo.Contracts.Services
{
  public interface ITransactionProcessorService
  {
    event EventHandler<(Transaction Transaction, Expense Expense)> TransactionProcessed;

    Task InitializeAsync();
    Task ProcessAsync(Transaction transaction);
  }
}
