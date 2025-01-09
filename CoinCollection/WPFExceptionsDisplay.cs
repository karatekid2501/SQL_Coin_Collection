using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace CoinCollection
{
    enum WPFExceptionsIcons
    {
        None = 0,
        IDI_Application = 32512,
        IDI_Hand,
        IDI_Question_Deprecated,
        IDI_Exclamation,
        IDI_Asterisk,
        IDI_Winlogo,
        IDI_Shield
    }

    /// <summary>
    /// Main class to easily display errors
    /// </summary>
    internal class WPFExceptionsDisplay : IDisposable
    {
        #region Get window icons
        private static BitmapSource GetShellIcon(WPFExceptionsIcons iconName = WPFExceptionsIcons.None)
        {
            if(iconName == WPFExceptionsIcons.None)
            {
                return null!;
            }

            IntPtr hIcon = LoadIcon(IntPtr.Zero, (int)iconName); // IDI_WARNING

            if (hIcon == IntPtr.Zero)
                throw new InvalidOperationException("Failed to load system icon.");

            BitmapSource icon = Imaging.CreateBitmapSourceFromHIcon(
                hIcon,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());

            DestroyIcon(hIcon); // Clean up
            return icon;
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr LoadIcon(IntPtr hInstance, int lpIconName);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DestroyIcon(IntPtr hIcon);
        #endregion

        public delegate void DisplayHandler();
        public delegate void HideHandler();

        public DisplayHandler? Display;
        public HideHandler? Hide;

        private readonly StackPanel _parent;

        private readonly bool _hasSeparator;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parent"></param>
        public WPFExceptionsDisplay(StackPanel parent, bool hasSeparator = false)
        {
            _parent = parent;

            Display += DisplayExceptions;
            Hide += ConcealExceptions;
            _hasSeparator = hasSeparator;
        }

        public void Dispose()
        {
            _parent.Children.Clear();
            Display -= DisplayExceptions;
            Hide -= ConcealExceptions;
        }

        /// <summary>
        /// Adds an exception to the error list
        /// </summary>
        /// <typeparam name="T">Type of exception</typeparam>
        /// <param name="exception"></param>
        public void Add<T>(WPFExceptionsDisplayItem<T> exception, WPFExceptionsIcons iconName = WPFExceptionsIcons.None) where T : Exception
        {
            if(_hasSeparator && _parent.Children.Count != 0)
            {
                _parent.Children.Add(new Separator());
            }

            _parent.Children.Add(BuildUI(exception, GetShellIcon(iconName)));
        }

        public void RemoveAt(int value)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(value);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(value, _parent.Children.Count);

            _parent.Children.RemoveAt(value);
        }

        public void Remove(string name)
        {
            if(name == _parent.Name)
            {
                return;
            }

            UIElement child = (UIElement)_parent.FindName(name);

            if(child != null)
            {
                _parent.Children.Remove(child);
            }
            else
            {
                Debug.WriteLine($"{name} does not exist in {_parent.Name}!!!");
            }
        }

        public void Remove<T>(WPFExceptionsDisplayItem<T> wPFExceptionsDisplayItem) where T : Exception
        {
            Remove(wPFExceptionsDisplayItem.Name);
        }

        public void Remove(UIElement child)
        {
            if(child != _parent)
            {
                _parent.Children.Remove(child);
            }
        }

        public void Clear()
        {
            _parent.Children.Clear();
        }

        public void ShowExceptions()
        {
            Display!.Invoke();
        }

        public void HideExceptions(bool clearExceptions = false)
        {
            if(clearExceptions)
            {
                Clear();
            }

            Hide!.Invoke();
        }

        protected virtual UIElement BuildUI<T>(WPFExceptionsDisplayItem<T> exception, BitmapSource iconInfo) where T : Exception
        {
            StackPanel exceptionInfo = new() 
            {
                Orientation = Orientation.Horizontal,
                Background = ColourToBrush(exception.BackgroundColour)
            };

            exceptionInfo.Children.Add(new Image()
            {
                Source = iconInfo,
                Width = iconInfo.Width,
                Height = iconInfo.Height,
            });

            exceptionInfo.Children.Add(new Label()
            {
                Content = exception.Name,
                Foreground = ColourToBrush(exception.TextColour)
            });

            //https://stackoverflow.com/questions/13584998/how-to-add-a-vertical-separator
            exceptionInfo.Children.Add(new Rectangle()
            {
                VerticalAlignment = VerticalAlignment.Stretch,
                Fill = ColourToBrush(Colors.DarkGray),
                Width = 1
            });

            exceptionInfo.Children.Add(new Label()
            {
                Content = exception.Description,
                Foreground = ColourToBrush(exception.TextColour)
            });

            return exceptionInfo;
        }

        protected static Brush ColourToBrush(Color color)
        {
            return new SolidColorBrush(color);
        }

        private void DisplayExceptions()
        {
            _parent.Visibility = Visibility.Visible;
        }

        private void ConcealExceptions()
        {
            _parent.Visibility = Visibility.Hidden;
        }
    }
}
