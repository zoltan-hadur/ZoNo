using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZoNo.Models;

namespace ZoNo.Contracts.Services
{
  public interface IExcelLoader
  {
    Task<IEnumerable<Transaction>> LoadAsync(string path);
  }
}
