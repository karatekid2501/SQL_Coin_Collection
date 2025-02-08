using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Primitives;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Windows;
using System.Windows.Threading;

#if DEBUG
using System.Runtime.ExceptionServices;
#endif

namespace CoinCollection
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        //Instance of the App
        private static App? _instance;

        //Directory of the SQL server
        public string? SQLDir { get; private set; }

        //String for the connection of the server
        public string? ConnectionString { get; private set; }

        //Readonly currency list
        public ReadOnlyCollection<Currency> Currencies => _currencies.AsReadOnly();

        public readonly ManualResetEvent ConfigWait;

        public readonly JsonConfigEditor ConfigEditor;

        public readonly SQLReportingSystem Report;

        private readonly IHost _host;

        private readonly IChangeToken _configToken;

        private readonly List<Currency> _currencies = [];

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

            ConfigEditor = new JsonConfigEditor(string.Empty, new JsonSerializerOptions { WriteIndented = true });

            Current.DispatcherUnhandledException += UnhandleExceptions;
            AppDomain.CurrentDomain.UnhandledException += UnhandleExceptions;

            #if DEBUG

            AppDomain.CurrentDomain.FirstChanceException += FirstChanceException;

            #endif

            CheckAppSettingsExist();

            Report = new SQLReportingSystem(string.Empty, ConfigEditor.Get<bool>("Enabled", "Report Settings"));

            //Adds the defualt currency type Unknown
            _currencies.Add(new());

            string[] currencyDirs = Directory.GetFiles(Path.Combine(Directory.GetCurrentDirectory(), "Currency"));

            foreach (string currencyDir in currencyDirs)
            {
                _currencies.Add(new(currencyDir));
            }

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
                //TODO: Sort out
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
            Report.AddReport($"----------File Closed----------");

            Current.DispatcherUnhandledException -= UnhandleExceptions;
            AppDomain.CurrentDomain.UnhandledException -= UnhandleExceptions;

            #if DEBUG

            AppDomain.CurrentDomain.FirstChanceException -= FirstChanceException;

            #endif

            await _host.StopAsync();
            base.OnExit(e);
        }

        /// <summary>
        /// Checks if the settings file exists and generates a new settings file if one is not present
        /// </summary>
        private void CheckAppSettingsExist()
        {
            if (!ConfigEditor.ConfigFileExist)
            {
                //TODO: Fix issue when trying to display messagebox before IHost is created
                //MessageBox.Show("No settings file exists, creating new one!!!", "Warning", MessageBoxButton.OK);

                ConfigEditor.Create(true,
                    new JsonValueEditGroup("ConnectionStrings", new JsonValueEdit<string>("DefaultConnection", string.Empty)),
                    new JsonValueEditGroupSingles(new JsonValueEdit<string>("SQL Dir", string.Empty)),
                    new JsonValueEditGroup("Report Settings", new JsonValueEdit<bool>("Enabled", true),
                        new JsonValueEdit<string>("Report Frequency", "Daily")));
            }
        }

        private void UnhandleExceptions(object sender, DispatcherUnhandledExceptionEventArgs args)
        {
            UnhandleExceptions(args.Exception);
            args.Handled = true;
        }

        private void UnhandleExceptions(object sender, UnhandledExceptionEventArgs args)
        {
            UnhandleExceptions((Exception)args.ExceptionObject);
        }

        private void UnhandleExceptions(Exception ex)
        {
            Report.AddReport($"Unhandled exception: {ex.Message}", ReportSeverity.Error);
        }

        #if DEBUG

        private void FirstChanceException(object? sender, FirstChanceExceptionEventArgs args)
        {
            Report.AddReport($"Debug exception: {args.Exception.Message}", ReportSeverity.Error);
        }

        #endif
    }
}
