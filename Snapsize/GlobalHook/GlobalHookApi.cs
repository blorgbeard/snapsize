using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Snapsize
{
    static class GlobalHookApi
    {

#if WIN64
        private const string NativeDllName = "globalhook64.dll";
#else
        private const string NativeDllName = "globalhook.dll";
#endif

        [DllImport(NativeDllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool InitializeCallWndProcHook(int threadID, IntPtr hWndDestination);

        [DllImport(NativeDllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void UninitializeCallWndProcHook();
    }
}
