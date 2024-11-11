using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
//using System.Windows.Shapes;
using System.Configuration;
using Microsoft.Extensions.Configuration;
using CoinCollection.Models;
using System.IO;
using System.Diagnostics;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System.Runtime.InteropServices;
using Microsoft.EntityFrameworkCore;

namespace CoinCollection
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : AdvanceWindow
    {
        public readonly float VersionNumb = 0.01f;

        private readonly SQLContainer _container;

        private class Currency
        {
            public string CurrencyName {  get; private set; }

            public string[] CurrencyInfo { get; private set; }

            public Currency()
            {
                CurrencyName = "Unknown";
                CurrencyInfo = ["Unknown"];
            }

            public Currency(string fileLocation)
            {
                string[] fileInfo = File.ReadAllText(fileLocation).Split(',');

                List<string> tempCurrencyInfo = ["Unknown"];

                foreach(string info in fileInfo)
                {
                    if (info.StartsWith('[') && info.EndsWith(']'))
                    {
                        CurrencyName = info[1..info.IndexOf(']')];
                    }
                    else
                    {
                        tempCurrencyInfo.Add(info);
                    }
                }

                CurrencyInfo = [..tempCurrencyInfo];

                if(string.IsNullOrEmpty(CurrencyName))
                {
                    CurrencyName = Path.GetFileName(fileLocation);
                    CurrencyName = CurrencyName[..CurrencyName.IndexOf('.')];
                }
            }
        }

        private readonly List<Currency> _currencies = [];

        public MainWindow()
        {
            InitializeComponent();

            App.GetInstance().GetService<ServerSelectorWindow>().Loaded += ShowOverlay;
            App.GetInstance().GetService<ServerSelectorWindow>().Closed += HideOverlay;
            
            App.GetInstance().GetService<DataModificationWindow>().Loaded += ShowOverlay;
            App.GetInstance().GetService<DataModificationWindow>().Closed += HideOverlay;

            _currencies.Add(new());

            string[] currencyDirs = Directory.GetFiles(Path.Combine(Directory.GetCurrentDirectory(), "Currency"));

            foreach(string currencyDir in currencyDirs)
            {
                _currencies.Add(new (currencyDir));
            }

            _container = new();
        }

        public override void Show(WindowStartupLocation wsl, bool topMost = false)
        {
            base.Show(wsl, topMost);

            if(_container.CheckServerExistance())
            {
                UpdateTable();
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            App.GetInstance().GetService<ServerSelectorWindow>().Loaded -= ShowOverlay;
            App.GetInstance().GetService<ServerSelectorWindow>().Closed -= HideOverlay;

            App.GetInstance().GetService<DataModificationWindow>().Loaded -= ShowOverlay;
            App.GetInstance().GetService<DataModificationWindow>().Closed -= HideOverlay;

            base.OnClosed(e);
        }

        public void ShowOverlay(Visibility v)
        {
            Overlay.Visibility = v;
        }

        public bool ExistingServer(string loc)
        {
            return _container.ExistingServer(loc);
        }

        public bool NewServer(string loc)
        {
            return _container.NewServer(loc);
        }

        private void ShowOverlay(object? o, EventArgs e)
        {
            ShowOverlay(Visibility.Visible);
        }

        private void HideOverlay(object? o, EventArgs e)
        {
            ShowOverlay(Visibility.Collapsed);
        }

        private void MenuFileExit(object sender, RoutedEventArgs e)
        {
            //TODO: Need to ensure that the exit button on window calls this method when exiting

            //This fixes an issue where the program will softlock when attempting to close the main window when clicking on the exit button
            App.GetInstance().GetService<ServerSelectorWindow>().Close();

            Close();
        }

        private void MenuAbout(object sender, RoutedEventArgs e)
        {
            //TODO: Show overlay on main window

            MessageBox.Show($"Version: {VersionNumb} \nServer Version: {_container.GetServerColumnInfo<string>("ServerInfo", "VersionNumb")}", "About");
        }

        private void ModifyServerItem(object sender, RoutedEventArgs e)
        {

        }

        private void AddServerItem(object sender, RoutedEventArgs e)
        {
            App.GetInstance().GetService<DataModificationWindow>().ShowDialog(WindowStartupLocation.CenterScreen, true);
        }

        private void UpdateTable()
        {
            Coin_List.ItemsSource = _container.GetServerInfo().DefaultView;
        }
    }
}