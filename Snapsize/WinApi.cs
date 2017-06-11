using System;
using System.Runtime.InteropServices;

namespace Snapsize
{
    static class WinApi
    {
        public const int WM_MOVE = 0x0003;

        [DllImport("user32.dll")]
        public static extern int RegisterWindowMessage(string name);
        [DllImport("user32.dll")]
        public static extern IntPtr GetProp(IntPtr hWnd, string name);
        [DllImport("user32.dll")]
        public static extern IntPtr GetDesktopWindow();
    }
}
