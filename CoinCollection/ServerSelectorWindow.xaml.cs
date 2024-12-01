using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static System.Net.WebRequestMethods;

namespace CoinCollection
{
    /// <summary>
    /// Interaction logic for ServerSelectorWindow.xaml
    /// </summary>
    public partial class ServerSelectorWindow : AdvanceWindow
    {
        private class OpenDialogContainer<T> where T : CommonItemDialog, new()
        {
            private readonly T _openDialog;
            private readonly MainWindowMethod _mwm;
            private readonly int _limitCheck;
            private readonly ServerSelectorWindow _window;

            public OpenDialogContainer(ServerSelectorWindow window, MainWindowMethod mwm, int limitCheck = 128)
            {
                ArgumentNullException.ThrowIfNull(window);
                ArgumentNullException.ThrowIfNull(mwm);

                _window = window;
                _mwm = mwm;

                _limitCheck = limitCheck;

                _openDialog = new T()
                { 
                    InitialDirectory = Directory.GetCurrentDirectory(),
                };

                if(_openDialog is OpenFileDialog)
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

            public void Check()
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
                    if(_mwm.Invoke(loc))
                    {
                        _window.Close();
                    }
                }
            }
        }

        public bool IsCancled { get; private set; } = false;

        private delegate bool MainWindowMethod(string loc);

        private readonly OpenDialogContainer<OpenFileDialog> _fileDialog;
        private readonly OpenDialogContainer<OpenFolderDialog> _folderDialog;

        public ServerSelectorWindow()
        {
            _fileDialog = new(this, delegate (string loc) { return App.GetInstance().GetService<MainWindow>().ExistingServer(loc); });
            _folderDialog = new(this, delegate (string loc) { return App.GetInstance().GetService<MainWindow>().NewServer(loc); }, 128 - 9);

            InitializeComponent();
        }

        public void CloseWindow()
        {
            Close();
            App.GetInstance().GetService<MainWindow>().Close();
        }

        protected override void Close_Click(object sender, RoutedEventArgs e)
        {
            IsCancled = true;
            CloseWindow();
        }

        private void ButtonNew(object sender, RoutedEventArgs e)
        {
            _folderDialog.Check();
        }

        private void ButtonSelect(object sender, RoutedEventArgs e)
        {
            _fileDialog.Check();
        }

        private void ButtonClose(object sender, RoutedEventArgs e)
        {
            //TODO Consider changing the Close_Click
            IsCancled = true;
            CloseWindow();
        }
    }
}
