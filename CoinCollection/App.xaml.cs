// <copyright file="App.xaml.cs" company="Karatekid2501">
// Copyright (c) Karatekid2501. All rights reserved.
// </copyright>

using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Primitives;

#if DEBUG
using System.Runtime.ExceptionServices;
#endif

namespace CoinCollection
{
    /// <summary>
    /// Interaction logic for App.xaml.
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Used for waiting for code to complete the writing of the config file.
        /// </summary>
        public readonly ManualResetEvent ConfigWait;

        /// <summary>
        /// Json editor for the config.
        /// </summary>
        public readonly JsonConfigEditor ConfigEditor;

        // Instance of the App
        private static App? _instance;

        private readonly IHost _host;

        private readonly IChangeToken _configToken;

        private readonly List<Currency> _currencies = [];

        private SQLReportingSystem? _report = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="App"/> class.
        /// </summary>
        public App()
        {
            if (_instance == null)
            {
#pragma warning disable S3010 // Static fields should not be updated in constructors
                _instance = this;
#pragma warning restore S3010 // Static fields should not be updated in constructors
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

            CustomMessageBoxWindow.DefualtParameters = new(Color.FromRgb(95, 158, 160));

            // Adds the defualt currency type Unknown
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

            _configToken.RegisterChangeCallback(
                state =>
                    {
                        var newConfig = (IConfiguration)state!;
                        ConnectionString = newConfig.GetConnectionString("DefaultConnection");
                        SQLDir = newConfig.GetValue<string>("SQL Dir");

                        ConfigWait.Set();
                    },
                _host.Services.GetRequiredService<IConfiguration>());
        }

        /// <summary>
        /// Gets SQL reporting system.
        /// </summary>
        public SQLReportingSystem Report
        {
            get { return _report ?? throw new ArgumentNullException(nameof(_report) ?? "_report", "_report is null!!!"); }
            private set { _report = value; }
        }

        /// <summary>
        /// Gets the directory of the SQL server.
        /// </summary>
        public string? SQLDir { get; private set; }

        /// <summary>
        /// Gets the string for the connection of the server.
        /// </summary>
        public string? ConnectionString { get; private set; }

        /// <summary>
        /// Gets readonly currency list.
        /// </summary>
        public ReadOnlyCollection<Currency> Currencies => _currencies.AsReadOnly();

        /// <summary>
        /// Gets the instance of App.
        /// </summary>
        /// <returns>Instance of App class.</returns>
        /// <exception cref="ArgumentNullException">Throws when an instance of App does not exists.</exception>
        public static App GetInstance()
        {
            if (_instance == null)
            {
                throw new ArgumentNullException(nameof(_instance) ?? "App Instance", "Instance of App already exists");
            }
            else
            {
                return _instance;
            }
        }

        /// <summary>
        /// Gets the service from IHost.
        /// </summary>
        /// <typeparam name="T">Type of service.</typeparam>
        /// <returns>The service.</returns>
        /// <exception cref="InvalidOperationException">Throws when the service is not found.</exception>
        public T GetService<T>()
            where T : notnull
        {
            T temp = _host.Services.GetRequiredService<T>();

            if (temp is null)
            {
                throw new InvalidOperationException($"Host does not have {nameof(T)}!!!");
            }
            else
            {
                return temp;
            }
        }

        /// <inheritdoc/>
        protected override async void OnStartup(StartupEventArgs e)
        {
            CheckAppSettingsExist();

            Report = new SQLReportingSystem(string.Empty, ConfigEditor.Get<bool>("Enabled", "Report Settings"));

            await _host.StartAsync();
            _host.Services.GetRequiredService<MainWindow>().Show(WindowStartupLocation.CenterScreen);
            base.OnStartup(e);
        }

        /// <inheritdoc/>
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
        /// Checks if the settings file exists and generates a new settings file if one is not present.
        /// </summary>
        private void CheckAppSettingsExist()
        {
            if (!ConfigEditor.ConfigFileExist)
            {
                MessageBox.Show("No settings file exists, creating new one!!!", "Warning", MessageBoxButton.OK);

                ConfigEditor.Create(
                    true,
                    new JsonValueEditGroup("ConnectionStrings", new JsonValueEdit<string>("DefaultConnection", string.Empty)),
                    new JsonValueEditGroupSingles(new JsonValueEdit<string>("SQL Dir", string.Empty)),
                    new JsonValueEditGroup(
                        "Report Settings",
                        new JsonValueEdit<bool>("Enabled", true),
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
