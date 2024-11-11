using CoinCollection.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Primitives;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Windows;

namespace CoinCollection
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static App? _instance;

        public string? SQLDir { get; private set; }

        public string? ConnectionString { get; private set; }

        public readonly ManualResetEvent ConfigWait;

        private readonly IHost _host;

        private readonly IChangeToken _configToken;

        public App()
        {
            if(_instance == null)
            {
                _instance = this;
            }
            else if (_instance != this)
            {
                throw new InvalidOperationException("Instance of App already exists");
            }

            CheckAppSettingsExist();

            _host = Host.CreateDefaultBuilder().ConfigureAppConfiguration((context, config) =>
                {
                    config.SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json", false, true).Build();
                })
                .ConfigureServices((context, services) =>
                {
                    services.AddSingleton<DataModificationWindow>();
                    services.AddSingleton<ServerSelectorWindow>();
                    services.AddSingleton<MainWindow>();

                    ConnectionString = context.Configuration.GetConnectionString("DefaultConnection");
                    SQLDir = context.Configuration.GetValue<string>("SQL Dir");

                }).Build();

            _configToken = _host.Services.GetRequiredService<IConfiguration>().GetReloadToken();

            ConfigWait = new ManualResetEvent(false);

            _configToken.RegisterChangeCallback(state =>
            {
                var newConfig = (IConfiguration)state!;
                ConnectionString = newConfig.GetConnectionString("DefaultConnection");
                SQLDir = newConfig.GetValue<string>("SQL Dir");

                ConfigWait.Set();
            }, _host.Services.GetRequiredService<IConfiguration>());
        }

        public static App GetInstance()
        {
            if (_instance == null)
            {
                throw new InvalidOperationException("Instance of App already exists");
            }
            else
            {
                return _instance;
            }
        }

        public T GetService<T>() where T : notnull
        {
            T temp = _host.Services.GetRequiredService<T>();

            if(temp == null)
            {
                throw new InvalidOperationException($"Host does not have {nameof(T)}!!!");
            }
            else
            {
                return temp;
            }
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

        private static void CheckAppSettingsExist()
        {
            if(!File.Exists(Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json")))
            {
                //TODO: Fix issue when trying to disaply messagebox before IHost is created
                //MessageBox.Show("No settings file exists, creating new one!!!", "Warning", MessageBoxButton.OK);

                JsonObject jObject = new()
                {
                    ["ConnectionStrings"] = new JsonObject
                    {
                        ["DefaultConnection"] = string.Empty
                    },

                    ["SQL Dir"] = string.Empty
                };

                File.WriteAllText(Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json"), jObject.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
            }
        }
    }
}
