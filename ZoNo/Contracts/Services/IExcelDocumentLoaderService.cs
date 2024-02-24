using ZoNo.Models;

namespace ZoNo.Contracts.Services
{
  public interface IExcelDocumentLoaderService
  {
    Task<IList<Transaction>> LoadDocumentAsync(string path);
  }
}
