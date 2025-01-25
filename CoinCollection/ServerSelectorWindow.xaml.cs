using System.Windows;

namespace CoinCollection
{
    /// <summary>
    /// Interaction logic for ServerSelectorWindow.xaml
    /// </summary>
    public partial class ServerSelectorWindow : AdvanceWindow
    {
        public bool IsCancled { get; private set; } = false;

        public ServerSelectorWindow() : base(false, "!", "!")
        {
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
            Misc.FolderDialog.Check(this);
        }

        private void ButtonSelect(object sender, RoutedEventArgs e)
        {
            Misc.FileDialog.Check(this);
        }
    }
}
