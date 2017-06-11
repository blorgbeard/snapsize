using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Snapsize
{
    public partial class ConfigForm : Form
    {

        [DllImport("user32.dll")]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder title, int size);
        [DllImport("user32.dll")]
        private static extern uint RealGetWindowClass(IntPtr hWnd, StringBuilder pszType, uint cchType);

        private string GetWindowName(IntPtr Hwnd)
        {
            // This function gets the name of a window from its handle
            StringBuilder Title = new StringBuilder(256);
            GetWindowText(Hwnd, Title, 256);

            return Title.ToString().Trim();
        }

        private string GetWindowClass(IntPtr Hwnd)
        {
            // This function gets the name of a window class from a window handle
            StringBuilder Title = new StringBuilder(256);
            RealGetWindowClass(Hwnd, Title, 256);

            return Title.ToString().Trim();
        }

        private readonly GlobalHooks _hooks;
        private readonly OverlayForm _overlay;

        public ConfigForm()
        {
            InitializeComponent();
            _hooks = new GlobalHooks(this.Handle);
            _hooks.CallWndProc.CallWndProc += Hooked_WndProc;
            _overlay = new OverlayForm();
        }

        private void Hooked_WndProc(IntPtr windowHandle, IntPtr message, IntPtr wParam, IntPtr lParam)
        {
            if (message == WinApi.WM_MOVE)
            {
                int x = (short)((UInt64)lParam & 0xFFFF);
                int y = (short)(((UInt64)lParam & 0xFFFF0000) >> 16);
                Log("WM_MOVE          {0}: ({1}, {2})", GetWindowName(windowHandle), x, y);
            } 
            else if (message == WinApi.WM_MOVING)
            {
                Log("WM_MOVING        {0}", GetWindowName(windowHandle));
            }
            else if (message == WinApi.WM_ENTERSIZEMOVE)
            {
                Log("WM_ENTERSIZEMOVE {0}", GetWindowName(windowHandle));
                _overlay.Show();
            }
            else if (message == WinApi.WM_EXITSIZEMOVE)
            {
                Log("WM_EXITSIZEMOVE  {0}", GetWindowName(windowHandle));
                _overlay.Hide();
                WinApi.SetWindowPos(
                    windowHandle, IntPtr.Zero, 
                    0, 0, 500, 700, 
                    WinApi.SetWindowPosFlags.AsynchronousWindowPosition | 
                    WinApi.SetWindowPosFlags.IgnoreZOrder);
            }
        }

        protected override void WndProc(ref Message m)
        {
            // Check to see if we've received any Windows messages telling us about our hooks
            if (_hooks != null)
            {
                _hooks.ProcessWindowMessage(ref m);
            }

            base.WndProc(ref m);
        }

        private void Log(string text, params object[] args)
        {
            var result = (args.Length > 0) ? string.Format(text, args) : text;
            logTextBox.AppendText(result);
            logTextBox.AppendText(Environment.NewLine);
        }

        private void ConfigForm_Shown(object sender, EventArgs e)
        {
            //Log("Number is .... {0} !!!", GlobalHookApi.GetNumber(313, 434));
            //_hooks.CallWndProc.Start();
            //Log("started.");

        }

        private void clearLogButton_Click(object sender, EventArgs e)
        {
            logTextBox.Clear();
        }

        private void startButton_Click(object sender, EventArgs e)
        {
            _hooks.CallWndProc.Start();
        }

        private void stopButton_Click(object sender, EventArgs e)
        {
            _hooks.CallWndProc.Stop();
        }
        
        private void ConfigForm_FormClosed(object sender, FormClosedEventArgs e)
        {

        }
    }
}
