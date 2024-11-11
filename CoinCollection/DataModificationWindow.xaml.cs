using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CoinCollection
{
    public enum DMWindowTitleName
    {
        New,
        Modify
    }

    /// <summary>
    /// Interaction logic for DataModificationWindow.xaml
    /// </summary>
    public partial class DataModificationWindow : AdvanceWindow
    {
        public DataModificationWindow()
        {
            InitializeComponent();
        }

        public virtual void Show(WindowStartupLocation wsl, bool topMost = false, DMWindowTitleName titleName = DMWindowTitleName.New)
        {
            Title = titleName.ToString();

            base.Show(wsl, topMost);
        }

        public virtual void ShowDialog(WindowStartupLocation wsl, bool topMost = false, DMWindowTitleName titleName = DMWindowTitleName.New)
        {
            Title = titleName.ToString();

            base.ShowDialog(wsl, topMost);
        }
    }
}
