using System.Runtime.CompilerServices;

namespace Tracer.Contracts
{
  public interface ITraceFactory
  {
    ITrace CreateNew([CallerFilePath] string filePath = null, [CallerLineNumber] int lineNumber = -1);
  }
}
