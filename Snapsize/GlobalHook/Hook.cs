using System;

namespace Snapsize
{
    abstract class Hook : IDisposable
    {
        protected bool _IsActive = false;
        protected IntPtr _Handle;

        public Hook(IntPtr Handle)
        {
            _Handle = Handle;
        }

        public void Start()
        {
            if (!_IsActive)
            {
                _IsActive = true;
                OnStart();
            }
        }

        public void Stop()
        {
            if (_IsActive)
            {
                OnStop();
                _IsActive = false;
            }
        }

        public bool IsActive
        {
            get { return _IsActive; }
        }

        protected abstract void OnStart();
        protected abstract void OnStop();
        public abstract void ProcessWindowMessage(ref System.Windows.Forms.Message m);

        #region IDisposable Support
        private bool disposed = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // free UNMANAGED resources - all subclasses will wrap these and use Stop to free them
                Stop();

                disposed = true;
            }
        }

        ~Hook()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
