using System;
using System.Collections.Generic;

namespace Comms
{
    public class DisposableList : IDisposableList
    {
        private bool _disposed = false;
        private readonly List<IDisposable> _list = new List<IDisposable>();

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }
            _disposed = true;
            foreach (var disposable in _list)
            {
                try
                {
                    disposable.Dispose();
                }
                catch (Exception)
                {
                    // ignored
                }
            }
        }

        public void Add(IDisposable disposable)
        {
            _list.Add(disposable);
        }
    }

    public interface IDisposableList: IDisposable
    {
        void Add(IDisposable disposable);
    }
}