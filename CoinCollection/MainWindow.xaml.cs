using System.Windows;
using System.Windows.Controls;
using System.Data;
using Microsoft.Data.SqlClient;

namespace CoinCollection
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : AdvanceWindow
    {
        //Current server version
        public readonly float VersionNumb = 0.01f;

        private readonly SQLContainer _container;

        private DataRowView? _dataRowView;

        private ServerDataContainer? _serverDataContainer;

        public MainWindow() : base()
        {
            InitializeComponent();

            App.GetInstance().GetService<ServerSelectorWindow>().Loaded += ShowOverlay;
            App.GetInstance().GetService<ServerSelectorWindow>().Closed += HideOverlay;

            App.GetInstance().GetService<DataModificationWindow>().IsVisibleChanged += DMWIsVisableChanged;

            _container = new();

            Enable_Reporting.IsChecked = App.GetInstance().ConfigEditor.Get<bool>("Enabled", "Report Settings");

            Ensable_Reporting(Enable_Reporting.IsChecked);

            string reportFrequncy = App.GetInstance().ConfigEditor.Get<string>("Report Frequency", "Report Settings");

            switch(reportFrequncy)
            {
                case "Daily":
                    RFCF_Daily.IsChecked = true;
                    break;
                case "Weekly":
                    RFCF_Weekly.IsChecked = true;
                    break;
                case "Monthly":
                    RFCF_Monthly.IsChecked = true;
                    break;
                default:
                    throw new Exception($"{reportFrequncy} is not a frequency type!!!");
            }
        }

        public override void Show(WindowStartupLocation wsl, bool topMost = false)
        {
            base.Show(wsl, topMost);

            if(_container.CheckServerExistance())
            {
                Coin_List.DisplayMemberPath = "Name";
                Coin_List.SelectedValuePath = "Id";

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

        /// <summary>
        /// Should the overlay be shown
        /// </summary>
        /// <param name="v">Visability of the overlay</param>
        public void ShowOverlay(Visibility v)
        {
            Overlay.Visibility = v;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="loc"></param>
        /// <returns></returns>
        public bool ExistingServer(string loc)
        {
            return _container.ExistingServer(loc);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="loc"></param>
        /// <returns></returns>
        public bool NewServer(string loc)
        {
            return _container.NewServer(loc);
        }

        /// <summary>
        /// Executes a Transact-SQL statement againts the connection and returns the number of rows affected
        /// </summary>
        /// <param name="sqlCommand">The SQL command that will be executed</param>
        /// <returns>The number of rows that are affected</returns>
        public int ExecuteNonQuery(SqlCommand sqlCommand)
        {
            return _container.ExecuteNonQuery(sqlCommand);
        }

        /// <summary>
        /// Tries to execute a Transact-SQL statement againts the connection and returns the number of rows affected
        /// </summary>
        /// <param name="sqlCommand">The SQL command that will be executed</param>
        /// <param name="sqlEx">The SQL exeption that is thrown if something goes wrong</param>
        /// <param name="ex">The execption that is thrown if something goes wrong</param>
        /// <returns>The number of rows that are affected</returns>
        public int TryExecuteNonQuery(SqlCommand sqlCommand, out SqlException sqlEx, out Exception ex)
        {
            return _container.TryExecuteNonQuery(sqlCommand, out sqlEx, out ex);
        }

        /// <summary>
        /// Tries to execute a Transact-SQL statement againts the connection and returns the number of rows affected
        /// </summary>
        /// <param name="sqlCommand">The SQL command that will be executed</param>
        /// <param name="sqlEx">The SQL exeption that is thrown if something goes wrong</param>
        /// <returns>The number of rows that are affected</returns>
        public int TryExecuteNonQuery(SqlCommand sqlCommand, out SqlException sqlEx)
        {
            return _container.TryExecuteNonQuery(sqlCommand, out sqlEx, out _);
        }

        /// <summary>
        /// Tries to execute a Transact-SQL statement againts the connection and returns the number of rows affected
        /// </summary>
        /// <param name="sqlCommand">The SQL command that will be executed</param>
        /// <param name="ex">The execption that is thrown if something goes wrong</param>
        /// <returns>The number of rows that are affected</returns>
        public int TryExecuteNonQuery(SqlCommand sqlCommand, out Exception ex)
        {
            return _container.TryExecuteNonQuery(sqlCommand, out _, out ex);
        }

        /// <summary>
        /// Executes the query, and returns the first column of the first row in the result set returned by the query. Additional columns or rows are ignored
        /// </summary>
        /// <param name="sqlCommand">The SQL command that will be executed</param>
        /// <returns>The first column of the first row in the result, or null reference if the result set is empty. Returns a maximum of 2033 characters</returns>
        public object ExecuteScalar(SqlCommand sqlCommand)
        {
            return _container.ExecuteScalar(sqlCommand);
        }

        /// <summary>
        /// Trys to execute the query, and returns the first column of the first row in the result set returned by the query. Additional columns or rows are ignored
        /// </summary>
        /// <param name="sqlCommand">The SQL command that will be executed</param>
        /// <param name="sqlEx">The SQL exeption that is thrown if something goes wrong</param>
        /// <param name="ex">The execption that is thrown if something goes wrong</param>
        /// <returns>The first column of the first row in the result, or null reference if the result set is empty. Returns a maximum of 2033 characters</returns>
        public object TryExecuteScalar(SqlCommand sqlCommand, out SqlException sqlEx, out Exception ex)
        {
            return _container.TryExecuteScalar(sqlCommand, out sqlEx, out ex);
        }

        /// <summary>
        /// Trys to execute the query, and returns the first column of the first row in the result set returned by the query. Additional columns or rows are ignored
        /// </summary>
        /// <param name="sqlCommand">The SQL command that will be executed</param>
        /// <param name="sqlEx">The SQL exeption that is thrown if something goes wrong</param>
        /// <returns>The first column of the first row in the result, or null reference if the result set is empty. Returns a maximum of 2033 characters</returns>
        public object TryExecuteScalar(SqlCommand sqlCommand, out SqlException sqlEx)
        {
            return _container.TryExecuteScalar(sqlCommand, out sqlEx, out _);
        }

        /// <summary>
        /// Trys to execute the query, and returns the first column of the first row in the result set returned by the query. Additional columns or rows are ignored
        /// </summary>
        /// <param name="sqlCommand">The SQL command that will be executed</param>
        /// <param name="ex">The execption that is thrown if something goes wrong</param>
        /// <returns>The first column of the first row in the result, or null reference if the result set is empty. Returns a maximum of 2033 characters</returns>
        public object TryExecuteScalar(SqlCommand sqlCommand, out Exception ex)
        {
            return _container.TryExecuteScalar(sqlCommand, out _, out ex);
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
            App.GetInstance().GetService<DataModificationWindow>().ShowDialog(WindowStartupLocation.CenterScreen, false, _serverDataContainer);
        }

        private void ClearServer(object sender, RoutedEventArgs e)
        {
            if(MessageBox.Show("Are you sure you want to clear the server, this action can not be undone?", "Clear Server", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                _container.ExecuteNonQuery(new SQLCommandFactory().Delete().From("Coin").ToSQLCommand());

                UpdateTable();
            }
        }

        private void AddServerItem(object sender, RoutedEventArgs e)
        {
            App.GetInstance().GetService<DataModificationWindow>().ShowDialog(WindowStartupLocation.CenterScreen);
        }

        private void RemoveServerItem(object sender, RoutedEventArgs e)
        {
            if(MessageBox.Show($"Are you sure you want to delete {_dataRowView![1]} from the server", 
                $"Delete {_dataRowView![1]}", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                _container.ExecuteNonQuery(new SQLCommandFactory().Delete().From("Coin").Where("Name", _dataRowView![1]).ToSQLCommand());
                UpdateTable();
            }
        }

        /// <summary>
        /// Updates the coin table
        /// </summary>
        public void UpdateTable()
        {
            Coin_List.ItemsSource = _container.GetServerInfo().DefaultView;

            Clear.IsEnabled = Coin_List.Items.Count > 0;
            Menu_Clear.IsEnabled = Clear.IsEnabled;
        }

        private void Coin_List_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _dataRowView = (DataRowView)Coin_List.SelectedItem;

            Delete.IsEnabled = _dataRowView != null;
            Modify.IsEnabled = _dataRowView != null;

            _serverDataContainer = new(_dataRowView!);

            if(_dataRowView != null)
            {
                ID_Coin_Label.Content = _dataRowView[0];
                Name_Coin_Label.Content = _dataRowView[1];
                Description_Coin_Label.Content = _dataRowView[2];
                //TODO: Add date info
                Amount_Made_Coin_Label.Content = _dataRowView[3];
                Currency_Type_Coin_Label.Content = _dataRowView[4];
                Original_Value_Coin_Label.Content = _dataRowView[5];
                Retail_Value_Coin_Label.Content = _dataRowView[6];
                Image_Coin_Label.Content = _dataRowView[7];

                Image_Display.Source = Misc.CreateImageFromPath(Image_Coin_Label.Content.ToString()!);

                Image_Display.ToolTip = Image_Coin_Label.Content.ToString()!;
            }
            else
            {
                ID_Coin_Label.Content = "No data for coin ID";
                Name_Coin_Label.Content = "No data for coin name";
                Description_Coin_Label.Content = "No data for coin description";
                //TODO: Add date info
                Amount_Made_Coin_Label.Content = "No data for coin amount made";
                Currency_Type_Coin_Label.Content = "No data for coin currency type";
                Original_Value_Coin_Label.Content = "No data for coin original value";
                Retail_Value_Coin_Label.Content = "No data for coin retail value";
                Image_Coin_Label.Content = "No data for coin image";

                //TODO: Need to test
                Image_Display.Source = null;
                Image_Display.ToolTip = null;
            }
        }

        private void RFCF_Checked(object sender, RoutedEventArgs e)
        {
            if(sender is MenuItem menu)
            {
                if(menu.Name.Contains("RFCF"))
                {
                    CheckRFCF(menu.Name);

                    if(IsLoaded)
                    {
                        App.GetInstance().ConfigEditor.UpdateConfigFile();
                        App.GetInstance().ConfigWait.WaitOne();
                        App.GetInstance().Report.UpdateReportFrequency();
                    }
                }
            }
        }

        /// <summary>
        /// Logic for selecting the right frequency in the UI and unchecking the other frequency types
        /// </summary>
        /// <param name="menuName">Type of report frequency</param>
        private void CheckRFCF(string menuName)
        {
            switch(menuName)
            {
                case "RFCF_Daily":
                    App.GetInstance().ConfigEditor.Set("Report Frequency", "Daily", "Report Settings");
                    RFCF_Weekly.IsChecked = false;
                    RFCF_Monthly.IsChecked = false;
                    break;
                case "RFCF_Weekly":
                    App.GetInstance().ConfigEditor.Set("Report Frequency", "Weekly", "Report Settings");
                    RFCF_Daily.IsChecked = false;
                    RFCF_Monthly.IsChecked = false;
                    break;
                case "RFCF_Monthly":
                    App.GetInstance().ConfigEditor.Set("Report Frequency", "Monthly", "Report Settings");
                    RFCF_Daily.IsChecked = false;
                    RFCF_Weekly.IsChecked = false;
                    break;
            }
        }

        private void Enable_Reporting_Checked(object sender, RoutedEventArgs e)
        {
            Ensable_Reporting(true);
        }

        private void Enable_Reporting_Unchecked(object sender, RoutedEventArgs e)
        {
            Ensable_Reporting(false);
        }

        private void RFCF_Unchecked(object sender, RoutedEventArgs e)
        {
            if(RFCF_Daily.IsChecked == false && RFCF_Weekly.IsChecked == false && RFCF_Monthly.IsChecked == false)
            {
                RFCF_Daily.IsChecked = true;
            }
        }

        /// <summary>
        /// Enable or disables the reporting system
        /// </summary>
        /// <param name="enable">Should the reporting system be enabled</param>
        private void Ensable_Reporting(bool enable)
        {
            RFCF_Daily.IsEnabled = enable;
            RFCF_Weekly.IsEnabled = enable;
            RFCF_Monthly.IsEnabled = enable;

            if(IsLoaded)
            {
                App.GetInstance().ConfigEditor.Set("Enabled", enable, "Report Settings");
                App.GetInstance().ConfigEditor.UpdateConfigFile();
                App.GetInstance().ConfigWait.WaitOne();

                if (enable)
                {
                    App.GetInstance().Report.IsEnabled = enable;
                    App.GetInstance().Report.AddReport("Report is enabled", ReportSeverity.Warning);
                }
                else
                {
                    App.GetInstance().Report.AddReport("Report is disabled", ReportSeverity.Warning);
                    App.GetInstance().Report.IsEnabled = enable;
                }
            }
        }
    }
}