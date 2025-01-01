﻿using Microsoft.Win32;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
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
    /// <summary>
    /// Stores common information for checking if the textbox was modified
    /// </summary>
    /// <param name="textBoxInfo">Altered text from textbox</param>
    /// <param name="label">Modified label</param>
    /// <param name="number">Number to alter</param>
    public class TextBoxCommand(string textBoxInfo, Label label, int number)
    {
        public string TextInfo { get; set; } = textBoxInfo;
        public Label Label { get; set; } = label;
        public int Number { get; set; } = number;
    }

    /// <summary>
    /// Converts the textbox information group into TextBoxCommand
    /// </summary>
    public class TextBoxCommandConverter : IMultiValueConverter
    {
        public int ValueAmount { get; set; }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length == ValueAmount && values[0] is string text && values[1] is Label label && values[2] is int number)
            {
                return new TextBoxCommand(text, label, number);
            }

            return null!;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Interaction logic for DataModificationWindow.xaml
    /// </summary>
    public partial class DataModificationWindow : AdvanceWindow
    {
        #region Get window icons
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
        #endregion

        public ICommand DeselectCommand { get; }
        public ICommand DeselectCheckCurrencyNameCommand { get; }

        public ICommand Test { get; }

        private readonly string _imagePath;

        private readonly OpenFileDialog _openFileDialog;

        private readonly string _defualtImageName = "No Coin Image.jpg";

        //https://stackoverflow.com/questions/1268552/how-do-i-get-a-textbox-to-only-accept-numeric-input-in-wpf
        private static readonly Regex _regex = new ("[^0-9]+");

        //Checks if the name of the coin already exists
        private bool _dupeName = false;

        private ServerDataContainer? _serverDataContainer;

        private static MainWindow GetMainWindow { get { return App.GetInstance().GetService<MainWindow>(); } }

        public DataModificationWindow() : base(true)
        {
            DeselectCommand = new RelayCommand(Deselect);
            DeselectCheckCurrencyNameCommand = new RelayCommand(DeselectCheckCurrencyName);

            Test = new RelayCommand<TextBoxCommand>(DeselectTextBox);

            DataContext = this;

            InitializeComponent();

            _imagePath = Path.Combine(Directory.GetCurrentDirectory(), "Images");

            Currency_Type_ComboBox.ItemsSource = App.GetInstance().Currencies;
            Currency_Type_ComboBox.DisplayMemberPath = "CurrencyName";
            Currency_Type_ComboBox.SelectedValuePath = "CurrencyInfo";
            Currency_Type_ComboBox.SelectedIndex = 0;

            IsVisibleChanged += Visable;
            Calender_Date_Selector.SelectedDatesChanged += Calender_Date_Selector_SelectedDatesChanged;

            Calender_Date_Selector.SelectedDate = DateTime.Now;

            _openFileDialog = new OpenFileDialog()
            {
                InitialDirectory = Directory.GetCurrentDirectory(),
                Filter = "JPG (*.jpg)|*.jpg|PNG (*.png)|*.png|JPEG (*.jpeg)|*.jpeg"
            };

            Image_Warning_Icon.Source = GetShellIcon();
            Name_Warning_Icon.Source = GetShellIcon();
            Submit_Error_Info_Icon.Source = GetShellIcon();

            Name_Warning_Group.Visibility = Visibility.Hidden;

            UpdateCoinImages();
        }

        private void Calender_Date_Selector_SelectedDatesChanged(object? sender, SelectionChangedEventArgs e)
        {
            Calender_Date_Selected_Label.Content = Calender_Date_Selector.SelectedDate!.Value.ToShortDateString();
        }

        public virtual void Show(WindowStartupLocation wsl, bool topMost = false, ServerDataContainer? serverDataContainer = null)
        {
            if(serverDataContainer == null)
            {
                Title = "New";
            }
            else
            {
                Title = "Modify";
            }

            _serverDataContainer = serverDataContainer;

            base.Show(wsl, topMost);
        }

        public virtual bool? ShowDialog(WindowStartupLocation wsl, bool topMost = false, ServerDataContainer? serverDataContainer = null)
        {
            if (serverDataContainer == null)
            {
                Title = "New";
            }
            else
            {
                Title = "Modify";
            }

            _serverDataContainer = serverDataContainer;

            return base.ShowDialog(wsl, topMost);
        }

        protected override void OnClosed(EventArgs e)
        {
            IsVisibleChanged -= Visable;
            Calender_Date_Selector.SelectedDatesChanged -= Calender_Date_Selector_SelectedDatesChanged;

            base.OnClosed(e);
        }

        private void Visable(object sender, DependencyPropertyChangedEventArgs e)
        {
            if(IsVisible)
            {
                if(_serverDataContainer == null)
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

                    Calender_Date_Selector.SelectedDate = DateTime.Now;

                    Submit_Error_Info_Group.Visibility = Visibility.Hidden;

                    Name_Warning_Group.Visibility = Visibility.Hidden;

                    Submit.IsEnabled = false;
                }
                else
                {
                    Name_Textbox.Text = _serverDataContainer[0];
                    Description_Textbox.Text = _serverDataContainer[1];

                    if (_serverDataContainer[2] == "∅")
                    {
                        Calender_Date_Selector.SelectedDate = DateTime.Now;
                    }
                    else
                    {
                        Calender_Date_Selector.SelectedDate = DateTime.Parse(_serverDataContainer[2]);
                    }
                    
                    Amount_Made_Textbox.Text = _serverDataContainer[3];

                    if (ComboBoxContains<Currency>(Currency_Type_ComboBox, x => x.CurrencyName == _serverDataContainer[4]))
                    {
                        ComboBoxContains(Original_Value_ComboBox, _serverDataContainer[5]);
                    }

                    Retail_Value_Textbox.Text = _serverDataContainer[6];

                    ComboBoxContains(Image_ComboBox, _serverDataContainer[7]);
                }
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
                Original_Value_ComboBox.SelectedIndex = 0;
                Original_Value_ComboBox.ItemsSource = selectedCurrency.CurrencyInfo;

                /*if(_serverDataContainer != null && IsLoaded)
                {
                    if (selectedCurrency.CurrencyName != _serverDataContainer[4])
                    {
                        Currency_Type_New.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        Currency_Type_New.Visibility = Visibility.Hidden;
                    }

                    AllowToSubmit();
                }*/

                CheckModified(selectedCurrency.CurrencyName, 4, ref Currency_Type_New);
            }
        }

        private void Original_Value_ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            /*if(_serverDataContainer != null && IsLoaded)
            {
                if (Original_Value_ComboBox.Items[Original_Value_ComboBox.SelectedIndex].ToString() != _serverDataContainer[5])
                {
                    Original_Value_New.Visibility = Visibility.Visible;
                }
                else
                {
                    Original_Value_New.Visibility = Visibility.Hidden;
                }

                AllowToSubmit();
            }*/

            CheckModified(Original_Value_ComboBox.Items[Original_Value_ComboBox.SelectedIndex].ToString()!, 5, ref Original_Value_New);
        }

        private void Image_ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(Image_ComboBox.Items.Count == 0)
            {
                return;
            }

            string imageName = Image_ComboBox.SelectedItem.ToString()!;

            /*BitmapImage bitmapImage = new();
            bitmapImage.BeginInit();
            bitmapImage.UriSource = new(Path.Combine(_imagePath, imageName), UriKind.RelativeOrAbsolute);
            bitmapImage.EndInit();

            Image_Viwer.Source = bitmapImage;*/

            Image_Viwer.Source = Misc.CreateImageFromPath(imageName);

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

            /*if(_serverDataContainer != null)
            {
                if(imageName != _serverDataContainer[7])
                {
                    Image_New.Visibility = Visibility.Visible;
                }
                else
                {
                    Image_New.Visibility = Visibility.Hidden;
                }

                AllowToSubmit();
            }*/

            CheckModified(imageName, 7, ref Image_New);
        }

        private void LostFocusSQLNameCheck(object sender, RoutedEventArgs e)
        {
            DupeNameCheck();
            AllowToSubmit();
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
            if(_serverDataContainer == null)
            {
                Submit.IsEnabled = !string.IsNullOrEmpty(Name_Textbox.Text) && !string.IsNullOrEmpty(Description_Textbox.Text) && !string.IsNullOrEmpty(Retail_Value_Textbox.Text) && !_dupeName;
            }
            else
            {
                Submit.IsEnabled = !string.IsNullOrEmpty(Name_Textbox.Text) && !string.IsNullOrEmpty(Description_Textbox.Text) && !string.IsNullOrEmpty(Retail_Value_Textbox.Text) && !_dupeName && 
                    (Description_New.IsVisible || Calender_New.IsVisible || Amount_Made_New.IsVisible || Currency_Type_New.IsVisible || Original_Value_New.IsVisible || Retail_Value_New.IsVisible || Image_New.IsVisible);
            }
        }

        private void SubmitButton(object sender, RoutedEventArgs e)
        {
            if(GetMainWindow.SubmitNew(new ServerDataContainer(Name_Textbox.Text, Description_Textbox.Text, Calender_Date_Selector.SelectedDate!.Value.ToShortDateString(),
                Amount_Made_Textbox.Text, Currency_Type_ComboBox.Text, Original_Value_ComboBox.Text, Retail_Value_Textbox.Text, Image_ComboBox.Text), out Exception exeption))
            {
                Visibility = Visibility.Hidden;
            }
            else
            {
                Submit_Error_Info_Group.Visibility = Visibility.Visible;
                Submit_Error_Info_Label.Content = exeption.Message;
            }
        }

        private void DeselectCheckCurrencyName()
        {
            DupeNameCheck();
            Deselect();
        }

        private void DeselectTextBox(TextBoxCommand textBoxCommand)
        {
            if(_serverDataContainer != null)
            {
                if(textBoxCommand.TextInfo != _serverDataContainer[textBoxCommand.Number])
                {
                    textBoxCommand.Label.Visibility = Visibility.Visible;
                }
                else
                {
                    textBoxCommand.Label.Visibility = Visibility.Hidden;
                }
            }

            Deselect();
        }

        private void Deselect()
        {
            Keyboard.ClearFocus();
            AllowToSubmit();
        }

        private void IsTextAllowed(object sender, TextCompositionEventArgs e)
        {
            e.Handled = _regex.IsMatch(e.Text);
        }

        private void DupeNameCheck()
        {
            if (_serverDataContainer != null)
            {
                if (_serverDataContainer[0] != Name_Textbox.Text)
                {
                    //Debug.WriteLine((int)GetMainWindow.ExecuteScalar(new SQLCommandFactory().Select("").Count().From("Coin").Where("Name", Name_Textbox.Text).And().Not("Name", _serverDataContainer[0]).ToSQLCommand()));

                    _dupeName = (int)GetMainWindow.ExecuteScalar(new SQLCommandFactory().Select("").Count().From("Coin").Where("Name", Name_Textbox.Text).And().Not("Name", _serverDataContainer[0]).ToSQLCommand()) > 0;

                    IsDupeName();

                    Name_New.Visibility = Visibility.Visible;
                }
                else
                {
                    _dupeName = false;
                    Name_New.Visibility = Visibility.Hidden;
                    Name_Warning_Group.Visibility = Visibility.Hidden;
                }
            }
            else
            {
                //Debug.WriteLine((int)GetMainWindow.ExecuteScalar(new SQLCommandFactory().Select("").Count().From("Coin").Where("Name", Name_Textbox.Text).ToSQLCommand()));
                //_dupeName = GetMainWindow.CheckMoneyNameExists(Name_Textbox.Text);
                _dupeName = (int)GetMainWindow.ExecuteScalar(new SQLCommandFactory().Select("").Count().From("Coin").Where("Name", Name_Textbox.Text).ToSQLCommand()) > 0;

                IsDupeName();
            }
        }

        /// <summary>
        /// Checks if a combobox contains a certain item by checking a paramiter with in the item using predicate
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="comboBox">Combobox to check</param>
        /// <param name="item">Item to check from using preficate</param>
        /// <returns>True if the combobox contains the item</returns>
        private static bool ComboBoxContains<T>(ComboBox comboBox, Predicate<T> item)
        {
            for(int i = 0; i < comboBox.Items.Count; i++)
            {
                if (item((T)comboBox.Items[i]))
                {
                    comboBox.SelectedIndex = i;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Checks if a combobox contains a certain item
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="comboBox">Combobox to check</param>
        /// <param name="item">Item to check from</param>
        /// <returns>True if the combobox contains the item</returns>
        private static bool ComboBoxContains<T>(ComboBox comboBox, T item)
        {
            for (int i = 0; i < comboBox.Items.Count; i++)
            {
                if (Equals(comboBox.Items[i], item))
                {
                    comboBox.SelectedIndex = i;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Check if coin name already exists in the SQL server
        /// </summary>
        private void IsDupeName()
        {
            if (_dupeName)
            {
                Name_Warning_Group.Visibility = Visibility.Visible;

                Name_Warning_Label.Content = $"{Name_Textbox.Text} already exists!!!";
            }
            else
            {
                Name_Warning_Group.Visibility = Visibility.Hidden;
            }
        }

        /// <summary>
        /// Checks if the text has been modified and if its deferent than the original text, show the modified label
        /// </summary>
        /// <param name="modifiedItem">Text that was modified</param>
        /// <param name="index">Index to check the original text</param>
        /// <param name="modifiedLabel">Show the modified lable if the text does not match the original text</param>
        private void CheckModified(string modifiedItem, int index, ref Label modifiedLabel)
        {
            if (_serverDataContainer != null && IsLoaded)
            {
                if (modifiedItem != _serverDataContainer[index])
                {
                    modifiedLabel.Visibility = Visibility.Visible;
                }
                else
                {
                    modifiedLabel.Visibility = Visibility.Hidden;
                }

                AllowToSubmit();
            }
        }
    }
}
