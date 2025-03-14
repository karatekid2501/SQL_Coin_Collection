// <copyright file="WPFExceptionsDisplay.cs" company="Karatekid2501">
// Copyright (c) Karatekid2501. All rights reserved.
// </copyright>

using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CoinCollection
{
    /// <summary>
    /// Exception icon to display when an exception is shown.
    /// </summary>
    public enum WPFExceptionsIcons
    {
#pragma warning disable SA1602 // Enumeration items should be documented
        None = 0,
        IDI_Application = 32512,
        IDI_Hand,
        IDI_Question_Deprecated,
        IDI_Exclamation,
        IDI_Asterisk,
        IDI_Winlogo,
        IDI_Shield,
#pragma warning restore SA1602 // Enumeration items should be documented
    }

    /// <summary>
    /// Main class to easily display errors.
    /// </summary>
    internal partial class WPFExceptionsDisplay : IDisposable
    {
        private static BitmapSource GetShellIcon(WPFExceptionsIcons iconName = WPFExceptionsIcons.None)
        {
            if (iconName == WPFExceptionsIcons.None)
            {
                return null!;
            }

            IntPtr hIcon = LoadIcon(IntPtr.Zero, (int)iconName); // IDI_WARNING

            if (hIcon == IntPtr.Zero)
            {
                throw new InvalidOperationException("Failed to load system icon.");
            }

            BitmapSource icon = Imaging.CreateBitmapSourceFromHIcon(
                hIcon,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());

            DestroyIcon(hIcon); // Clean up
            return icon;
        }

        [LibraryImport("user32.dll", SetLastError = true)]
        private static partial IntPtr LoadIcon(IntPtr hInstance, int lpIconName);

        [LibraryImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool DestroyIcon(IntPtr hIcon);
    }

    /// <summary>
    /// Implementation of WPFExceptionsDisplay.
    /// </summary>
    internal partial class WPFExceptionsDisplay : IDisposable
    {
        private readonly StackPanel _parent;

        private readonly bool _hasSeparator;

        /// <summary>
        /// Display the exceptions.
        /// </summary>
        private DisplayHandler? _display;

        /// <summary>
        /// Hide the exceptions.
        /// </summary>
        private HideHandler? _hide;

        /// <summary>
        /// Initializes a new instance of the <see cref="WPFExceptionsDisplay"/> class.
        /// </summary>
        /// <param name="parent">Parent to add exceptions to.</param>
        /// <param name="hasSeparator">Seperate each exceptions.</param>
        public WPFExceptionsDisplay(StackPanel parent, bool hasSeparator = false)
        {
            _parent = parent;

            _display += DisplayExceptions;
            _hide += ConcealExceptions;
            _hasSeparator = hasSeparator;
        }

        /// <summary>
        /// Handles the display.
        /// </summary>
        public delegate void DisplayHandler();

        /// <summary>
        /// Handles the hide.
        /// </summary>
        public delegate void HideHandler();

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Adds an exception to the error list.
        /// </summary>
        /// <typeparam name="T">Type of exception.</typeparam>
        /// <param name="exception">Information about the exception.</param>
        /// <param name="iconName">Icon to use.</param>
        public void Add<T>(WPFExceptionsDisplayItem<T> exception, WPFExceptionsIcons iconName = WPFExceptionsIcons.None)
            where T : Exception
        {
            if (_hasSeparator && _parent.Children.Count != 0)
            {
                _parent.Children.Add(new Separator());
            }

            _parent.Children.Add(BuildUI(exception, GetShellIcon(iconName)));
        }

        /// <summary>
        /// Remove the exception at position.
        /// </summary>
        /// <param name="value">Position to remove the exception.</param>
        public void RemoveAt(int value)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(value);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(value, _parent.Children.Count);

            _parent.Children.RemoveAt(value);
        }

        /// <summary>
        /// Removes an exception from the list of exceptions.
        /// </summary>
        /// <param name="name">Name of exception to remove.</param>
        public void Remove(string name)
        {
            if (name == _parent.Name)
            {
                return;
            }

            UIElement child = (UIElement)_parent.FindName(name);

            if (child != null)
            {
                _parent.Children.Remove(child);
            }
            else
            {
                Debug.WriteLine($"{name} does not exist in {_parent.Name}!!!");
            }
        }

        /// <summary>
        /// Removes an exception from the list of exceptions.
        /// </summary>
        /// <typeparam name="T">Type of exception to use.</typeparam>
        /// <param name="wPFExceptionsDisplayItem">Exception to remove.</param>
        public void Remove<T>(WPFExceptionsDisplayItem<T> wPFExceptionsDisplayItem)
            where T : Exception
        {
            Remove(wPFExceptionsDisplayItem.Name);
        }

        /// <summary>
        /// Removes an exception from the list of exceptions.
        /// </summary>
        /// <param name="child">UIElement exception to remove.</param>
        public void Remove(UIElement child)
        {
            if (child != _parent)
            {
                _parent.Children.Remove(child);
            }
        }

        /// <summary>
        /// Clears the exceptions.
        /// </summary>
        public void Clear()
        {
            _parent.Children.Clear();
        }

        /// <summary>
        /// Shows the exceptions.
        /// </summary>
        public void ShowExceptions()
        {
            _display!.Invoke();
        }

        /// <summary>
        /// Hides the exceptions.
        /// </summary>
        /// <param name="clearExceptions">When hidden, should the exceptions be cleared.</param>
        public void HideExceptions(bool clearExceptions = false)
        {
            if (clearExceptions)
            {
                Clear();
            }

            _hide!.Invoke();
        }

        /// <summary>
        /// Converts the colour to the brush.
        /// </summary>
        /// <param name="color">Colour to use.</param>
        /// <returns>The brush of the colour.</returns>
        protected static Brush ColourToBrush(Color color) => new SolidColorBrush(color);

        /// <summary>
        /// Builds the UI.
        /// </summary>
        /// <typeparam name="T">Type of exception to use.</typeparam>
        /// <param name="exception">Information about the exception.</param>
        /// <param name="iconInfo">Icon to use.</param>
        /// <returns>The built UIElement.</returns>
        protected virtual UIElement BuildUI<T>(WPFExceptionsDisplayItem<T> exception, BitmapSource iconInfo)
            where T : Exception
        {
            StackPanel exceptionInfo = new()
            {
                Orientation = Orientation.Horizontal,
                Background = ColourToBrush(exception.BackgroundColour),
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
                Foreground = ColourToBrush(exception.TextColour),
            });

            // https://stackoverflow.com/questions/13584998/how-to-add-a-vertical-separator
            exceptionInfo.Children.Add(new Rectangle()
            {
                VerticalAlignment = VerticalAlignment.Stretch,
                Fill = ColourToBrush(Colors.DarkGray),
                Width = 1,
            });

            exceptionInfo.Children.Add(new Label()
            {
                Content = exception.Description,
                Foreground = ColourToBrush(exception.TextColour),
            });

            return exceptionInfo;
        }

        /// <summary>
        /// Disposes values.
        /// </summary>
        /// <param name="disposing">Is the class being disposed.</param>
        protected virtual void Dispose(bool disposing)
        {
            _parent.Children.Clear();
            _display -= DisplayExceptions;
            _hide -= ConcealExceptions;
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
