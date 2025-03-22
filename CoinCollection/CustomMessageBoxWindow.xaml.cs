// <copyright file="CustomMessageBoxWindow.xaml.cs" company="Karatekid2501">
// Copyright (c) Karatekid2501. All rights reserved.
// </copyright>

using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CoinCollection
{
    /// <summary>
    /// Icon type for CustomImageInfo class.
    /// </summary>
    public enum CustomMessageBoxImageInfoIcon
    {
#pragma warning disable SA1602 // Enumeration items should be documented
        NONE = 0,
        IDI_ERROR = 32513,
        IDI_QUESTION = 32514,
        IDI_WARNING = 32515,
        IDI_INFORMATION = 32516,
#pragma warning restore SA1602 // Enumeration items should be documented
    }

    /// <summary>
    /// Custom checkbox for the CustomMessageBox window.
    /// </summary>
    public class CustomMessageBoxCheckBoxInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CustomMessageBoxCheckBoxInfo"/> class.
        /// </summary>
        /// <param name="info">Text for the check box label.</param>
        /// <param name="isChecked">Should the checkbox be checked already.</param>
        public CustomMessageBoxCheckBoxInfo(string info, bool isChecked)
        {
            Info = info;

            IsChecked = isChecked;
        }

        private CustomMessageBoxCheckBoxInfo()
        {
        }

        /// <summary>
        /// Gets what the checkbox label will say.
        /// </summary>
        public string Info { get; private set; } = string.Empty;

        /// <summary>
        /// Gets a value indicating whether the checkbox should be checked already.
        /// </summary>
        public bool IsChecked { get; private set; }
    }

    /// <summary>
    /// Custom button for the CustomMessageBox window.
    /// </summary>
    public class CustomMessageBoxButtonInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CustomMessageBoxButtonInfo"/> class.
        /// </summary>
        /// <param name="name">Button name.</param>
        /// <param name="clickEvent">Click event for the button.</param>
        /// <param name="isCancel">Is the button a cancel button.</param>
        public CustomMessageBoxButtonInfo(string name, Action? clickEvent = null, bool isCancel = false)
            : this(name, Colors.Black, Color.FromRgb(221, 221, 221), clickEvent, isCancel)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomMessageBoxButtonInfo"/> class.
        /// </summary>
        /// <param name="name">Button name.</param>
        /// <param name="textColour">Colour text for the button.</param>
        /// <param name="backgroundColour">Colour of the background for the button.</param>
        /// <param name="clickEvent">Click event for the button.</param>
        /// <param name="isCancel">Is the button a cancel button.</param>
        public CustomMessageBoxButtonInfo(string name, Color textColour, Color backgroundColour, Action? clickEvent = null, bool isCancel = false)
        {
            Name = name;

            TextColour = textColour;

            BackgroundColour = backgroundColour;

            ClickEvent = clickEvent;

            IsCancel = isCancel;
        }

        private CustomMessageBoxButtonInfo()
        {
        }

        /// <summary>
        /// Gets the name of the button.
        /// </summary>
        public string Name { get; private set; } = string.Empty;

        /// <summary>
        /// Gets the text colour of the button.
        /// </summary>
        public Color TextColour { get; private set; }

        /// <summary>
        /// Gets the background colour of the button.
        /// </summary>
        public Color BackgroundColour { get; private set; }

        /// <summary>
        /// Gets the click event for the button.
        /// </summary>
        public Action? ClickEvent { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the button is a cancel button.
        /// </summary>
        public bool IsCancel { get; private set; }

        /// <summary>
        /// Has the button got a click event.
        /// </summary>
        /// <returns>True if the click event is not null.</returns>
        public bool HasClickEvent()
        {
            return ClickEvent != null;
        }
    }

    /// <summary>
    /// Parameters for the CustomMessageBox window.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="CustomMessageBoxParameters"/> class.
    /// </remarks>
    /// <param name="titleColour">Title colour.</param>
    /// <param name="backgroundColor">Background colour.</param>
    /// <param name="textColour">Text colour.</param>
    /// <param name="wsl">Start location for the CustomMessageBox window.</param>
    /// <param name="topMost">Should CustomMessageBox window be at the top of other windows.</param>
    public class CustomMessageBoxParameters(Color titleColour, Color backgroundColor, Color textColour, WindowStartupLocation wsl = WindowStartupLocation.CenterScreen, Window? owner = null, bool topMost = true)
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CustomMessageBoxParameters"/> class.
        /// </summary>
        /// <param name="wsl">Start location for the CustomMessageBox window.</param>
        /// <param name="topMost">Should CustomMessageBox window be at the top of other windows.</param>
        public CustomMessageBoxParameters(WindowStartupLocation wsl = WindowStartupLocation.CenterScreen, Window? owner = null, bool topMost = true)
            : this(Colors.Gray, Colors.White, Colors.Black, wsl, owner, topMost)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomMessageBoxParameters"/> class.
        /// </summary>
        /// <param name="titleColour">Title colour.</param>
        /// <param name="wsl">Start location for the CustomMessageBox window.</param>
        /// <param name="topMost">Should CustomMessageBox window be at the top of other windows.</param>
        public CustomMessageBoxParameters(Color titleColour, WindowStartupLocation wsl = WindowStartupLocation.CenterScreen, Window? owner = null, bool topMost = true)
            : this(titleColour, Colors.White, Colors.Black, wsl, owner, topMost)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomMessageBoxParameters"/> class.
        /// </summary>
        /// <param name="titleColour">Title colour.</param>
        /// <param name="backgroundColor">Background colour.</param>
        /// <param name="wsl">Start location for the CustomMessageBox window.</param>
        /// <param name="topMost">Should CustomMessageBox window be at the top of other windows.</param>
        public CustomMessageBoxParameters(Color titleColour, Color backgroundColor, WindowStartupLocation wsl = WindowStartupLocation.CenterScreen, Window? owner = null, bool topMost = true)
            : this(titleColour, backgroundColor, Colors.Black, wsl, owner, topMost)
        {
        }

        /// <summary>
        /// Gets the title colour for the CustomMessageBox window.
        /// </summary>
        public Color TitleColour { get; private set; } = titleColour;

        /// <summary>
        /// Gets the background colour for the CustomMessageBox window.
        /// </summary>
        public Color BackgroundColour { get; private set; } = backgroundColor;

        /// <summary>
        /// Gets the text colour for the CustomMessageBox window.
        /// </summary>
        public Color TextColour { get; private set; } = textColour;

        /// <summary>
        /// Gets the position that a System.Windows.Window will be shown in when it is first opened.
        /// </summary>
        public WindowStartupLocation WSL { get; private set; } = wsl;

        /// <summary>
        /// Gets a value indicating whether a window appears in the topmost z-order.
        /// </summary>
        public bool TopMost { get; private set; } = topMost;

        public Window? Owner { get; private set; } = owner;
    }

    /// <summary>
    /// Image to display in the CustomMessageBox window.
    /// </summary>
    public partial class CustomMessageBoxImage
    {
#pragma warning disable SA1310 // Field names should not contain underscore
        private const uint IMAGE_ICON = 1;
        private const uint LR_SHARED = 0x00008000;
#pragma warning restore SA1310 // Field names should not contain underscore

        private readonly CustomMessageBoxImageInfoIcon _customImage = CustomMessageBoxImageInfoIcon.NONE;

        private readonly ImageSource? _imageSource;

        private readonly string _imagePath = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomMessageBoxImage"/> class.
        /// </summary>
        /// <param name="mib">Type of image to display.</param>
        public CustomMessageBoxImage(MessageBoxImage mib)
        {
            _customImage = mib switch
            {
                MessageBoxImage.Hand or MessageBoxImage.Error or MessageBoxImage.Stop => CustomMessageBoxImageInfoIcon.IDI_ERROR,
                MessageBoxImage.Warning or MessageBoxImage.Exclamation => CustomMessageBoxImageInfoIcon.IDI_WARNING,
                MessageBoxImage.Information or MessageBoxImage.Asterisk => CustomMessageBoxImageInfoIcon.IDI_INFORMATION,
                MessageBoxImage.Question => CustomMessageBoxImageInfoIcon.IDI_QUESTION,
                MessageBoxImage.None or _=> CustomMessageBoxImageInfoIcon.NONE,
            };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomMessageBoxImage"/> class.
        /// </summary>
        /// <param name="customImage">Type of image to display.</param>
        public CustomMessageBoxImage(CustomMessageBoxImageInfoIcon customImage)
        {
            _customImage = customImage;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomMessageBoxImage"/> class.
        /// </summary>
        /// <param name="image">Image to display.</param>
        public CustomMessageBoxImage(Image image)
            : this(image.Source)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomMessageBoxImage"/> class.
        /// </summary>
        /// <param name="imageSource">Image to display.</param>
        public CustomMessageBoxImage(ImageSource imageSource)
        {
            _imageSource = imageSource;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomMessageBoxImage"/> class.
        /// </summary>
        /// <param name="imagePath">Path to image to display.</param>
        public CustomMessageBoxImage(string imagePath)
        {
            _imagePath = imagePath;
        }

        /// <summary>
        /// Gets an ImageSource from one of the populated options.
        /// </summary>
        /// <returns>An ImageSource.</returns>
        /// <exception cref="ArgumentNullException">Throws when unable to image using image path.</exception>
        public ImageSource GetImageSource()
        {
            if (_customImage != CustomMessageBoxImageInfoIcon.NONE)
            {
                return GetMessageBoxIcon(_customImage);
            }
            else if (_imageSource != null)
            {
                return _imageSource;
            }
            else if (!string.IsNullOrEmpty(_imagePath))
            {
                if (File.Exists(_imagePath))
                {
                    return new ImageSourceConverter().ConvertFromString(_imagePath) as ImageSource ?? new Image().Source;
                }
                else
                {
                    throw new ArgumentNullException($"Unable to find image at [{_imagePath}]", nameof(_imagePath));
                }
            }
            else
            {
                return ExtraImageFunctions();
            }
        }

        /// <summary>
        /// Extra functions added to support child inheritence implementation.
        /// </summary>
        /// <returns>An ImageSource.</returns>
        protected virtual ImageSource ExtraImageFunctions()
        {
            Debug.WriteLine("No convertion avalible");

            return new Image().Source;
        }

        [LibraryImport("user32.dll", SetLastError = true)]
        private static partial IntPtr LoadImageW(IntPtr hInst, IntPtr lpszName, uint uType, int cx, int cy, uint fuLoad);

        private static BitmapSource GetMessageBoxIcon(CustomMessageBoxImageInfoIcon customImageInfoIcon)
        {
            IntPtr hIcon = LoadImageW(IntPtr.Zero, (int)customImageInfoIcon, IMAGE_ICON, 32, 32, LR_SHARED);

            if (hIcon == IntPtr.Zero)
            {
                throw new ArgumentNullException(nameof(hIcon) ?? "hIcon", "Unable to find icon.");
            }

            return Imaging.CreateBitmapSourceFromHIcon(hIcon, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
        }
    }

    /// <summary>
    /// Interaction logic for CustomMessageBoxWindow.xaml.
    /// </summary>
    public partial class CustomMessageBoxWindow : AdvanceWindow
    {
        private readonly CustomMessageBoxParameters _parameters;

        private bool _useDefualtValues = true;

        private int _result = 0;

        private Size _currentMonitorSize = Size.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomMessageBoxWindow"/> class.
        /// </summary>
        /// <param name="window">Owner of this window.</param>
        private CustomMessageBoxWindow()
            : this(DefualtParameters)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomMessageBoxWindow"/> class.
        /// </summary>
        /// <param name="parameters">Parameters for how the message box will appeare.</param>
        private CustomMessageBoxWindow(CustomMessageBoxParameters parameters)
            : base(
                  new AdvanceSystemMenu(
                      AdvanceSystemMenuDefualtButtons.SC_RESTORE | AdvanceSystemMenuDefualtButtons.SC_SIZE |
                      AdvanceSystemMenuDefualtButtons.SC_MINIMIZE | AdvanceSystemMenuDefualtButtons.SC_MAXIMIZE |
                      AdvanceSystemMenuDefualtButtons.SC_CLOSE),
                  false,
                  "!",
                  "!",
                  "Exit",
                  false)
        {
            _parameters = parameters;

            Owner = _parameters.Owner;

            Topmost = _parameters.TopMost;

            InitializeComponent();

            Exit.IsEnabled = false;

            Title_Background.Background = new SolidColorBrush(_parameters.TitleColour);

            Main_Background.Background = new SolidColorBrush(_parameters.BackgroundColour);

            Description.Foreground = new SolidColorBrush(_parameters.TextColour);

            Extra_Option_Label.Foreground = Description.Foreground;

            // https://stackoverflow.com/questions/11013316/get-the-height-width-of-window-wpf
            SizeChanged += OnWindowSizeChanged;
        }

        /// <summary>
        /// Gets or sets the defualt parameters (Note: does not need to be set at start of application).
        /// </summary>
        public static CustomMessageBoxParameters DefualtParameters { get; set; } = new();

        public static int Show()
        {
            CustomMessageBoxWindow instance = new();

            instance.ShowDialog(WindowStartupLocation.CenterScreen, true);

            return instance._result;
        }

        public static int Show(string title)
        {
            return Show(new CustomMessageBoxWindow(), title, string.Empty, null, null, null, null);
        }

        public static int Show(string title, string description)
        {
            return Show(new CustomMessageBoxWindow(), title, description, null, null, null, null);
        }

        public static int Show(string title, string description, CustomMessageBoxImage imageInfo)
        {
            return Show(new CustomMessageBoxWindow(), title, description, imageInfo, null, null, null);
        }

        public static int Show(string title, string description, CustomMessageBoxImage imageInfo, CustomMessageBoxButtonInfo button1)
        {
            return Show(new CustomMessageBoxWindow(), title, description, imageInfo, button1, null, null);
        }

        public static int Show(string title, string description, CustomMessageBoxImage imageInfo, CustomMessageBoxButtonInfo button1, CustomMessageBoxButtonInfo button2)
        {
            return Show(new CustomMessageBoxWindow(), title, description, imageInfo, button1, button2, null);
        }

        public static int Show(string title, string description, CustomMessageBoxImage imageInfo, CustomMessageBoxButtonInfo button1, CustomMessageBoxButtonInfo button2, CustomMessageBoxButtonInfo button3)
        {
            return Show(new CustomMessageBoxWindow(), title, description, imageInfo, button1, button2, button3);
        }

        public static int Show(string title, string description, CustomMessageBoxImage imageInfo, CustomMessageBoxCheckBoxInfo cbInfo, out bool checkBoxResult)
        {
            return Show(new CustomMessageBoxWindow(), title, description, imageInfo, cbInfo, out checkBoxResult, null, null, null);
        }

        public static int Show(string title, string description, CustomMessageBoxImage imageInfo, CustomMessageBoxCheckBoxInfo cbInfo, out bool checkBoxResult, CustomMessageBoxButtonInfo button1)
        {
            return Show(new CustomMessageBoxWindow(), title, description, imageInfo, cbInfo, out checkBoxResult, button1, null, null);
        }

        public static int Show(string title, string description, CustomMessageBoxImage imageInfo, CustomMessageBoxCheckBoxInfo cbInfo, out bool checkBoxResult, CustomMessageBoxButtonInfo button1, CustomMessageBoxButtonInfo button2)
        {
            return Show(new CustomMessageBoxWindow(), title, description, imageInfo, cbInfo, out checkBoxResult, button1, button2, null);
        }

        public static int Show(string title, string description, CustomMessageBoxImage imageInfo, CustomMessageBoxCheckBoxInfo cbInfo, out bool checkBoxResult, CustomMessageBoxButtonInfo button1, CustomMessageBoxButtonInfo button2, CustomMessageBoxButtonInfo button3)
        {
            return Show(new CustomMessageBoxWindow(), title, description, imageInfo, cbInfo, out checkBoxResult, button1, button2, button3);
        }

        public static int Show(string title, string description, CustomMessageBoxImage imageInfo, CustomMessageBoxCheckBoxInfo cbInfo, out bool? checkBoxResult)
        {
            return Show(new CustomMessageBoxWindow(), title, description, imageInfo, cbInfo, out checkBoxResult, null, null, null);
        }

        public static int Show(string title, string description, CustomMessageBoxImage imageInfo, CustomMessageBoxCheckBoxInfo cbInfo, out bool? checkBoxResult, CustomMessageBoxButtonInfo button1)
        {
            return Show(new CustomMessageBoxWindow(), title, description, imageInfo, cbInfo, out checkBoxResult, button1, null, null);
        }

        public static int Show(string title, string description, CustomMessageBoxImage imageInfo, CustomMessageBoxCheckBoxInfo cbInfo, out bool? checkBoxResult, CustomMessageBoxButtonInfo button1, CustomMessageBoxButtonInfo button2)
        {
            return Show(new CustomMessageBoxWindow(), title, description, imageInfo, cbInfo, out checkBoxResult, button1, button2, null);
        }

        public static int Show(string title, string description, CustomMessageBoxImage imageInfo, CustomMessageBoxCheckBoxInfo cbInfo, out bool? checkBoxResult, CustomMessageBoxButtonInfo button1, CustomMessageBoxButtonInfo button2, CustomMessageBoxButtonInfo button3)
        {
            return Show(new CustomMessageBoxWindow(), title, description, imageInfo, cbInfo, out checkBoxResult, button1, button2, button3);
        }

        public static int Show(CustomMessageBoxParameters parameters)
        {
            return Show(new CustomMessageBoxWindow(parameters), string.Empty, string.Empty, null, null, null, null);
        }

        public static int Show(CustomMessageBoxParameters parameters, string title)
        {
            return Show(new CustomMessageBoxWindow(parameters), title, string.Empty, null, null, null, null);
        }

        public static int Show(CustomMessageBoxParameters parameters, string title, string description)
        {
            return Show(new CustomMessageBoxWindow(parameters), title, description, null, null, null, null);
        }

        public static int Show(CustomMessageBoxParameters parameters, string title, string description, CustomMessageBoxImage imageInfo)
        {
            return Show(new CustomMessageBoxWindow(parameters), title, description, imageInfo, null, null, null);
        }

        public static int Show(CustomMessageBoxParameters parameters, string title, string description, CustomMessageBoxImage imageInfo, CustomMessageBoxButtonInfo button1)
        {
            return Show(new CustomMessageBoxWindow(parameters), title, description, imageInfo, button1, null, null);
        }

        public static int Show(CustomMessageBoxParameters parameters, string title, string description, CustomMessageBoxImage imageInfo, CustomMessageBoxButtonInfo button1, CustomMessageBoxButtonInfo button2)
        {
            return Show(new CustomMessageBoxWindow(parameters), title, description, imageInfo, button1, button2, null);
        }

        public static int Show(CustomMessageBoxParameters parameters, string title, string description, CustomMessageBoxImage imageInfo, CustomMessageBoxButtonInfo button1, CustomMessageBoxButtonInfo button2, CustomMessageBoxButtonInfo button3)
        {
            return Show(new CustomMessageBoxWindow(parameters), title, description, imageInfo, button1, button2, button3);
        }

        public static int Show(CustomMessageBoxParameters parameters, string title, string description, CustomMessageBoxImage imageInfo, CustomMessageBoxCheckBoxInfo cbInfo, out bool checkBoxResult)
        {
            return Show(new CustomMessageBoxWindow(parameters), title, description, imageInfo, cbInfo, out checkBoxResult, null, null, null);
        }

        public static int Show(CustomMessageBoxParameters parameters, string title, string description, CustomMessageBoxImage imageInfo, CustomMessageBoxCheckBoxInfo cbInfo, out bool checkBoxResult, CustomMessageBoxButtonInfo button1)
        {
            return Show(new CustomMessageBoxWindow(parameters), title, description, imageInfo, cbInfo, out checkBoxResult, button1, null, null);
        }

        public static int Show(CustomMessageBoxParameters parameters, string title, string description, CustomMessageBoxImage imageInfo, CustomMessageBoxCheckBoxInfo cbInfo, out bool checkBoxResult, CustomMessageBoxButtonInfo button1, CustomMessageBoxButtonInfo button2)
        {
            return Show(new CustomMessageBoxWindow(parameters), title, description, imageInfo, cbInfo, out checkBoxResult, button1, button2, null);
        }

        public static int Show(CustomMessageBoxParameters parameters, string title, string description, CustomMessageBoxImage imageInfo, CustomMessageBoxCheckBoxInfo cbInfo, out bool checkBoxResult, CustomMessageBoxButtonInfo button1, CustomMessageBoxButtonInfo button2, CustomMessageBoxButtonInfo button3)
        {
            return Show(new CustomMessageBoxWindow(parameters), title, description, imageInfo, cbInfo, out checkBoxResult, button1, button2, button3);
        }

        public static int Show(CustomMessageBoxParameters parameters, string title, string description, CustomMessageBoxImage imageInfo, CustomMessageBoxCheckBoxInfo cbInfo, out bool? checkBoxResult)
        {
            return Show(new CustomMessageBoxWindow(parameters), title, description, imageInfo, cbInfo, out checkBoxResult, null, null, null);
        }

        public static int Show(CustomMessageBoxParameters parameters, string title, string description, CustomMessageBoxImage imageInfo, CustomMessageBoxCheckBoxInfo cbInfo, out bool? checkBoxResult, CustomMessageBoxButtonInfo button1)
        {
            return Show(new CustomMessageBoxWindow(parameters), title, description, imageInfo, cbInfo, out checkBoxResult, button1, null, null);
        }

        public static int Show(CustomMessageBoxParameters parameters, string title, string description, CustomMessageBoxImage imageInfo, CustomMessageBoxCheckBoxInfo cbInfo, out bool? checkBoxResult, CustomMessageBoxButtonInfo button1, CustomMessageBoxButtonInfo button2)
        {
            return Show(new CustomMessageBoxWindow(parameters), title, description, imageInfo, cbInfo, out checkBoxResult, button1, button2, null);
        }

        public static int Show(CustomMessageBoxParameters parameters, string title, string description, CustomMessageBoxImage imageInfo, CustomMessageBoxCheckBoxInfo cbInfo, out bool? checkBoxResult, CustomMessageBoxButtonInfo button1, CustomMessageBoxButtonInfo button2, CustomMessageBoxButtonInfo button3)
        {
            return Show(new CustomMessageBoxWindow(parameters), title, description, imageInfo, cbInfo, out checkBoxResult, button1, button2, button3);
        }

        private static int Show(CustomMessageBoxWindow instance, string title, string description, CustomMessageBoxImage? imageInfo, CustomMessageBoxButtonInfo? button1, CustomMessageBoxButtonInfo? button2, CustomMessageBoxButtonInfo? button3)
        {
            return Show(instance, title, description, imageInfo, null, out bool? _, button1, button2, button3);
        }

        private static int Show(CustomMessageBoxWindow instance, string title, string description, CustomMessageBoxImage? imageInfo, CustomMessageBoxCheckBoxInfo? cbInfo, out bool checkBoxResult, CustomMessageBoxButtonInfo? button1, CustomMessageBoxButtonInfo? button2, CustomMessageBoxButtonInfo? button3)
        {
            int result = Show(instance, title, description, imageInfo, cbInfo, out bool? checkBoxResultNull, button1, button2, button3);

            checkBoxResult = (bool)checkBoxResultNull!;

            return result;
        }

        private static int Show(CustomMessageBoxWindow instance, string title, string description, CustomMessageBoxImage? imageInfo, CustomMessageBoxCheckBoxInfo? cbInfo, out bool? checkBoxResult, CustomMessageBoxButtonInfo? button1, CustomMessageBoxButtonInfo? button2, CustomMessageBoxButtonInfo? button3)
        {
            instance._useDefualtValues = false;

            instance.Populate(title, description, imageInfo, cbInfo, button1, button2, button3);

            instance.ShowDialog(WindowStartupLocation.CenterScreen, true);

            checkBoxResult = instance.Extra_Option.IsChecked;

            return instance._result;
        }

        /*public static int Show(Window window, string title)
        {
            //return Show(new CustomMessageBoxParameters(), window);
        }

        public static int Show(CustomMessageBoxParameters parameters, Window window, string title)
        {
            //return Show(new CustomMessageBoxParameters(), window);
        }*/

        /*
        /// <summary>
        /// Opens a message.
        /// </summary>
        /// <param name="title">Title of the message box.</param>
        /// <returns>Result of the selection.</returns>
        public int Show(string title)
        {
            return Show(title, string.Empty, null, null);
        }

        /// <summary>
        /// Opens a message.
        /// </summary>
        /// <param name="title">Title of the message box.</param>
        /// <param name="description">Description of the message box.</param>
        /// <returns>Result of the selection.</returns>
        public int Show(string title, string description)
        {
            return Show(title, description, null, null);
        }

        /// <summary>
        /// Opens a message.
        /// </summary>
        /// <param name="title">Title of the message box.</param>
        /// <param name="description">Description of the message box.</param>
        /// <param name="imageInfo">Kind of image t display.</param>
        /// <returns>Result of the selection.</returns>
        public int Show(string title, string description, CustomMessageBoxImage imageInfo)
        {
            return Show(title, description, imageInfo, null);
        }

        /// <summary>
        /// Opens a message.
        /// </summary>
        /// <param name="title">Title of the message box.</param>
        /// <param name="description">Description of the message box.</param>
        /// <param name="imageInfo">Kind of image t display.</param>
        /// <param name="cbInfo">Information about the extra check box.</param>
        /// <param name="buttons">The buttons to use (Note: Limited to a max of three).</param>
        /// <returns>Result of the selection.</returns>
        public int Show(string title, string description, CustomMessageBoxImage? imageInfo, CustomMessageBoxCheckBoxInfo? cbInfo, params CustomMessageBoxButtonInfo[] buttons)
        {
            _useDefualtValues = false;

            Populate(title, description, imageInfo, cbInfo, buttons);

            return ShowDialog(_parameters.WSL, _parameters.TopMost);
        }

        /// <summary>
        /// Opens a message.
        /// </summary>
        /// <param name="title">Title of the message box.</param>
        /// <param name="description">Description of the message box.</param>
        /// <param name="imageInfo">Kind of image t display.</param>
        /// <param name="cbInfo">Information about the extra check box.</param>
        /// <param name="checkBoxResult">Result of check box.</param>
        /// <param name="buttons">The buttons to use (Note: Limited to a max of three).</param>
        /// <returns>Result of the selection.</returns>
        public int Show(string title, string description, CustomMessageBoxImage? imageInfo, CustomMessageBoxCheckBoxInfo? cbInfo, out bool checkBoxResult, params CustomMessageBoxButtonInfo[] buttons)
        {
            int result = Show(title, description, imageInfo, cbInfo, out bool? checkBoxResultNullable, buttons);

            checkBoxResult = (bool)checkBoxResultNullable!;

            return result;
        }

        /// <summary>
        /// Opens a message.
        /// </summary>
        /// <param name="title">Title of the message box.</param>
        /// <param name="description">Description of the message box.</param>
        /// <param name="imageInfo">Kind of image t display.</param>
        /// <param name="cbInfo">Information about the extra check box.</param>
        /// <param name="checkBoxResult">Result of check box.</param>
        /// <param name="buttons">The buttons to use (Note: Limited to a max of three).</param>
        /// <returns>Result of the selection.</returns>
        public int Show(string title, string description, CustomMessageBoxImage? imageInfo, CustomMessageBoxCheckBoxInfo? cbInfo, out bool? checkBoxResult, params CustomMessageBoxButtonInfo[] buttons)
        {
            _useDefualtValues = false;

            Populate(title, description, imageInfo, cbInfo, buttons);

            int result = ShowDialog(_parameters.WSL, _parameters.TopMost);

            checkBoxResult = Extra_Option.IsChecked;

            return result;
        }*/

        /*
        /// <summary>
        /// Opens a message.
        /// </summary>
        /// <param name="wsl">Start up location of the window.</param>
        /// <param name="topMost">Should the window always be on top.</param>
        /// <returns>Result of the message.</returns>
        public new int Show(WindowStartupLocation wsl, bool topMost = false)
        {
            return ShowDialog(wsl, topMost);
        }

        /// <summary>
        /// Opens a message.
        /// </summary>
        /// <returns>Result of the message.</returns>
        public new int Show()
        {
            return ShowDialog();
        }

        /// <summary>
        /// Opens a message. (Note: Please use Show method instead.)
        /// </summary>
        /// <param name="wsl">Start up location of the window.</param>
        /// <param name="topMost">Should the window always be on top.</param>
        /// <returns>Result of the message.</returns>
        public new int ShowDialog(WindowStartupLocation wsl, bool topMost = false)
        {
            base.ShowDialog(wsl, topMost);

            return _result;
        }

        /// <summary>
        /// Opens a message. (Note: Please use Show method instead.)
        /// </summary>
        /// <returns>Result of the message.</returns>
        public new int ShowDialog()
        {
            base.ShowDialog();

            return _result;
        }*/

        /// <inheritdoc/>
        protected override void Close_Click(object sender, RoutedEventArgs e)
        {
            _result = 0;

            base.Close_Click(sender, e);
        }

        /// <inheritdoc/>
        protected override void OnSourceInitialized(EventArgs e)
        {
            if (_useDefualtValues)
            {
                Populate();
            }

            base.OnSourceInitialized(e);
        }

        /// <inheritdoc/>
        protected override void OnClosed(EventArgs e)
        {
            SizeChanged -= OnWindowSizeChanged;

            base.OnClosed(e);
        }

        /// <summary>
        /// Triggers when the window resizes.
        /// </summary>
        /// <param name="sender">What triggered the event.</param>
        /// <param name="e">Event arguments.</param>
        protected void OnWindowSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_currentMonitorSize == Size.Empty || !CheckCurrentMonitorSize())
            {
                _currentMonitorSize = new()
                {
                    // https://learn.microsoft.com/en-us/answers/questions/252411/how-to-scale-wpf-application-according-to-screen-r
                    // https://www.codeproject.com/Articles/9656/Dissecting-the-MessageBox
                    Width = SystemParameters.PrimaryScreenWidth * 0.6f,
                    Height = SystemParameters.PrimaryScreenHeight * 0.9f,
                };

                MaxWidth = _currentMonitorSize.Width;
                MaxHeight = _currentMonitorSize.Height;

                Description.MaxWidth = MaxWidth;
                Description.MaxHeight = MaxHeight;
            }
        }

        private bool CheckCurrentMonitorSize()
        {
            return (_currentMonitorSize.Width <= SystemParameters.PrimaryScreenWidth - float.Epsilon && _currentMonitorSize.Width >= SystemParameters.PrimaryScreenWidth + float.Epsilon)
                || (_currentMonitorSize.Height <= SystemParameters.PrimaryScreenHeight - float.Epsilon && _currentMonitorSize.Height >= SystemParameters.PrimaryScreenHeight + float.Epsilon);
        }

        //private void Populate(string title = "Info", string description = "Are you sure?", CustomMessageBoxImage? imageInfo = null, CustomMessageBoxCheckBoxInfo? cbInfo = null, params CustomMessageBoxButtonInfo[] buttons)
        private void Populate(string title = "Info", string description = "Are you sure?", CustomMessageBoxImage? imageInfo = null, CustomMessageBoxCheckBoxInfo? cbInfo = null, params CustomMessageBoxButtonInfo?[] buttons)
        {
            /*if (buttons.Length > 3)
            {
                throw new ArgumentOutOfRangeException(nameof(buttons), "Not allowed to exceed 3 buttons");
            }*/

            Title = title;
            Description.Text = description;

            if (imageInfo != null)
            {
                Icon_Image.Source = imageInfo.GetImageSource();
            }

            if (cbInfo != null)
            {
                Options.Visibility = Visibility.Visible;

                Extra_Option_Label.Content = cbInfo.Info;

                Extra_Option.IsChecked = cbInfo.IsChecked;
            }

            /*if (buttons.Length == 0)
            {
                Button_Parent.Children.Add(ButtonCreation(new CustomMessageBoxButtonInfo("Ok"), 1));
            }
            else
            {
                for (int i = 0; i < buttons.Length; i++)
                {
                    Button_Parent.Children.Add(ButtonCreation(buttons[i], i + 1));
                }
            }*/

            if (buttons.Length == 0 || buttons[0] == null)
            {
                Button_Parent.Children.Add(ButtonCreation(new CustomMessageBoxButtonInfo("Ok"), 1));
            }
            else
            {
                Button_Parent.Children.Add(ButtonCreation(buttons[0]!, 1));

                for (int i = 1; i < buttons.Length; i++)
                {
                    if (buttons[i] != null)
                    {
                        Button_Parent.Children.Add(ButtonCreation(buttons[i]!, i + 1));
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

        private Button ButtonCreation(CustomMessageBoxButtonInfo bInfo, int value)
        {
            Button newButton = new()
            {
                Content = bInfo.Name,
                Background = new SolidColorBrush(bInfo.BackgroundColour),
                Foreground = new SolidColorBrush(bInfo.TextColour),
                MinWidth = 75,
                Margin = new Thickness(5, 0, 0, 0),
                Style = (Style)FindResource("MessageBoxStyle"),
            };

            if (bInfo.IsCancel)
            {
                value = 0;
            }

            if (bInfo.HasClickEvent())
            {
                newButton.Click += (s, e) => Click(value, bInfo.ClickEvent!);
            }
            else
            {
                newButton.Click += (s, e) => Click(value);
            }

            return newButton;
        }

        private void Click(int value, Action clickEvent)
        {
            clickEvent();

            Click(value);
        }

        private void Click(int value)
        {
            _result = value;

            Close();
        }
    }
}
