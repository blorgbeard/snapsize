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
        
        /// <summary>
        /// Given a rectangle into which we want to put (the visible part of) a window, calculate the
        /// rectangle that we need to pass to MoveWindow to make that happen.
        /// We need to take into account the invisible border that Windows puts around windows to make
        /// resizing easier.
        /// </summary>
        /// <param name="window"></param>
        /// <param name="area"></param>
        /// <returns></returns>
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
            
            if (!_inSizeMove && message == WinApi.WM_ENTERSIZEMOVE)
            {
                Log("WM_ENTERSIZEMOVE {0}", GetWindowName(window)); //Log("inSizeMove = {0}", _inSizeMove);
                _initialSize = WinApi.GetWindowBounds(window).Size;
                //Log("initial size:    {0}", _initialSize);
                _inSizeMove = true;
                _window = window;
                ShowUpdateOverlay();
            }
            else if (_inSizeMove && message == WinApi.WM_MOVE)
            {
                //if (!_checkedMovingNotSizing)
                {
                //    var newSize = WinApi.GetWindowBounds(window).Size;
                //    Log("new size: {0}", newSize);
                //    _movingNotSizing = newSize == _initialSize;
                //    _checkedMovingNotSizing = true;
                }
                int x = (short)((UInt64)lParam & 0xFFFF);
                int y = (short)(((UInt64)lParam & 0xFFFF0000) >> 16);
                //Log("WM_MOVE          {0}: ({1}, {2})", GetWindowName(window), x, y);
                ShowUpdateOverlay();
            }
            else if(_inSizeMove && message == WinApi.WM_EXITSIZEMOVE)
            {
                Log("WM_EXITSIZEMOVE  {0}", GetWindowName(window));

                if (_snapMode)// && _checkedMovingNotSizing && _movingNotSizing)
                {

                    var area = _areas.GetClosestSnapAreaPixels(Cursor.Position);
                    var realArea = AdjustDestinationAreaForWindow(window, area);

                    Log("Moving to {0}; sending {1} to MoveWindow to account for invisible window borders", area, realArea);

                    WinApi.MoveWindow(
                        _window,
                        area.X,
                        area.Y,
                        area.Width,
                        area.Height,
                        true);
                }

                _window = IntPtr.Zero;
                _inSizeMove = false; Log("inSizeMove = {0}", _inSizeMove);

                ShowUpdateOverlay();
            }           
        }

        private void _keyHook_KeyboardPressed(object sender, GlobalKeyboardHookEventArgs e)
        {
            Keys vk = (Keys)e.KeyboardData.VirtualCode;
                        
            if (vk == Keys.Shift || vk == Keys.ShiftKey || vk == Keys.LShiftKey || vk == Keys.RShiftKey)
            {
                var shouldBeInSnapMode = e.KeyboardState == GlobalKeyboardHook.KeyboardState.KeyDown;
                if (shouldBeInSnapMode != _snapMode)
                {
                    _snapMode = shouldBeInSnapMode; 
                    Log("Set snapmode to {0}", _snapMode);
                    ShowUpdateOverlay();
                }
            }
        }

        private void ShowUpdateOverlay()
        {
            if (_snapMode && _inSizeMove)// && _movingNotSizing)
            {
                var area = _areas.GetClosestSnapAreaPixels(Cursor.Position);
                if (!_overlay.Visible)
                {
                    Log("Showing overlay");
                    _overlay.Show();
                }
                if (area != _overlay.DesktopBounds)
                {
                    Log("Moving overlay to {0}", area);
                    _overlay.SetDesktopBounds(area.X, area.Y, area.Width, area.Height);
                }
            }
            else if (_overlay.Visible)
            {
                Log("Hiding overlay");
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
