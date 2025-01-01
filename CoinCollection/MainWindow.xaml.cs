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
using System.Data;
using Microsoft.Data.SqlClient;

namespace CoinCollection
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : AdvanceWindow
    {
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

        public bool CheckMoneyNameExists(string currencyName)
        {
            if(string.IsNullOrEmpty(currencyName))
            {
                return false;
            }

            return _container.CheckMoneyNameExists(currencyName);
        }

        public bool SubmitNew(ServerDataContainer newData, out Exception e)
        {
            bool done = _container.SubmitNew(newData, out e);

            if (done)
            {
                UpdateTable();
            }

            return done;
        }

        public bool SubmitAltered(ServerDataContainer modifiedData)
        {
            bool done = _container.SubmitAltered(modifiedData);

            if (done)
            {
                UpdateTable();
            }

            return done;
        }

        public object ExecuteScalar(SqlCommand sqlCommand)
        {
            return _container.ExecuteScalar(sqlCommand);
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
            App.GetInstance().GetService<DataModificationWindow>().ShowDialog(WindowStartupLocation.CenterScreen, true, _serverDataContainer);
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
            App.GetInstance().GetService<DataModificationWindow>().ShowDialog(WindowStartupLocation.CenterScreen, true);
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

        private void UpdateTable()
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

                /*string imageName = Image_Coin_Label.Content.ToString()!;

                BitmapImage bitmapImage = new();
                bitmapImage.BeginInit();
                bitmapImage.UriSource = new(Path.Combine(Path.Combine(Directory.GetCurrentDirectory(), "Images"), imageName), UriKind.RelativeOrAbsolute);
                bitmapImage.EndInit();*/

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
    }
}