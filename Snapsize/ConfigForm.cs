using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace Snapsize
{
    public partial class ConfigForm : Form
    {

        [DllImport("user32.dll")]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder title, int size);
        
        private string GetWindowName(IntPtr Hwnd)
        {
            // This function gets the name of a window from its handle
            StringBuilder Title = new StringBuilder(256);
            GetWindowText(Hwnd, Title, 256);

            return Title.ToString().Trim();
        }
        
        private readonly GlobalHooks _hooks;
        //private readonly InterceptKeys _keyHook;
        private readonly GlobalKeyboardHook _keyHook;
        private readonly OverlayForm _overlay;

        // todo: allow customization of these
        private readonly List<Rectangle> _snapAreasPercentage = new List<Rectangle>()
        {
            // horizontal halves
            new Rectangle(0,0,50,100),
            new Rectangle(50,0,50,100),

            // horizontal thirds
            new Rectangle(0,0,33,100),
            new Rectangle(33,0,34,100),
            new Rectangle(67,0,33,100),

            // horizontal 1/3 + 2/3
            new Rectangle(0,0,33,100),
            new Rectangle(33,0,67,100),
            
            // horizontal 2/3 + 1/3
            new Rectangle(0,0,67,100),
            new Rectangle(67,0,33,100),
            
            // quandrants
            new Rectangle(0,0,50,50),
            new Rectangle(50,0,50,50),
            new Rectangle(0,50,50,50),
            new Rectangle(50,50,50,50),
        };

        private readonly List<Rectangle> _snapAreasPixels;


        public ConfigForm()
        {
            InitializeComponent();
            _hooks = new GlobalHooks(this.Handle);
            _hooks.CallWndProc.CallWndProc += Hooked_WndProc;
            _keyHook = new GlobalKeyboardHook();
            _keyHook.KeyboardPressed += _keyHook_KeyboardPressed;
            //_keyHook.KeyDown += _keyHook_KeyDown;
            //_keyHook.KeyUp += _keyHook_KeyUp;


            _overlay = new OverlayForm();
            _snapAreasPixels = (
                from percent in _snapAreasPercentage
                select new Rectangle(
                    Screen.PrimaryScreen.WorkingArea.Width * percent.X / 100,
                    Screen.PrimaryScreen.WorkingArea.Height * percent.Y / 100,
                    Screen.PrimaryScreen.WorkingArea.Width * percent.Width / 100,
                    Screen.PrimaryScreen.WorkingArea.Height * percent.Height / 100)
                ).ToList();
        }

        private int DistanceOfPointFromCentreOfRectangle(Point point, Rectangle rectangle)
        {
            var centre = new Point(rectangle.X + rectangle.Width / 2, rectangle.Y + rectangle.Height / 2);
            var xdist = point.X - centre.X;
            var ydist = point.Y - centre.Y;
            var dist = //Math.Sqrt  // comparing these to each other only, don't need to bother Sqrting them
                (
                    xdist*xdist + ydist*ydist
                );
            return dist;
        }

        private Rectangle GetClosestSnapAreaPixels(Point position)
        {
            var areasWithDistances =
                from area in _snapAreasPixels
                select new { area, dist = DistanceOfPointFromCentreOfRectangle(position, area) };

            return areasWithDistances.OrderBy(t => t.dist).Select(t => t.area).First();
        }

        private Rectangle GetWindowBounds(IntPtr window)
        {
            WinApi.RECT output = new WinApi.RECT();
            if (WinApi.GetWindowRect(window, out output))
            {
                return output;
            }
            throw new Exception(string.Format("Couldn't get bounds of window {0}", window));
        }

        private Rectangle GetExtendedFrameBounds(IntPtr window)
        {
            WinApi.RECT output = new WinApi.RECT();
            if (WinApi.DwmGetWindowAttribute(window, WinApi.DWMWINDOWATTRIBUTE.ExtendedFrameBounds, out output, Marshal.SizeOf(output)) == 0)
            {
                return output;
            }
            return GetWindowBounds(window);
        }

        private Rectangle AdjustDestinationAreaForWindow(IntPtr window, Rectangle area)
        {
            var realBounds = GetExtendedFrameBounds(window);
            var invisibleBorderBounds = GetWindowBounds(window);

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

        private Rectangle _area;

        private IntPtr _window;
        private bool _inSizeMove = false;
        private bool _moved = false;
        private bool _snapMode = false;

        private void _keyHook_KeyboardPressed(object sender, GlobalKeyboardHookEventArgs e)
        {
            Keys vk = (Keys)e.KeyboardData.VirtualCode;
            Log("{0} {1}", e.KeyboardState, vk);

            if  (vk == Keys.Shift || vk == Keys.ShiftKey || vk == Keys.LShiftKey || vk == Keys.RShiftKey)
            {
                _snapMode = e.KeyboardState == GlobalKeyboardHook.KeyboardState.KeyDown;
            }

            ShowUpdateOverlay();
        }

        private void Hooked_WndProc(IntPtr window, IntPtr message, IntPtr wParam, IntPtr lParam)
        {
            if (_inSizeMove && window != _window)
                return;

            if (message == WinApi.WM_ENTERSIZEMOVE)
            {
                _inSizeMove = true;
                _window = window;
                Log("WM_ENTERSIZEMOVE {0}", GetWindowName(window));                
            }
            else if (message == WinApi.WM_MOVE)
            {
                int x = (short)((UInt64)lParam & 0xFFFF);
                int y = (short)(((UInt64)lParam & 0xFFFF0000) >> 16);                    
                Log("WM_MOVE          {0}: ({1}, {2})", GetWindowName(window), x, y);

                ShowUpdateOverlay();
            }
            else if(message == WinApi.WM_EXITSIZEMOVE)
            {
                _window = IntPtr.Zero;
                _inSizeMove = false;

                Log("WM_EXITSIZEMOVE  {0}", GetWindowName(window));
                
                if (_snapMode)
                {

                    var area = GetClosestSnapAreaPixels(Cursor.Position);
                    _area = area;
                    _window = window;

                    Log("Moving to {0}", _area);

                    _area = AdjustDestinationAreaForWindow(window, _area);

                    WinApi.MoveWindow(
                        _window,
                        _area.X,
                        _area.Y,
                        _area.Width,
                        _area.Height,
                        true);
                }

                //WinApi.SetWindowPos(
                //    windowHandle, IntPtr.Zero, 
                //    area.X, area.Y, area.Width, area.Height,
                //    WinApi.SetWindowPosFlags.AsynchronousWindowPosition | 
                //    WinApi.SetWindowPosFlags.IgnoreZOrder);
            }

            ShowUpdateOverlay();
        }

        private void ShowUpdateOverlay()
        {
            if (_snapMode && _inSizeMove)
            {
                var area = GetClosestSnapAreaPixels(Cursor.Position);
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

        private void Log(string text, params object[] args)
        {
            var result = (args.Length > 0) ? string.Format(text, args) : text;
            logTextBox.AppendText(result);
            logTextBox.AppendText(Environment.NewLine);
        }

        private void ConfigForm_Shown(object sender, EventArgs e)
        {
           
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

        private void button1_Click(object sender, EventArgs e)
        {
  
        }

        private void lblCoords_Resize(object sender, EventArgs e)
        {

        }

        private void ConfigForm_Resize(object sender, EventArgs e)
        {
            lblCoords.Text = string.Format("{0}",
                this.DesktopBounds);
        }
    }
}
