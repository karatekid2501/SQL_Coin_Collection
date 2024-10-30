using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Configuration;
using Microsoft.Extensions.Configuration;
using CoinCollection.Models;
using System.IO;
using System.Diagnostics;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System.Runtime.InteropServices;

namespace CoinCollection
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : AdvanceWindow
    {
        private readonly SQLContainer _container;

        private readonly IHost _host;

        public MainWindow(LocalDBMSSQLLocalDBContext context, IConfiguration config, IHost host)
        {
            InitializeComponent();

            _host = host;

            _host.Services.GetRequiredService<ServerSelectorWindow>().Loaded += ShowOverlay;
            _host.Services.GetRequiredService<ServerSelectorWindow>().Closed += HideOverlay;

            _container = new(context, config, host);
        }

        protected override void OnClosed(EventArgs e)
        {
            _host.Services.GetRequiredService<ServerSelectorWindow>().Loaded -= ShowOverlay;
            _host.Services.GetRequiredService<ServerSelectorWindow>().Closed -= HideOverlay;

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
    }
}