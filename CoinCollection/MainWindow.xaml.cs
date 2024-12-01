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

        //private Visibility _overlayVis { get { return Overlay.Visibility; } set { Overlay.Visibility = value; } }

        public MainWindow() : base()
        {
            InitializeComponent();

            App.GetInstance().GetService<ServerSelectorWindow>().Loaded += ShowOverlay;
            App.GetInstance().GetService<ServerSelectorWindow>().Closed += HideOverlay;

            App.GetInstance().GetService<DataModificationWindow>().IsVisibleChanged += DMWIsVisableChanged;

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

            App.GetInstance().GetService<DataModificationWindow>().IsVisibleChanged -= DMWIsVisableChanged;

            App.GetInstance().GetService<DataModificationWindow>().Close();

            base.OnClosed(e);
        }

        public void ShowOverlay(Visibility v)
        {
            Overlay.Visibility = v;
            //_overlayVis = v;
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

        private void DMWIsVisableChanged(object? o, DependencyPropertyChangedEventArgs e)
        {
            if((bool)e.NewValue)
            {
                ShowOverlay(Visibility.Visible);
            }
            else
            {
                ShowOverlay(Visibility.Collapsed);
            }
        }

        protected override void Close_Click(object sender, RoutedEventArgs e)
        {
            //This fixes an issue where the program will softlock when attempting to close the main window when clicking on the exit button
            App.GetInstance().GetService<ServerSelectorWindow>().Close();
            base.Close_Click(sender, e);
        }

        private void MenuAbout(object sender, RoutedEventArgs e)
        {
            ShowOverlay(Visibility.Visible);
            MessageBox.Show($"Version: {VersionNumb} \nServer Version: {_container.GetServerColumnInfo<string>("ServerInfo", "VersionNumb")}", "About");
            ShowOverlay(Visibility.Hidden);
        }

        private void ModifyServerItem(object sender, RoutedEventArgs e)
        {
            App.GetInstance().GetService<DataModificationWindow>().ShowDialog(WindowStartupLocation.CenterScreen, true, DMWindowTitleName.Modify);
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