using System.Threading.Tasks;
using Consul.Net.Utilities;

namespace Consul.Net.Tests
{
  public class SelectiveParallel
  {
    static readonly AsyncReaderWriterLock m_Lock = new AsyncReaderWriterLock();

    static internal Task<AsyncReaderWriterLock.Releaser> Parallel()
    {
      return m_Lock.ReaderLockAsync();
    }

    static internal Task<AsyncReaderWriterLock.Releaser> NoParallel()
    {
      return m_Lock.WriterLockAsync();
    }
  }
}