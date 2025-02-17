using Microsoft.Win32;
using System.IO;
using System.Text;
using System.Windows.Media.Imaging;

namespace CoinCollection
{
    /// <summary>
    /// Generic class for methods that do not need their own classes
    /// </summary>
    internal static class Misc
    {
        /*public class OpenDialogContainer<T> where T : CommonItemDialog, new()
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

        public static readonly OpenDialogContainer<OpenFileDialog> FileDialog = new(delegate (string loc) { 
            bool success = App.GetInstance().GetService<MainWindow>().ExistingServer(loc);

            int serverNamePos = loc.LastIndexOf('\\');
            string report = $"{loc[(serverNamePos + 1)..]} at {loc[..serverNamePos]}";

            if (success) 
            {
                App.GetInstance().Report.AddReport($"Successfully got {report}", ReportSeverity.Info);
                return true;
            }
            else
            {
                App.GetInstance().Report.AddReport($"Unable to get {report}", ReportSeverity.Error);
                return false;
            }
        });

        public static readonly OpenDialogContainer<OpenFolderDialog> FolderDialog = new(delegate (string loc) {
            bool success = App.GetInstance().GetService<MainWindow>().NewServer(loc);

            int serverNamePos = loc.LastIndexOf('\\');
            string report = $"Creation of {loc[(serverNamePos + 1)..]} at {loc[..serverNamePos]}";

            if(success)
            {
                App.GetInstance().Report.AddReport($"{report} was a success", ReportSeverity.Info);
                return true;
            }
            else
            {
                App.GetInstance().Report.AddReport($"{report} failed", ReportSeverity.Error);
                return false;
            }

        }, 128 - 9);*/

        /// <summary>
        /// Container class for both save and open FileDialog
        /// </summary>
        /// <typeparam name="T">Generic class that both SaveFileDialog and OpenFileDialog use</typeparam>
        public class FileSaveOpenContainer<T> where T : FileDialog, new()
        {
            private readonly T _fileFolderDialog;

            private readonly Type _type;

            private readonly string _tile;

            private readonly string _fileName = "";

            public FileSaveOpenContainer()
            {
                _fileFolderDialog = new T()
                {
                    InitialDirectory = Directory.GetCurrentDirectory(),
                    Filter = "SQL Server (*.mdf)|*.mdf",
                    DefaultExt = ".mdf"
                };

                if(_fileFolderDialog is SaveFileDialog)
                {
                    _type = typeof(SaveFileDialog);
                    _tile = "Create New Server";
                    _fileName = "Coins";
                }
                else if (_fileFolderDialog is OpenFileDialog)
                {
                    _type = typeof(OpenFileDialog);
                    _tile = "Select Server";
                }
                else
                {
                    throw new InvalidCastException($"Unable to convert {typeof(T)} to either SaveFileDialog or OpenFolderDialog.");
                }
            }

            /// <summary>
            /// Checks reather the Dialog was successful
            /// </summary>
            /// <param name="advanceWindow">Window to close if the Dialog was successful</param>
            public void Check(AdvanceWindow? advanceWindow = null)
            {
                Reset();

                if (_fileFolderDialog.ShowDialog() == true)
                {
                    string loc = _fileFolderDialog.FileName;

                    App instacne = App.GetInstance();

                    bool success;

                    StringBuilder sb = new();

                    int serverNamePos = loc.LastIndexOf('\\');
                    sb.Append($"{loc[(serverNamePos + 1)..]} at {loc[..serverNamePos]}");

                    if (_type == typeof(SaveFileDialog))
                    {
                        success = instacne.GetService<MainWindow>().NewServer(loc);

                        if (success)
                        {
                            sb.Insert(0, "Successfully created ");
                        }
                        else
                        {
                            sb.Insert(0, "Failed to create ");
                        }
                    }
                    else
                    {
                        success = instacne.GetService<MainWindow>().ExistingServer(loc);

                        if (success)
                        {
                            sb.Insert(0, "Successfully selected ");
                        }
                        else
                        {
                            sb.Insert(0, "Failed to select ");
                        }
                    }

                    if (success)
                    {
                        instacne.Report.AddReport(sb.ToString(), ReportSeverity.Info);

                        advanceWindow?.Close();
                    }
                    else
                    {
                        instacne.Report.AddReport(sb.ToString(), ReportSeverity.Error);
                    }
                }
            }

            /// <summary>
            /// Resets the information that FileDialog uses
            /// </summary>
            private void Reset()
            {
                _fileFolderDialog.Title = _tile;
                _fileFolderDialog.FileName = _fileName;
            }
        }

        public static readonly FileSaveOpenContainer<SaveFileDialog> SaveFile = new();

        public static readonly FileSaveOpenContainer<OpenFileDialog> OpenFile = new();

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
