using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Snapsize
{
    class GlobalHooks : IDisposable
    {                
        // Handle of the window intercepting messages
        private IntPtr _Handle;
        
        public GlobalHooks(IntPtr Handle)
        {
            _Handle = Handle;
            CallWndProc = new CallWndProcHook(_Handle);
        }

        public void ProcessWindowMessage(ref Message m)
        {
            CallWndProc.ProcessWindowMessage(ref m);
        }

        public void Dispose()
        {
            ((IDisposable)CallWndProc).Dispose();
        }

        public CallWndProcHook CallWndProc { get; private set; }
    }
}
