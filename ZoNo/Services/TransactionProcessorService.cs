using Tracer;
using Tracer.Contracts;
using ZoNo.Contracts.Services;
using ZoNo.Helpers;
using ZoNo.Models;

namespace ZoNo.Services
{
  public class TransactionProcessorService(
    IRulesService _rulesService,
    IRuleEvaluatorServiceBuilder _ruleEvaluatorServiceBuilder,
    ITraceFactory _traceFactory) : ITransactionProcessorService
  {
    private readonly SemaphoreSlim _guard = new(initialCount: 1, maxCount: 1);
    private string _rulesJson = null;

    private IRuleEvaluatorService<Transaction, Transaction> _transactionRuleEvaluatorService = null;
    private IRuleEvaluatorService<Transaction, Expense> _expenseRuleEvaluatorService = null;

    public event EventHandler<(Transaction Transaction, Expense Expense)> TransactionProcessed;

    public async Task InitializeAsync()
    {
      using var trace = _traceFactory.CreateNew();
      using var guard = await LockGuard.CreateAsync(_guard, TimeSpan.FromSeconds(5));

      var transactionRules = _rulesService.GetRules(RuleType.Transaction);
      var expenseRules = _rulesService.GetRules(RuleType.Expense);
      var rulesJson = (await Json.StringifyAsync(transactionRules)) + (await Json.StringifyAsync(expenseRules));

      // Generate the code and compile it only in case the rules have changed
      if (rulesJson != _rulesJson)
      {
        trace.Info("Regenerating and compiling rules");
        _transactionRuleEvaluatorService?.Dispose();
        _expenseRuleEvaluatorService?.Dispose();
        _rulesJson = rulesJson;
        var taskTransaction = TraceFactory.HandleAsAsyncVoid(() => _ruleEvaluatorServiceBuilder.BuildAsync<Transaction, Transaction>(transactionRules));
        var taskExpense = TraceFactory.HandleAsAsyncVoid(() => _ruleEvaluatorServiceBuilder.BuildAsync<Transaction, Expense>(expenseRules));
        await Task.WhenAll([taskTransaction, taskExpense]);
        _transactionRuleEvaluatorService = taskTransaction.Result;
        _expenseRuleEvaluatorService = taskExpense.Result;
      }
    }

    public async Task ProcessAsync(Transaction transaction)
    {
      using var trace = _traceFactory.CreateNew();
      using var guard = await LockGuard.CreateAsync(_guard, TimeSpan.Zero);

      if (_transactionRuleEvaluatorService is null || _expenseRuleEvaluatorService is null)
      {
        throw new InvalidOperationException($"{nameof(InitializeAsync)} should be called first!");
      }

      trace.Debug(Format([transaction]));
      if (_transactionRuleEvaluatorService.EvaluateRules(input: transaction, output: transaction))
      {
        trace.Debug(Format([transaction]));
        var expense = new Expense()
        {
          Id = transaction.Id,
          With = [],
          Category = Categories.Uncategorized.General,
          Description = transaction.PartnerName,
          Currency = transaction.Currency,
          Cost = -transaction.Amount,
          Date = transaction.TransactionTime,
          Group = "Non-group expenses"
        };
        if (_expenseRuleEvaluatorService.EvaluateRules(input: transaction, output: expense))
        {
          trace.Debug(Format([expense]));
          TransactionProcessed?.Invoke(this, (transaction, expense));
        }
      }
    }
  }
}
