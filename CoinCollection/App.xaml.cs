using CoinCollection.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace CoinCollection
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly IHost _host;

        public App()
        {
            _host = Host.CreateDefaultBuilder().ConfigureAppConfiguration((context, config) =>
                {
                    config.SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json", false, true).Build();
                })
                .ConfigureServices((context, services) =>
                {
                    services.AddDbContext<LocalDBMSSQLLocalDBContext>(options => options.UseSqlServer(context.Configuration.GetConnectionString("DefaultConnection")));
                    services.AddSingleton<ServerSelectorWindow>();
                    services.AddSingleton<MainWindow>();

                }).Build();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            await _host.StartAsync();
            _host.Services.GetRequiredService<MainWindow>().Show(WindowStartupLocation.CenterScreen);
            base.OnStartup(e);
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            await _host.StopAsync();
            base.OnExit(e);
        }
    }
}
