using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace Snapsize
{

    public partial class MainForm : Form
    {                
        private readonly GlobalHooks _hooks;
        private readonly GlobalKeyboardHook _keyHook;
        private readonly OverlayForm _overlay;
        private readonly SnapAreas _areas = new SnapAreas();

        protected override void SetVisibleCore(bool value)
        {
            // this is only a form so that we have a handle for GlobalHooks.
            // it will never be visible.
            base.SetVisibleCore(false);
        }

        public MainForm()
        {
            InitializeComponent();

            _hooks = new GlobalHooks(this.Handle);
            _hooks.CallWndProc.CallWndProc += Hooked_WndProc;
            _hooks.CallWndProc.Start();

            _keyHook = new GlobalKeyboardHook();
            _keyHook.KeyboardPressed += _keyHook_KeyboardPressed;
            
            _overlay = new OverlayForm();
            
        }
        
        private Rectangle AdjustDestinationAreaForWindow(IntPtr window, Rectangle area)
        {
            var realBounds = WinApi.GetExtendedFrameBounds(window);
            var invisibleBorderBounds = WinApi.GetWindowBounds(window);

            var invisibleBorderSizeLeft = realBounds.Left - invisibleBorderBounds.Left;
            var invisibleBorderSizeRight = invisibleBorderBounds.Right - realBounds.Right;
            var invisibleBorderSizeTop = realBounds.Top - invisibleBorderBounds.Top;
            var invisibleBorderSizeBottom = invisibleBorderBounds.Bottom - realBounds.Bottom;

            return new Rectangle(
                area.Left - invisibleBorderSizeLeft,
                area.Top - invisibleBorderSizeTop,
                area.Width + invisibleBorderSizeLeft + invisibleBorderSizeRight,
                area.Height + invisibleBorderSizeTop + invisibleBorderSizeBottom);

        }
        
        private IntPtr _window;
        private bool _inSizeMove = false;
        private bool _checkedMovingNotSizing = false;
        private bool _movingNotSizing = false;
        private bool _snapMode = false;
        private Size _initialSize;

        private void Hooked_WndProc(IntPtr window, IntPtr message, IntPtr wParam, IntPtr lParam)
        {
            if (_inSizeMove && window != _window)
                return;
            
            if (message == WinApi.WM_ENTERSIZEMOVE)
            {
                _inSizeMove = true;
                _window = window;
                _initialSize = WinApi.GetWindowBounds(window).Size;
                Log("initial size: {0}", _initialSize);
                Log("WM_ENTERSIZEMOVE {0}", GetWindowName(window));                
            }
            else if (_inSizeMove && message == WinApi.WM_MOVE)
            {
                //if (!_checkedMovingNotSizing)
                {
                    var newSize = WinApi.GetWindowBounds(window).Size;
                    Log("new size: {0}", newSize);
                    _movingNotSizing = newSize == _initialSize;
                    _checkedMovingNotSizing = true;
                }
                int x = (short)((UInt64)lParam & 0xFFFF);
                int y = (short)(((UInt64)lParam & 0xFFFF0000) >> 16);                    
                Log("WM_MOVE          {0}: ({1}, {2})", GetWindowName(window), x, y);                
            }
            else if(_inSizeMove && message == WinApi.WM_EXITSIZEMOVE)
            {
                _window = IntPtr.Zero;
                _inSizeMove = false;

                Log("WM_EXITSIZEMOVE  {0}", GetWindowName(window));
                
                if (_snapMode && _checkedMovingNotSizing && _movingNotSizing)
                {

                    var area = _areas.GetClosestSnapAreaPixels(Cursor.Position);
                    _window = window;

                    Log("Moving to {0}", area);

                    area = AdjustDestinationAreaForWindow(window, area);

                    WinApi.MoveWindow(
                        _window,
                        area.X,
                        area.Y,
                        area.Width,
                        area.Height,
                        true);
                }
            }

            ShowUpdateOverlay();
        }

        private void _keyHook_KeyboardPressed(object sender, GlobalKeyboardHookEventArgs e)
        {
            Keys vk = (Keys)e.KeyboardData.VirtualCode;
            Log("{0} {1}", e.KeyboardState, vk);

            if (vk == Keys.Shift || vk == Keys.ShiftKey || vk == Keys.LShiftKey || vk == Keys.RShiftKey)
            {
                _snapMode = e.KeyboardState == GlobalKeyboardHook.KeyboardState.KeyDown;
            }

            ShowUpdateOverlay();
        }

        private void ShowUpdateOverlay()
        {
            if (_snapMode && _inSizeMove && _movingNotSizing)
            {
                var area = _areas.GetClosestSnapAreaPixels(Cursor.Position);
                _overlay.SetDesktopBounds(area.X, area.Y, area.Width, area.Height);
                _overlay.Show();
            }
            else
            {
                _overlay.Hide();
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

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            _hooks.CallWndProc.Stop();
            _hooks.Dispose();
            _keyHook.Dispose();
        }

#if DEBUG

        [DllImport("user32.dll")]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder title, int size);

        private string GetWindowName(IntPtr Hwnd)
        {
            // This function gets the name of a window from its handle
            StringBuilder Title = new StringBuilder(256);
            GetWindowText(Hwnd, Title, 256);

            return Title.ToString().Trim();
        }

        private void Log(string text, params object[] args)
        {
            var result = (args.Length > 0) ? string.Format(text, args) : text;
            Debug.WriteLine(result);
        }        

#else
        private void Log(string text, params object[] args)
        {}
#endif


    }
}
