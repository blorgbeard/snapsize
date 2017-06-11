using System;
using System.Windows.Forms;

namespace Snapsize
{
    class CallWndProcHook : Hook
    {
        // Values retreived with RegisterWindowMessage
        private int _MsgID_CallWndProc;
        private int _MsgID_CallWndProc_Params;
        private int _MsgID_CallWndProc_HookReplaced;

        public delegate void WndProcEventHandler(IntPtr Handle, IntPtr Message, IntPtr wParam, IntPtr lParam);

        public event WndProcEventHandler CallWndProc;

        private IntPtr _CacheHandle;
        private IntPtr _CacheMessage;

        public CallWndProcHook(IntPtr Handle) : base(Handle)
        {
        }

        protected override void OnStart()
        {
            // Retreive the message IDs that we'll look for in WndProc
            _MsgID_CallWndProc_HookReplaced = WinApi.RegisterWindowMessage("MSG_SNAPSIZE_HOOK_REPLACED");
            _MsgID_CallWndProc = WinApi.RegisterWindowMessage("MSG_SNAPSIZE_CALLWNDPROC");
            _MsgID_CallWndProc_Params = WinApi.RegisterWindowMessage("MSG_SNAPSIZE_CALLWNDPROC_PARAMS");

            // Start the hook
            GlobalHookApi.InitializeCallWndProcHook(0, _Handle);
        }

        protected override void OnStop()
        {
            GlobalHookApi.UninitializeCallWndProcHook();
        }

        public override void ProcessWindowMessage(ref Message m)
        {
            if (m.Msg == _MsgID_CallWndProc)
            {
                _CacheHandle = m.WParam;
                _CacheMessage = m.LParam;
            }
            else if (m.Msg == _MsgID_CallWndProc_Params)
            {
                if (CallWndProc != null && _CacheHandle != IntPtr.Zero && _CacheMessage != IntPtr.Zero)
                {
                    CallWndProc(_CacheHandle, _CacheMessage, m.WParam, m.LParam);
                }
                _CacheHandle = IntPtr.Zero;
                _CacheMessage = IntPtr.Zero;
            }
        }
    }
}
