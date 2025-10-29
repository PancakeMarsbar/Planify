using System;
using System.Threading;

namespace Planify.Services
{
    public sealed class FileMutex : IDisposable
    {
        readonly Mutex _mutex;
        public FileMutex(string name)
        {
            _mutex = new Mutex(false, $"Planify_{name}");
        }
        public bool TryLock(int ms = 1000) => _mutex.WaitOne(ms);
        public void Unlock() => _mutex.ReleaseMutex();
        public void Dispose() { try { _mutex.ReleaseMutex(); } catch { } _mutex.Dispose(); }
    }
}
