// <copyright file="AdvanceWindow.cs" company="Karatekid2501">
// Copyright (c) Karatekid2501. All rights reserved.
// </copyright>

using System;
using System.Collections;
using System.Diagnostics;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;

namespace CoinCollection
{
    /// <summary>
    /// Defualt buttons for the SystemMenu.
    /// </summary>
    [Flags]
    public enum AdvanceSystemMenuDefualtButtons
    {
#pragma warning disable SA1602 // Enumeration items should be documented
        None = 0,
        Disable = 1,
        SC_RESTORE = 2,
        SC_MOVE = 4,
        SC_SIZE = 8,
        SC_MINIMIZE = 16,
        SC_MAXIMIZE = 32,
        SC_CLOSE = 64,
        All = SC_RESTORE | SC_MOVE | SC_SIZE | SC_MINIMIZE | SC_MAXIMIZE | SC_CLOSE,
#pragma warning restore SA1602 // Enumeration items should be documented
    }

    /// <summary>
    /// System menu commands.
    /// </summary>
    [Flags]
    public enum AdvanceSystemMenuItemCommand
    {
#pragma warning disable S2346 // Flags enumerations zero-value members should be named "None"
#pragma warning disable SA1602 // Enumeration items should be documented
        MF_STRING = 1,
#pragma warning restore S2346 // Flags enumerations zero-value members should be named "None"
        MF_DISABLED = 2,
        MF_ENABLED = 4,
        MF_GRAYED = 8,
        MF_CHECKED = 16,
        MF_UNCHECKED = 32,
#pragma warning restore SA1602 // Enumeration items should be documented
    }

    /// <summary>
    /// Base class for the items used in the AdvanceSystemMenu.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="AdvanceSystemMenuItemBase"/> class.
    /// </remarks>
    /// <param name="flag">Raw uint flag number command.</param>
    public abstract partial class AdvanceSystemMenuItemBase(uint flag)
    {
        private readonly uint _flag = flag;

        /// <summary>
        /// Initializes a new instance of the <see cref="AdvanceSystemMenuItemBase"/> class.
        /// </summary>
        /// <param name="command">Command to display the button type.</param>
        protected AdvanceSystemMenuItemBase(AdvanceSystemMenuItemCommand command)
            : this(0U)
        {
            Array temp = Enum.GetValues(typeof(AdvanceSystemMenuItemCommand));

            for (int i = 0; i < Enum.GetNames(typeof(AdvanceSystemMenuItemCommand)).Length; i++)
            {
                AdvanceSystemMenuItemCommand item = (AdvanceSystemMenuItemCommand)temp.GetValue(i)!;

                if ((command & item) != 0)
                {
                    _flag |= Convert(item);
                }
            }
        }

        /// <summary>
        /// Gets the ID for the item.
        /// </summary>
        public uint ID { get; private set; }

        /// <summary>
        /// Adds the item to the menu with paramters.
        /// </summary>
        /// <param name="hMenu">Menu to add the item to.</param>
        public abstract void AppendMenu(IntPtr hMenu);

        /// <summary>
        /// Adds the item to the menu and generates a new unique ID.
        /// </summary>
        /// <param name="hMenu">System menu to attach the item to.</param>
        /// <param name="uIDNewItem">Unique ID.</param>
        /// <param name="lpNewItem">Name of the item.</param>
        protected void AddMenuItem(IntPtr hMenu, uint uIDNewItem, string lpNewItem = "")
        {
            ID = uIDNewItem;

            AppendMenuW(hMenu, _flag, uIDNewItem, lpNewItem);
        }

        [LibraryImport("user32.dll", StringMarshalling = StringMarshalling.Utf16)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool AppendMenuW(IntPtr hMenu, uint uFlags, UIntPtr uIDNewItem, string lpNewItem);

        private static uint Convert(AdvanceSystemMenuItemCommand item)
        {
            return item switch
            {
                AdvanceSystemMenuItemCommand.MF_STRING => 0x00000000,
                AdvanceSystemMenuItemCommand.MF_DISABLED => 0x00000002,
                AdvanceSystemMenuItemCommand.MF_ENABLED => 0x00000000,
                AdvanceSystemMenuItemCommand.MF_GRAYED => 0x00000001,
                AdvanceSystemMenuItemCommand.MF_CHECKED => 0x00000008,
                AdvanceSystemMenuItemCommand.MF_UNCHECKED => 0x00000000,
                _ => throw new NotImplementedException(),
            };
        }
    }

    /// <summary>
    /// Spacer item.
    /// </summary>
    public class AdvanceSystemMenuSpacer : AdvanceSystemMenuItemBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AdvanceSystemMenuSpacer"/> class.
        /// </summary>
        public AdvanceSystemMenuSpacer()
            : base(0x00000800)
        {
        }

        /// <inheritdoc/>
        public override void AppendMenu(nint hMenu)
        {
            AddMenuItem(hMenu, 0);
        }
    }

    /// <summary>
    /// Button item.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="AdvanceSystemMenuButton"/> class.
    /// </remarks>
    /// <param name="command">Command type to show how the button will be displayed.</param>
    /// <param name="name">Name of the button.</param>
    /// <param name="task">Task to trigger once selected.</param>
    public class AdvanceSystemMenuButton(AdvanceSystemMenuItemCommand command, string name, Action task)
        : AdvanceSystemMenuItemBase(command), IDisposable
    {
        private static readonly uint _startBaseID = 0x8000;

        private static readonly HashSet<uint> _usedIDs = [];

        private readonly string _name = name;

        private uint _uniqueButtonID = 0x8000;

        /// <summary>
        /// Gets the task the button will trigger once selected.
        /// </summary>
        public Action Task { get; private set; } = task;

        /// <inheritdoc/>
        public override void AppendMenu(nint hMenu)
        {
            AddMenuItem(hMenu, GetAvalibleID(ref _uniqueButtonID), _name);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposing the class.
        /// </summary>
        /// <param name="disposing">Is the class being disposed.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _usedIDs.Remove(_uniqueButtonID);
            }
        }

        private static uint GetAvalibleID(ref uint uniqueButtonID)
        {
            uniqueButtonID = _startBaseID;

            while (_usedIDs.Contains(uniqueButtonID))
            {
                uniqueButtonID++;
            }

            _usedIDs.Add(uniqueButtonID);

            return uniqueButtonID;
        }
    }

    /// <summary>
    /// Group of buttons and groups.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="AdvanceSystemMenuGroupButton"/> class.
    /// </remarks>
    /// <param name="name">Nmae of the submenu.</param>
    /// <param name="subitems">List of the buttons and groups that are stored in the submenu.</param>
    public partial class AdvanceSystemMenuGroupButton(string name, params AdvanceSystemMenuItemBase[] subitems)
        : AdvanceSystemMenuItemBase(0x00000010), IDisposable
    {
        private readonly string _name = name;

        private readonly IntPtr _hwnd = CreatePopupMenu();

        private readonly AdvanceSystemMenuItemBase[] _subitems = subitems;

        private readonly List<IDisposable> _disposables = [];

        /// <inheritdoc/>
        public override void AppendMenu(nint hMenu)
        {
            foreach (AdvanceSystemMenuItemBase item in _subitems)
            {
                item.AppendMenu(_hwnd);

                if (item is IDisposable dis)
                {
                    _disposables.Add(dis);
                }
            }

            AddMenuItem(hMenu, (uint)_hwnd, _name);
        }

        /// <summary>
        /// Gets the values stored within each button item.
        /// </summary>
        /// <returns>Dictorary of all button actions and IDs.</returns>
        public Dictionary<uint, Action> GetValues()
        {
            Dictionary<uint, Action> temp = new();

            foreach (AdvanceSystemMenuItemBase subItem in _subitems)
            {
                if (subItem is AdvanceSystemMenuButton menuButton)
                {
                    temp.Add(menuButton.ID, menuButton.Task);
                }
                else if (subItem is AdvanceSystemMenuGroupButton groupButton)
                {
                    temp = temp.Concat(groupButton.GetValues()).GroupBy(kv => kv.Key).ToDictionary(dict => dict.Key, dict => dict.First().Value);
                }
            }

            return temp;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposing the class.
        /// </summary>
        /// <param name="disposing">Is the class being disposed.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (IDisposable dis in _disposables)
                {
                    dis.Dispose();
                }
            }
        }

        [LibraryImport("user32.dll")]
        private static partial IntPtr CreatePopupMenu();
    }

    /// <summary>
    /// System menu manager.
    /// </summary>
    public partial class AdvanceSystemMenu(AdvanceSystemMenuDefualtButtons removeButtons = AdvanceSystemMenuDefualtButtons.None,
        params AdvanceSystemMenuItemBase[] menuItems)
        : IDisposable
    {
#pragma warning disable SA1310 // Field names should not contain underscore
        private const int WM_SYSCOMMAND = 0x0112;
        private const uint MF_BYPOSITION = 0x400;
#pragma warning restore SA1310 // Field names should not contain underscore

        private readonly AdvanceSystemMenuDefualtButtons _buttonsToRemove = removeButtons;

        private readonly AdvanceSystemMenuItemBase[] _menuItems = menuItems;

        private readonly List<IDisposable> _disposables = [];

        private IntPtr hWnd;
        private IntPtr sysMenu;

        private Dictionary<uint, Action> _buttonActions = [];

        /// <summary>
        /// Alters the system menu attached to the window.
        /// </summary>
        /// <param name="window">Window to modify the system menu to.</param>
        public void OnSourceInitialized(AdvanceWindow window)
        {
            hWnd = new WindowInteropHelper(window).Handle;
            sysMenu = GetSystemMenu(hWnd, false);

            if (_buttonsToRemove != AdvanceSystemMenuDefualtButtons.None
                && _buttonsToRemove != AdvanceSystemMenuDefualtButtons.Disable
                && (_buttonsToRemove & AdvanceSystemMenuDefualtButtons.Disable) == 0)
            {
                if (_buttonsToRemove == AdvanceSystemMenuDefualtButtons.All || (_buttonsToRemove & AdvanceSystemMenuDefualtButtons.All) == 0)
                {
                    int count = GetMenuItemCount(sysMenu);

                    for (int i = count - 1; i >= 0; i--) // Remove all items
                    {
                        RemoveMenu(sysMenu, (uint)i, MF_BYPOSITION);
                    }
                }
                else
                {
                    Array temp = Enum.GetValues(typeof(AdvanceSystemMenuDefualtButtons));

                    for (int i = 2; i < Enum.GetNames(typeof(AdvanceSystemMenuDefualtButtons)).Length - 1; i++)
                    {
                        AdvanceSystemMenuDefualtButtons button = (AdvanceSystemMenuDefualtButtons)temp.GetValue(i)!;

                        if ((_buttonsToRemove & button) != 0)
                        {
                            RemoveMenu(sysMenu, Convert((AdvanceSystemMenuDefualtButtons)temp.GetValue(i)!), 0x0);

                            if (button == AdvanceSystemMenuDefualtButtons.SC_CLOSE)
                            {
                                RemoveMenu(sysMenu, (uint)GetMenuItemCount(sysMenu) - 1, MF_BYPOSITION);
                            }
                        }
                    }
                }
            }

            if (_buttonsToRemove != AdvanceSystemMenuDefualtButtons.Disable && (_buttonsToRemove & AdvanceSystemMenuDefualtButtons.Disable) == 0)
            {
                foreach (AdvanceSystemMenuItemBase menuItem in _menuItems)
                {
                    menuItem.AppendMenu(sysMenu);

                    if (menuItem is AdvanceSystemMenuButton menuButton)
                    {
                        _buttonActions.Add(menuButton.ID, menuButton.Task);

                        _disposables.Add(menuButton);
                    }
                    else if (menuItem is AdvanceSystemMenuGroupButton groupButton)
                    {
                        _buttonActions = _buttonActions.Concat(groupButton.GetValues()).GroupBy(kv => kv.Key).ToDictionary(dict => dict.Key, dict => dict.First().Value);

                        _disposables.Add(groupButton);
                    }
                }
            }

            HwndSource.FromHwnd(hWnd).AddHook(new HwndSourceHook(WndProc));
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposing the class.
        /// </summary>
        /// <param name="disposing">Is the class being disposed.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (IDisposable dis in _disposables)
                {
                    dis.Dispose();
                }
            }
        }

        private static uint Convert(AdvanceSystemMenuDefualtButtons button)
        {
            return button switch
            {
                AdvanceSystemMenuDefualtButtons.SC_RESTORE => 0xF120,
                AdvanceSystemMenuDefualtButtons.SC_MOVE => 0xF010,
                AdvanceSystemMenuDefualtButtons.SC_SIZE => 0xF000,
                AdvanceSystemMenuDefualtButtons.SC_MINIMIZE => 0xF020,
                AdvanceSystemMenuDefualtButtons.SC_MAXIMIZE => 0xF030,
                AdvanceSystemMenuDefualtButtons.SC_CLOSE => 0xF060,
                _ => throw new ArgumentException(nameof(AdvanceSystemMenuDefualtButtons), $"{button} type is not used."),
            };
        }

        [LibraryImport("user32.dll")]
        private static partial IntPtr GetSystemMenu(IntPtr hWnd, [MarshalAs(UnmanagedType.Bool)] bool bRevert);

        [LibraryImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool RemoveMenu(IntPtr hMenu, uint uPosition, uint uFlags);

        [LibraryImport("user32.dll")]
        private static partial int GetMenuItemCount(IntPtr hMenu);

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (_buttonsToRemove == AdvanceSystemMenuDefualtButtons.Disable)
            {
                if (msg == 0xa5 && wParam.ToInt32() == 0x02)
                {
                    handled = true;
                }
            }
            else
            {
                if (msg == WM_SYSCOMMAND)
                {
                    int command = wParam.ToInt32() & 0xFFF0;

                    foreach (var pair in _buttonActions)
                    {
                        if (pair.Key == command)
                        {
                            pair.Value();
                            handled = true;
                            break;
                        }
                    }
                }
            }

            return IntPtr.Zero;
        }
    }

    /// <summary>
    /// Additional options for the Window class.
    /// </summary>
    public partial class AdvanceWindow : Window
    {
        // https://www.youtube.com/watch?v=4JK9VtU8bYw
        [LibraryImport("User32", EntryPoint = "GetMonitorInfoW")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO lpmi);

        /// <summary>
        /// Gets the window from the monitor.
        /// </summary>
        /// <param name="handle">Window handler.</param>
        /// <param name="flags">Specifies which monitor to return if the window is not associated with a monitor.</param>
        /// <returns>The monitor contining the window.</returns>
        [LibraryImport("User32")]
        internal static partial IntPtr MonitorFromWindow(IntPtr handle, int flags);

        private IntPtr WindowProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case 0x0024:
                    WmGetMinMaxInfo(hwnd, lParam);
                    handled = true;
                    break;

                // https://stackoverflow.com/questions/16074229/disable-maximizing-wpf-window-on-double-click-on-the-caption
                case 0x00A3:
                    if (_enableDoubleClickTitlebar)
                    {
                        WindowState = WindowState != WindowState.Normal ? WindowState.Normal : WindowState.Maximized;
                    }

                    handled = true;
                    break;
            }

            return IntPtr.Zero;
        }

        private void WmGetMinMaxInfo(IntPtr hwnd, IntPtr lParam)
        {
            MINMAXINFO mmi = (MINMAXINFO)Marshal.PtrToStructure(lParam, typeof(MINMAXINFO))!;

#pragma warning disable SA1312 // Variable names should begin with lower-case letter
            int MONITOR_DEFAULTTONEAREST = 0x00000002;
#pragma warning restore SA1312 // Variable names should begin with lower-case letter

            IntPtr monitor = MonitorFromWindow(hwnd, MONITOR_DEFAULTTONEAREST);

            if (monitor != IntPtr.Zero)
            {
                MONITORINFO monitorInfo = new();
                GetMonitorInfo(monitor, ref monitorInfo);
                RECT rcWorkArea = monitorInfo.RcWork;
                RECT rcMonitorArea = monitorInfo.RcMonitor;
                mmi.PtMaxPosition.X = Math.Abs(rcWorkArea.Left - rcMonitorArea.Left);
                mmi.PtMaxPosition.Y = Math.Abs(rcWorkArea.Top - rcMonitorArea.Top);
                mmi.PtMaxSize.X = Math.Abs(rcWorkArea.Right - rcWorkArea.Left);
                mmi.PtMaxSize.Y = Math.Abs(rcWorkArea.Bottom - rcWorkArea.Top);
                mmi.PtMinTrackSize.X = (int)MinWidth;
                mmi.PtMinTrackSize.Y = (int)MinHeight;
            }

            Marshal.StructureToPtr(mmi, lParam, true);
        }

#pragma warning disable S101 // Types should be named in PascalCase
        /// <summary>
        /// Construct a point of coordinates (x,y).
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct POINT(int x, int y)
        {
            /// <summary>
            /// x coordinate of point.
            /// </summary>
            public int X = x;

            /// <summary>
            /// y coordinate of point.
            /// </summary>
            public int Y = y;
        }

        /// <summary>
        /// Information about minimum and maximum window size.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct MINMAXINFO
        {
            /// <summary>
            /// Point that is reserved and should not be modified.
            /// </summary>
            public POINT PtReserved;

            /// <summary>
            /// The maximum tracking size of the window (only used for maximized windows).
            /// </summary>
            public POINT PtMaxSize;

            /// <summary>
            /// The top-left corner of the maximized window.
            /// </summary>
            public POINT PtMaxPosition;

            /// <summary>
            /// The minimum width & height the window can be resized to.
            /// </summary>
            public POINT PtMinTrackSize;

            /// <summary>
            /// The maximum width & height the window can be resized to.
            /// </summary>
            public POINT PtMaxTrackSize;
        }

        /// <summary>
        /// Information about the monitor.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct MONITORINFO
        {
            /// <summary>
            /// The size of the structure.
            /// </summary>
            public int CbSize;

            /// <summary>
            /// The bounding rectangle of the entire monitor.
            /// </summary>
            public RECT RcMonitor;

            /// <summary>
            /// The work area.
            /// </summary>
            public RECT RcWork;

            /// <summary>
            /// 1 if it's the primary monitor, otherwise 0.
            /// </summary>
            public int DwFlags;

            /// <summary>
            /// Initializes a new instance of the <see cref="MONITORINFO"/> struct.
            /// </summary>
            public MONITORINFO()
            {
                CbSize = Marshal.SizeOf(typeof(MONITORINFO));
                RcMonitor = default;
                RcWork = default;
                DwFlags = 0;
            }
        }

        /// <summary>
        /// Rect of minitor.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 0)]
        public struct RECT
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="RECT"/> struct.
            /// </summary>
            /// <param name="left">Left side of the rect.</param>
            /// <param name="top">Top side of the rect.</param>
            /// <param name="right">Right side of the rect.</param>
            /// <param name="bottom">Bottom side of the rect.</param>
            public RECT(int left, int top, int right, int bottom)
            {
                Left = left;
                Top = top;
                Right = right;
                Bottom = bottom;
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="RECT"/> struct.
            /// </summary>
            /// <param name="rcSrc">Rect to copy from.</param>
            public RECT(RECT rcSrc)
            {
                Left = rcSrc.Left;
                Top = rcSrc.Top;
                Right = rcSrc.Right;
                Bottom = rcSrc.Bottom;
            }

            /// <summary>
            /// Creates an empty Rect.
            /// </summary>
#pragma warning disable SA1201 // Elements should appear in the correct order
            public static readonly RECT Empty = default;
#pragma warning restore SA1201 // Elements should appear in the correct order

            /// <summary>
            /// Left side of the rect.
            /// </summary>
            public int Left;

            /// <summary>
            /// Top side of the rect.
            /// </summary>
            public int Top;

            /// <summary>
            /// Right side of the rect.
            /// </summary>
            public int Right;

            /// <summary>
            /// Bottom side of the rect.
            /// </summary>
            public int Bottom;

            /// <summary>
            /// Gets a value indicating width of the rect.
            /// </summary>
            public readonly int Width => Math.Abs(Right - Left);

            /// <summary>
            /// Gets a value indicating the hight of the rect.
            /// </summary>
            public readonly int Height => Bottom - Top;

            /// <summary>
            /// Gets a value indicating whether the rect is empty.
            /// </summary>
            public readonly bool IsEmpty => Left >= Right || Top >= Bottom;

            public static bool operator ==(RECT rect1, RECT rect2) => rect1.Left == rect2.Left && rect1.Top == rect2.Top && rect1.Right == rect2.Right && rect1.Bottom == rect2.Bottom;

            public static bool operator !=(RECT rect1, RECT rect2) => !(rect1 == rect2);

            /// <inheritdoc/>
            public override readonly string ToString()
            {
                if (this == Empty)
                {
                    return "RECT {Empty}";
                }

                return "RECT { left : " + Left + " / top : " + Top + " / right : " + Right + " / bottom : " + Bottom + " }";
            }

            /// <inheritdoc/>
            public override readonly bool Equals(object? obj)
            {
                if (obj is not Rect)
                {
                    return false;
                }

                return this == (RECT)obj;
            }

            /// <summary>
            /// Return the HashCode for this struct (not garanteed to be unique).
            /// </summary>
            /// <returns>Unique hash code.</returns>
            public override readonly int GetHashCode()
            {
                return Left.GetHashCode() + Top.GetHashCode() + Right.GetHashCode() + Bottom.GetHashCode();
            }
        }
#pragma warning restore S101 // Types should be named in PascalCase
    }

    /// <summary>
    /// AdvanceWindow implementaton.
    /// </summary>
    public partial class AdvanceWindow : Window
    {
        protected readonly bool _enableDoubleClickTitlebar;

        // https://learn.microsoft.com/en-us/windows/apps/design/style/segoe-ui-symbol-font
        private readonly string _maximizeIcon = "\uE922";
        private readonly string _restoreIcon = "\uE923";

        private readonly string _maximizeName;
        private readonly string _minimizeName;
        private readonly string _closeName;

        private readonly bool _reuseClose = false;

        private readonly string _childClassName;

        private readonly AdvanceSystemMenu _systemMenu;

        private Button? _maximize = null;
        private Button? _minimize = null;
        private Button? _close = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="AdvanceWindow"/> class.
        /// </summary>
        /// <param name="reuseClose">Reuse the window when its closed.</param>
        /// <param name="maximizeName">Name of the maximum button (Use '!' to not to use this button).</param>
        /// <param name="minimizeName">Name of the minimise button (Use '!' to not to use this button.</param>
        /// <param name="closeName">Name of the close button (Use '!' to not to use this button).</param>
        /// <param name="enableDoubleClickTitlebar">Should the user be able to double click the title bar.</param>
        public AdvanceWindow(bool reuseClose = false, string maximizeName = "Maximize", string minimizeName = "Minimize", string closeName = "Exit", bool enableDoubleClickTitlebar = true)
            : this(new AdvanceSystemMenu(), reuseClose, maximizeName, minimizeName, closeName, enableDoubleClickTitlebar)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AdvanceWindow"/> class.
        /// </summary>
        /// <param name="advanceSystemMenu">Customisable system menu for the AdvanceWindow.</param>
        /// <param name="reuseClose">Reuse the window when its closed.</param>
        /// <param name="maximizeName">Name of the maximum button (Use '!' to not to use this button).</param>
        /// <param name="minimizeName">Name of the minimise button (Use '!' to not to use this button.</param>
        /// <param name="closeName">Name of the close button (Use '!' to not to use this button).</param>
        /// <param name="enableDoubleClickTitlebar">Should the user be able to double click the title bar.</param>
        public AdvanceWindow(
            AdvanceSystemMenu advanceSystemMenu,
            bool reuseClose = false,
            string maximizeName = "Maximize",
            string minimizeName = "Minimize",
            string closeName = "Exit",
            bool enableDoubleClickTitlebar = true)
            : base()
        {
            _enableDoubleClickTitlebar = enableDoubleClickTitlebar;

            _childClassName = "Unknown";

            Loaded += OnLoad;
            SourceInitialized += SourceInit;
            StateChanged += ChangeSize;
            Activated += WindowActivate;

            _reuseClose = reuseClose;

            _maximizeName = maximizeName;
            _minimizeName = minimizeName;
            _closeName = closeName;

            _childClassName = GetType().Name;

            _systemMenu = advanceSystemMenu;
        }

        /// <summary>
        /// Opens a window and returns without waiting for the newly opened window to close.
        /// </summary>
        /// <param name="wsl">Start up location of the window.</param>
        /// <param name="topMost">Should the window always be on top.</param>
        public virtual void Show(WindowStartupLocation wsl, bool topMost = false)
        {
            WindowStartupLocation = wsl;
            Topmost = topMost;
            Show();
        }

        /// <summary>
        /// Opens a window and returns only when the newly opened window is closed.
        /// </summary>
        /// <param name="wsl">Start up location of the window.</param>
        /// <param name="topMost">Should the window always be on top.</param>
        /// <returns>
        /// A <see cref="System.Nullable"/> value of type <see cref="bool"/> that specifies whether the activity
        /// was accepted (<see href="true"/>) or canceled (<see href="false"/>). The return value is the value of the
        /// <see cref="System.Windows.Window.DialogResult"/> property before a window closes.</returns>
        public virtual bool? ShowDialog(WindowStartupLocation wsl, bool topMost = false)
        {
            WindowStartupLocation = wsl;
            Topmost = topMost;
            return ShowDialog();
        }

        /// <summary>
        /// Event for when the window is active.
        /// </summary>
        /// <param name="sender">Object that sent the event.</param>
        /// <param name="e">Event arguments.</param>
        public virtual void WindowActivate(object? sender, EventArgs e)
        {
            if (!Topmost)
            {
                Focus();
            }
        }

        /// <inheritdoc/>
        protected override void OnClosed(EventArgs e)
        {
            Loaded -= OnLoad;
            SourceInitialized -= SourceInit;
            StateChanged -= ChangeSize;
            Activated -= WindowActivate;

            _systemMenu.Dispose();

            base.OnClosed(e);
        }

        /// <inheritdoc/>
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            _systemMenu.OnSourceInitialized(this);
        }

        /// <summary>
        /// Click event for minimizing the window.
        /// </summary>
        /// <param name="sender">Object that sent the event.</param>
        /// <param name="e">Event arguments.</param>
        protected virtual void Minimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        /// <summary>
        /// Click event for maximizing the window.
        /// </summary>
        /// <param name="sender">Object that sent the event.</param>
        /// <param name="e">Event arguments.</param>
        protected virtual void Maximize_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Normal;
            }
            else
            {
                WindowState = WindowState.Maximized;
            }
        }

        /// <summary>
        /// Click event for closing the window.
        /// </summary>
        /// <param name="sender">Object that sent the event.</param>
        /// <param name="e">Event arguments.</param>
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
            IntPtr handle = new WindowInteropHelper(this).Handle;
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
            FindButton(_minimizeName, ref _minimize!);
            FindButton(_closeName, ref _close!);
        }

        /// <summary>
        /// Trys to find the button in the XAML.
        /// </summary>
        /// <param name="buttonName">Name of the button.</param>
        /// <param name="button">Reference to the button value to store the found button.</param>
        private void FindButton(string buttonName, ref Button button, [CallerArgumentExpression(nameof(buttonName))] string parameterName = "Name not found!!!", [CallerLineNumber] int lineNumb = -1)
        {
            if (buttonName == "!")
            {
                return;
            }

            if (string.IsNullOrEmpty(buttonName))
            {
                Debug.WriteLine($"{parameterName} parameter, called at {lineNumb}, is empty!!!");
                return;
            }

            object temp = FindName(buttonName);

            if (temp == null)
            {
                Debug.WriteLine($"{buttonName} does not exist in {_childClassName}!!!");
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
