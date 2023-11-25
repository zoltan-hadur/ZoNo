using ZoNo.Models;

namespace ZoNo.Contracts.Services
{
  public interface IExcelDocumentLoader
  {
    Task<IList<Transaction>> LoadAsync(string path);
  }
}
