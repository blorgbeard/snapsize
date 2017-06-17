using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Snapsize
{
    static class WinApi
    {
        public static readonly IntPtr WM_MOVE = (IntPtr)0x0003;
        public static readonly IntPtr WM_SIZE = (IntPtr)0x0005;
        public static readonly IntPtr WM_ENTERSIZEMOVE = (IntPtr)0x0231;
        public static readonly IntPtr WM_EXITSIZEMOVE = (IntPtr)0x0232;

        [DllImport("user32.dll")]
        public static extern int RegisterWindowMessage(string name);
        [DllImport("user32.dll")]
        public static extern IntPtr GetProp(IntPtr hWnd, string name);
        [DllImport("user32.dll")]
        public static extern IntPtr GetDesktopWindow();

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left, Top, Right, Bottom;

            public RECT(int left, int top, int right, int bottom)
            {
                Left = left;
                Top = top;
                Right = right;
                Bottom = bottom;
            }

            public RECT(System.Drawing.Rectangle r) : this(r.Left, r.Top, r.Right, r.Bottom) { }

            public int X
            {
                get { return Left; }
                set { Right -= (Left - value); Left = value; }
            }

            public int Y
            {
                get { return Top; }
                set { Bottom -= (Top - value); Top = value; }
            }

            public int Height
            {
                get { return Bottom - Top; }
                set { Bottom = value + Top; }
            }

            public int Width
            {
                get { return Right - Left; }
                set { Right = value + Left; }
            }

            public System.Drawing.Point Location
            {
                get { return new System.Drawing.Point(Left, Top); }
                set { X = value.X; Y = value.Y; }
            }

            public System.Drawing.Size Size
            {
                get { return new System.Drawing.Size(Width, Height); }
                set { Width = value.Width; Height = value.Height; }
            }

            public static implicit operator System.Drawing.Rectangle(RECT r)
            {
                return new System.Drawing.Rectangle(r.Left, r.Top, r.Width, r.Height);
            }

            public static implicit operator RECT(System.Drawing.Rectangle r)
            {
                return new RECT(r);
            }

            public static bool operator ==(RECT r1, RECT r2)
            {
                return r1.Equals(r2);
            }

            public static bool operator !=(RECT r1, RECT r2)
            {
                return !r1.Equals(r2);
            }

            public bool Equals(RECT r)
            {
                return r.Left == Left && r.Top == Top && r.Right == Right && r.Bottom == Bottom;
            }

            public override bool Equals(object obj)
            {
                if (obj is RECT)
                    return Equals((RECT)obj);
                else if (obj is System.Drawing.Rectangle)
                    return Equals(new RECT((System.Drawing.Rectangle)obj));
                return false;
            }

            public override int GetHashCode()
            {
                return ((System.Drawing.Rectangle)this).GetHashCode();
            }

            public override string ToString()
            {
                return string.Format(System.Globalization.CultureInfo.CurrentCulture, "{{Left={0},Top={1},Right={2},Bottom={3}}}", Left, Top, Right, Bottom);
            }
        }

        /// <summary>
        ///     The MoveWindow function changes the position and dimensions of the specified window. For a top-level window, the
        ///     position and dimensions are relative to the upper-left corner of the screen. For a child window, they are relative
        ///     to the upper-left corner of the parent window's client area.
        ///     <para>
        ///     Go to https://msdn.microsoft.com/en-us/library/windows/desktop/ms633534%28v=vs.85%29.aspx for more
        ///     information
        ///     </para>
        /// </summary>
        /// <param name="hWnd">C++ ( hWnd [in]. Type: HWND )<br /> Handle to the window.</param>
        /// <param name="X">C++ ( X [in]. Type: int )<br />Specifies the new position of the left side of the window.</param>
        /// <param name="Y">C++ ( Y [in]. Type: int )<br /> Specifies the new position of the top of the window.</param>
        /// <param name="nWidth">C++ ( nWidth [in]. Type: int )<br />Specifies the new width of the window.</param>
        /// <param name="nHeight">C++ ( nHeight [in]. Type: int )<br />Specifies the new height of the window.</param>
        /// <param name="bRepaint">
        ///     C++ ( bRepaint [in]. Type: bool )<br />Specifies whether the window is to be repainted. If this
        ///     parameter is TRUE, the window receives a message. If the parameter is FALSE, no repainting of any kind occurs. This
        ///     applies to the client area, the nonclient area (including the title bar and scroll bars), and any part of the
        ///     parent window uncovered as a result of moving a child window.
        /// </param>
        /// <returns>
        ///     If the function succeeds, the return value is nonzero.<br /> If the function fails, the return value is zero.
        ///     <br />To get extended error information, call GetLastError.
        /// </returns>
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);
        
        public static Rectangle GetWindowBounds(IntPtr window)
        {
            WinApi.RECT output = new WinApi.RECT();
            if (WinApi.GetWindowRect(window, out output))
            {
                return output;
            }
            throw new Exception(string.Format("Couldn't get bounds of window {0}", window));
        }

        public static Rectangle GetExtendedFrameBounds(IntPtr window)
        {
            WinApi.RECT output = new WinApi.RECT();
            if (WinApi.DwmGetWindowAttribute(window, WinApi.DWMWINDOWATTRIBUTE.ExtendedFrameBounds, out output, Marshal.SizeOf(output)) == 0)
            {
                return output;
            }
            return GetWindowBounds(window);
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool GetWindowRect(IntPtr hwnd, out RECT lpRect);
        [DllImport("dwmapi.dll")]
        private static extern int DwmGetWindowAttribute(IntPtr hwnd, DWMWINDOWATTRIBUTE dwAttribute, out bool pvAttribute, int cbAttribute);
        [DllImport("dwmapi.dll")]
        private static extern int DwmGetWindowAttribute(IntPtr hwnd, DWMWINDOWATTRIBUTE dwAttribute, out RECT pvAttribute, int cbAttribute);

        private enum DWMWINDOWATTRIBUTE : uint
        {
            NCRenderingEnabled = 1,
            NCRenderingPolicy,
            TransitionsForceDisabled,
            AllowNCPaint,
            CaptionButtonBounds,
            NonClientRtlLayout,
            ForceIconicRepresentation,
            Flip3DPolicy,
            ExtendedFrameBounds,
            HasIconicBitmap,
            DisallowPeek,
            ExcludedFromPeek,
            Cloak,
            Cloaked,
            FreezeRepresentation
        }
    }
}
