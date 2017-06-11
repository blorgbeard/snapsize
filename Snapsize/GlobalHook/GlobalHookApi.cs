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
        [DllImport("globalhook.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool InitializeCallWndProcHook(int threadID, IntPtr hWndDestination);

        [DllImport("globalhook.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void UninitializeCallWndProcHook();
    }
}
