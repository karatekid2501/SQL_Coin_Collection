using Microsoft.Win32;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace CoinCollection
{
    /// <summary>
    /// Generic class for methods that do not need their own classes
    /// </summary>
    internal static class Misc
    {
        public class OpenDialogContainer<T> where T : CommonItemDialog, new()
        {
            private readonly T _openDialog;
            private readonly MainWindowMethod _mwm;
            private readonly int _limitCheck;

            public OpenDialogContainer(MainWindowMethod mwm, int limitCheck = 128)
            {
                ArgumentNullException.ThrowIfNull(mwm);

                _mwm = mwm;

                _limitCheck = limitCheck;

                _openDialog = new T()
                {
                    InitialDirectory = Directory.GetCurrentDirectory(),
                };

                if (_openDialog is OpenFileDialog)
                {
                    OpenFileDialog? temp = _openDialog as OpenFileDialog ?? null;

                    if (temp != null)
                    {
                        temp.Filter = "SQL Server (*.mdf)|*.mdf";

                        _openDialog = (T)(CommonItemDialog)temp;
                    }
                    else
                    {
                        ArgumentNullException.ThrowIfNull(temp);
                    }
                }
            }

            public void Check(AdvanceWindow advanceWindow)
            {
                string loc;

                while (true)
                {
                    _openDialog.ShowDialog();

                    loc = _openDialog.ToString().IndexOfPos(':', 3, StringCleanUp.Trim);

                    if (loc.Length <= _limitCheck)
                    {
                        break;
                    }
                    else
                    {
                        MessageBox.Show($"Directory length over {_limitCheck}!!!", "Error");
                    }
                }

                if (!string.IsNullOrEmpty(loc))
                {
                    if (_mwm.Invoke(loc))
                    {
                        advanceWindow.Close();
                    }
                }
            }
        }

        public delegate bool MainWindowMethod(string loc);

        public static readonly OpenDialogContainer<OpenFileDialog> FileDialog = new(delegate (string loc) { return App.GetInstance().GetService<MainWindow>().ExistingServer(loc); });
        public static readonly OpenDialogContainer<OpenFolderDialog> FolderDialog = new(delegate (string loc) { return App.GetInstance().GetService<MainWindow>().NewServer(loc); }, 128 - 9);

        //Hard coded path for the image folder location
        private static readonly string _imagePath = Path.Combine(Directory.GetCurrentDirectory(), "Images");

        /// <summary>
        /// Creates a BitmapImage from path
        /// </summary>
        /// <param name="imageName">Name of the image</param>
        /// <returns>The generate BitmapImage. If no image is found, creates a blank image</returns>
        public static BitmapImage CreateImageFromPath(string imageName)
        {
            BitmapImage bitmapImage = new();

            if (!Path.Exists(Path.Combine(_imagePath, imageName)))
            {
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = new MemoryStream();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();

                return bitmapImage;
            }

            bitmapImage.BeginInit();
            bitmapImage.UriSource = new(Path.Combine(_imagePath, imageName), UriKind.RelativeOrAbsolute);
            bitmapImage.EndInit();

            return bitmapImage;
        }
    }
}
