// <copyright file="Misc.cs" company="Karatekid2501">
// Copyright (c) Karatekid2501. All rights reserved.
// </copyright>

using System.IO;
using System.Text;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

namespace CoinCollection
{
    /// <summary>
    /// Generic class for methods that do not need their own classes.
    /// </summary>
    internal static class Misc
    {
        /// <summary>
        /// Saves a database.
        /// </summary>
        public static readonly FileSaveOpenContainer<SaveFileDialog> SaveFile = new("Create New Server", MainWindowLoadCreate.CreateDatabase);

        /// <summary>
        /// Opens a database.
        /// </summary>
        public static readonly FileSaveOpenContainer<OpenFileDialog> OpenFile = new("Select Server", MainWindowLoadCreate.LoadDatabase);

        /// <summary>
        /// Backups a database.
        /// </summary>
        public static readonly FileSaveOpenContainer<SaveFileDialog> BackupFile = new("Backup Server", MainWindowLoadCreate.BackupDatabase, "SQL Server Backup", "bak");

        // Hard coded path for the image folder location
        private static readonly string _imagePath = Path.Combine(Directory.GetCurrentDirectory(), "Images");

        /// <summary>
        /// Creates a BitmapImage from path.
        /// </summary>
        /// <param name="imageName">Name of the image.</param>
        /// <returns>The generate BitmapImage. If no image is found, creates a blank image.</returns>
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

        /// <summary>
        /// Container class for both save and open FileDialog.
        /// </summary>
        /// <typeparam name="T">Generic class that both SaveFileDialog and OpenFileDialog use.</typeparam>
        public class FileSaveOpenContainer<T>
            where T : FileDialog, new()
        {
            private readonly T _fileFolderDialog;

            private readonly Type _type;

            private readonly string _tile;

            private readonly string _fileName = string.Empty;

            private readonly MainWindowLoadCreate _mwlc;

            /// <summary>
            /// Initializes a new instance of the <see cref="FileSaveOpenContainer{T}"/> class.
            /// </summary>
            /// <param name="tile">Title of the window.</param>
            /// <param name="mwlc">Command database type to the MainWindow class.</param>
            /// <param name="filterName">Name of the filter used.</param>
            /// <param name="extensionType">Type of the extension used.</param>
            public FileSaveOpenContainer(string tile, MainWindowLoadCreate mwlc, string filterName = "SQL Server", string extensionType = ".mdf")
            {
                _mwlc = mwlc;

                if (!extensionType.StartsWith('.'))
                {
                    extensionType = $".{extensionType}";
                }

                _fileFolderDialog = new T()
                {
                    InitialDirectory = Directory.GetCurrentDirectory(),
                    Filter = $"{filterName} (*{extensionType})|*{extensionType}",
                    DefaultExt = extensionType,
                };

                _tile = tile;

                if (_fileFolderDialog is SaveFileDialog)
                {
                    _type = typeof(SaveFileDialog);
                    _fileName = "Coins";
                }
                else if (_fileFolderDialog is OpenFileDialog)
                {
                    _type = typeof(OpenFileDialog);
                }
                else
                {
                    throw new InvalidCastException($"Unable to convert {typeof(T)} to either SaveFileDialog or OpenFolderDialog.");
                }
            }

            /// <summary>
            /// Checks reather the Dialog was successful.
            /// </summary>
            /// <param name="advanceWindow">Window to close if the Dialog was successful.</param>
            /// <param name="fileNameOverride">Overrides the file name.</param>
            /// <param name="extraInfo">Additional information.</param>
            public void Check(AdvanceWindow? advanceWindow = null, string fileNameOverride = "", object? extraInfo = null)
            {
                Reset();

                if (!string.IsNullOrEmpty(fileNameOverride))
                {
                    _fileFolderDialog.FileName = fileNameOverride;
                }

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
                        success = CreateLoad(instacne.GetService<MainWindow>(), loc, sb, "Successfully created", "Failed to create", extraInfo);
                    }
                    else
                    {
                        success = CreateLoad(instacne.GetService<MainWindow>(), loc, sb, "Successfully selected", "Failed to select", extraInfo);
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

            private bool CreateLoad(MainWindow mainWindow, string loc, StringBuilder sb, string successMessage, string failedMessage, object? extraInfo)
            {
                bool success = mainWindow.CreateLoadDatabase(loc, _mwlc, extraInfo);

                if (success)
                {
                    sb.Insert(0, $"{successMessage} ");
                }
                else
                {
                    sb.Insert(0, $"{failedMessage} ");
                }

                return success;
            }

            /// <summary>
            /// Resets the information that FileDialog uses.
            /// </summary>
            private void Reset()
            {
                _fileFolderDialog.Title = _tile;
                _fileFolderDialog.FileName = _fileName;
            }
        }
    }
}
