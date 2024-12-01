using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CoinCollection
{
    public enum DMWindowTitleName
    {
        New,
        Modify
    }

    /// <summary>
    /// Interaction logic for DataModificationWindow.xaml
    /// </summary>
    public partial class DataModificationWindow : AdvanceWindow
    {
        private static BitmapSource GetShellIcon()
        {
            IntPtr hIcon = LoadIcon(IntPtr.Zero, 32515); // IDI_WARNING

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

        public ICommand DeselectCommand { get; }

        private readonly string _imagePath;

        private readonly OpenFileDialog _openFileDialog;

        private readonly string _defualtImageName = "No Coin Image.jpg";

        public DataModificationWindow() : base(true)
        {
            DeselectCommand = new RelayCommand(Deselect);
            DataContext = this;

            InitializeComponent();

            _imagePath = Path.Combine(Directory.GetCurrentDirectory(), "Images");

            Currency_Type_ComboBox.ItemsSource = App.GetInstance().Currencies;
            Currency_Type_ComboBox.DisplayMemberPath = "CurrencyName";
            Currency_Type_ComboBox.SelectedValuePath = "CurrencyInfo";
            Currency_Type_ComboBox.SelectedIndex = 0;

            IsVisibleChanged += Visable;

            _openFileDialog = new OpenFileDialog()
            {
                InitialDirectory = Directory.GetCurrentDirectory(),
                Filter = "JPG (*.jpg)|*.jpg|PNG (*.png)|*.png"
            };

            Image_Warning_Icon.Source = GetShellIcon();

            UpdateCoinImages();
        }

        public virtual void Show(WindowStartupLocation wsl, bool topMost = false, DMWindowTitleName titleName = DMWindowTitleName.New)
        {
            Title = titleName.ToString();

            base.Show(wsl, topMost);
        }

        public virtual void ShowDialog(WindowStartupLocation wsl, bool topMost = false, DMWindowTitleName titleName = DMWindowTitleName.New)
        {
            Title = titleName.ToString();

            base.ShowDialog(wsl, topMost);
        }

        protected override void OnClosed(EventArgs e)
        {
            IsVisibleChanged -= Visable;

            base.OnClosed(e);
        }

        private void Visable(object sender, DependencyPropertyChangedEventArgs e)
        {
            if(!IsVisible)
            {
                Image_ComboBox.SelectedIndex = 0;
                Currency_Type_ComboBox.SelectedIndex = 0;

                Name_Textbox.Text = string.Empty;
                Description_Textbox.Text = string.Empty;
                Amount_Made_Textbox.Text = string.Empty;

                Currency_Type_ComboBox.SelectedIndex = 0;
                Original_Value_ComboBox.SelectedIndex = 0;

                Retail_Value_Textbox.Text = string.Empty;

                Image_ComboBox.SelectedIndex = 0;
            }
        }

        private void UpdateCoinImages()
        {
            string[] imageNames = Directory.GetFiles(_imagePath);

            Image_ComboBox.Items.Clear();

            int defualtPos = -1;

            foreach (string imageName in imageNames)
            {
                Image_ComboBox.Items.Add(Path.GetFileName(imageName));

                if(Path.GetFileName(imageName) == _defualtImageName)
                {
                    defualtPos = Image_ComboBox.Items.Count - 1;
                }
            }

            if(defualtPos != -1 && defualtPos != 0)
            {
                (Image_ComboBox.Items[defualtPos], Image_ComboBox.Items[0]) = (Image_ComboBox.Items[0], Image_ComboBox.Items[defualtPos]);
            }

            Image_ComboBox.SelectedIndex = 0;
        }

        private void UpdateCoinImages(object sender, RoutedEventArgs e)
        {
            UpdateCoinImages();
            Image_ComboBox_SelectionChanged(sender, null!);
        }

        private void Currency_Type_ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(Currency_Type_ComboBox.SelectedItem is Currency selectedCurrency)
            {
                Original_Value_ComboBox.ItemsSource = selectedCurrency.CurrencyInfo;
                Original_Value_ComboBox.SelectedIndex = 0;
            }
        }

        private void Image_ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(Image_ComboBox.Items.Count == 0)
            {
                return;
            }

            string imageName = Image_ComboBox.SelectedItem.ToString()!;

            BitmapImage bitmapImage = new();
            bitmapImage.BeginInit();
            bitmapImage.UriSource = new(Path.Combine(_imagePath, imageName), UriKind.RelativeOrAbsolute);
            bitmapImage.EndInit();

            Image_Viwer.Source = bitmapImage;

            Image_ComboBox.ToolTip = imageName;

            Image_Name.Content = imageName;

            if(Image_Viwer.Source.Width != 250 || Image_Viwer.Source.Height != 250)
            {
                Image_Warning_Group.Visibility = Visibility.Visible;

                Image_Warning.Content = $"Size not correct ({Math.Round(Image_Viwer.Source.Width)} X {Math.Round(Image_Viwer.Source.Height)})!!!";
            }
            else
            {
                Image_Warning_Group.Visibility= Visibility.Hidden;
            }
        }

        private void LostFocusTextBoxEvent(object sender, RoutedEventArgs e)
        {
            AllowToSubmit();
        }

        private void AddNewImage(object sender, RoutedEventArgs e)
        {
            if(_openFileDialog.ShowDialog() == true)
            {
                string newImagePath = Path.Combine(_imagePath, _openFileDialog.SafeFileName);

                if (File.Exists(newImagePath))
                {
                    MessageBox.Show($"{_openFileDialog.SafeFileName} already exists!!!", "Error");
                }
                else
                {
                    MessageBoxResult imageWarning = MessageBoxResult.None;

                    using (SixLabors.ImageSharp.Image image = SixLabors.ImageSharp.Image.Load(_openFileDialog.FileName))
                    {
                        if(image.Width != 250 || image.Height != 250)
                        {
                            imageWarning = MessageBox.Show($"{_openFileDialog.SafeFileName} is not 250 x 250. Are you sure you want to use this image?", "Warning", MessageBoxButton.YesNo);
                        }
                    }

                    if(imageWarning == MessageBoxResult.None || imageWarning == MessageBoxResult.Yes)
                    {
                        File.Copy(_openFileDialog.FileName, newImagePath);
                        UpdateCoinImages();
                    }
                }
            }
        }

        private void AllowToSubmit()
        {
            Submit.IsEnabled = !string.IsNullOrEmpty(Name_Textbox.Text) && !string.IsNullOrEmpty(Description_Textbox.Text) && !string.IsNullOrEmpty(Retail_Value_Textbox.Text);
        }

        private void Deselect()
        {
            Keyboard.ClearFocus();
        }
    }
}
