using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Runtime.CompilerServices;

namespace CoinCollection
{
    /// <summary>
    /// Additional options for the Window class
    /// </summary>
    public partial class AdvanceWindow : Window
    {
        #region Fix for WindowChrome fall screen issue
        //https://www.youtube.com/watch?v=4JK9VtU8bYw
        private static IntPtr WindowProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case 0x0024:
                    WmGetMinMaxInfo(hwnd, lParam);
                    handled = true;
                    break;
            }
            return (IntPtr)0;
        }

        private static void WmGetMinMaxInfo(IntPtr hwnd, IntPtr lParam)
        {
            MINMAXINFO mmi = (MINMAXINFO)Marshal.PtrToStructure(lParam, typeof(MINMAXINFO))!;
            int MONITOR_DEFAULTTONEAREST = 0x00000002;
            IntPtr monitor = MonitorFromWindow(hwnd, MONITOR_DEFAULTTONEAREST);
            if (monitor != IntPtr.Zero)
            {
                MONITORINFO monitorInfo = new ();
                GetMonitorInfo(monitor, monitorInfo);
                RECT rcWorkArea = monitorInfo.rcWork;
                RECT rcMonitorArea = monitorInfo.rcMonitor;
                mmi.ptMaxPosition.x = Math.Abs(rcWorkArea.left - rcMonitorArea.left);
                mmi.ptMaxPosition.y = Math.Abs(rcWorkArea.top - rcMonitorArea.top);
                mmi.ptMaxSize.x = Math.Abs(rcWorkArea.right - rcWorkArea.left);
                mmi.ptMaxSize.y = Math.Abs(rcWorkArea.bottom - rcWorkArea.top);
            }
            Marshal.StructureToPtr(mmi, lParam, true);
        }

        /// <summary>Construct a point of coordinates (x,y).</summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct POINT(int x, int y)
        {
            /// <summary>x coordinate of point.</summary>
            public int x = x;
            /// <summary>y coordinate of point.</summary>
            public int y = y;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MINMAXINFO
        {
            public POINT ptReserved;
            public POINT ptMaxSize;
            public POINT ptMaxPosition;
            public POINT ptMinTrackSize;
            public POINT ptMaxTrackSize;
        };

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public class MONITORINFO
        {
            public int cbSize = Marshal.SizeOf(typeof(MONITORINFO));
            public RECT rcMonitor = new ();
            public RECT rcWork = new ();
            public int dwFlags = 0;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 0)]
        public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
            public static readonly RECT Empty = new ();
            public readonly int Width => Math.Abs(right - left);
            public readonly int Height => bottom - top;
            public RECT(int left, int top, int right, int bottom)
            {
                this.left = left;
                this.top = top;
                this.right = right;
                this.bottom = bottom;
            }
            public RECT(RECT rcSrc)
            {
                left = rcSrc.left;
                top = rcSrc.top;
                right = rcSrc.right;
                bottom = rcSrc.bottom;
            }
            public readonly bool IsEmpty => left >= right || top >= bottom;

            public override readonly string ToString()
            {
                if (this == Empty) { return "RECT {Empty}"; }
                return "RECT { left : " + left + " / top : " + top + " / right : " + right + " / bottom : " + bottom + " }";
            }
            public override readonly bool Equals(object? obj)
            {
                if (obj is not Rect) { return false; }
                return (this == (RECT)obj);
            }
            /// <summary>Return the HashCode for this struct (not garanteed to be unique)</summary>
            public override readonly int GetHashCode()
            {
                return left.GetHashCode() + top.GetHashCode() + right.GetHashCode() + bottom.GetHashCode();
            }

            /// <summary> Determine if 2 RECT are equal (deep compare)</summary>
            public static bool operator ==(RECT rect1, RECT rect2) { return (rect1.left == rect2.left && rect1.top == rect2.top && rect1.right == rect2.right && rect1.bottom == rect2.bottom); }
            /// <summary> Determine if 2 RECT are different(deep compare)</summary>
            public static bool operator !=(RECT rect1, RECT rect2) { return !(rect1 == rect2); }
        }

        //TODO: Figure out how to fix this issue
        [DllImport("user32")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetMonitorInfo(IntPtr hMonitor, MONITORINFO lpmi);

        [LibraryImport("User32")]
        internal static partial IntPtr MonitorFromWindow(IntPtr handle, int flags);
        #endregion

        //https://learn.microsoft.com/en-us/windows/apps/design/style/segoe-ui-symbol-font
        protected readonly string _maximizeIcon = "\uE922";
        protected readonly string _restoreIcon = "\uE923";

        private Button? _maximize;

        private readonly string _maximizeName;
        private readonly string _minimizeName;
        private readonly string _closeName;

        private readonly bool _reuseClose = false;

        /// <summary>
        /// Constructor for the AdvanceWindow class
        /// </summary>
        /// <param name="reuseClose">Reuse the window when its closed</param>
        /// <param name="maximizeName">Name of the maximum button</param>
        /// <param name="minimizeName">Name of the minimise button</param>
        /// <param name="closeName">Name of the close button</param>
        public AdvanceWindow(bool reuseClose = false, string maximizeName = "Maximize", string minimizeName = "minimize", string closeName = "Close") : base()
        {
            //TODO: Fix issue with minimize name not capitalised

            Loaded += OnLoad;
            SourceInitialized += SourceInit;
            StateChanged += ChangeSize;
            
            _reuseClose = reuseClose;

            _maximizeName = maximizeName;
            _minimizeName = minimizeName;
            _closeName = closeName;
        }

        protected override void OnClosed(EventArgs e)
        {
            Loaded -= OnLoad;
            SourceInitialized -= SourceInit;
            StateChanged -= ChangeSize;

            base.OnClosed(e);
        }

        /// <summary>
        /// Opens a window and returns without waiting for the newly opened window to close.
        /// </summary>
        /// <param name="wsl">Start up location of the window</param>
        /// <param name="topMost">Should the window always be on top</param>
        public virtual void Show(WindowStartupLocation wsl, bool topMost = false)
        {
            WindowStartupLocation = wsl;
            Topmost = topMost;
            Show();
        }

        /// <summary>
        /// Opens a window and returns only when the newly opened window is closed.
        /// </summary>
        /// <param name="wsl">Start up location of the window</param>
        /// <param name="topMost">Should the window always be on top</param>
        /// <returns>
        /// A <see cref="System.Nullable"/> value of type <see cref="System.Boolean"/> that specifies whether the activity
        /// was accepted (<see href="true"/>) or canceled (<see href="false"/>). The return value is the value of the
        /// <see cref="System.Windows.Window.DialogResult"/> property before a window closes.</returns>
        public virtual bool? ShowDialog(WindowStartupLocation wsl, bool topMost = false)
        {
            WindowStartupLocation = wsl;
            Topmost = topMost;
            return ShowDialog();
        }

        protected virtual void Minimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        protected virtual void Maximize_Click(object sender, RoutedEventArgs e)
        {
            if(WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Normal;
            }
            else
            {
                WindowState = WindowState.Maximized;
            }
        }

        protected virtual void Close_Click(object sender, RoutedEventArgs e)
        {
            if (_reuseClose)
            {
                Visibility = Visibility.Hidden;
            }
            else
            {
                Close();
            }
        }

        private void SourceInit(object? o, EventArgs e)
        {
            IntPtr handle = (new WindowInteropHelper(this)).Handle;
            HwndSource.FromHwnd(handle).AddHook(new HwndSourceHook(WindowProc));
        }

        private void ChangeSize(object? sender, EventArgs e)
        {
            if (_maximize != null)
            {
                if (WindowState == WindowState.Maximized)
                {
                    _maximize.Content = _restoreIcon;
                }
                else
                {
                    _maximize.Content = _maximizeIcon;
                }
            }
        }

        private void OnLoad(object sender, RoutedEventArgs e)
        {
            FindButton(_maximizeName, ref _maximize!);
        }

        /// <summary>
        /// Trys to find the button in the XAML
        /// </summary>
        /// <param name="buttonName">Name of the button</param>
        /// <param name="button">Reference to the button value to store the found button</param>
        private void FindButton(string buttonName, ref Button button, [CallerArgumentExpression(nameof(buttonName))] string parameterName = "Name not found!!!", [CallerLineNumber] int lineNumb = -1)
        {
            if (string.IsNullOrEmpty(buttonName))
            {
                Debug.WriteLine($"{parameterName} parameter, called at {lineNumb}, is empty!!!");
                return;
            }

            object temp = FindName(buttonName);

            if (temp == null)
            {
                Debug.WriteLine($"{buttonName} does not exist!!!");
            }
            else if (temp is not Button)
            {
                Debug.WriteLine($"{buttonName} is not a button!!!");
            }
            else
            {
                button = (Button)temp;
            }
        }
    }
}
