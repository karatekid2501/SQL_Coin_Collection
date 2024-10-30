using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CoinCollection
{
    public partial class AdvanceWindow : Window
    {
        public void Show(WindowStartupLocation wsl, bool topMost = false)
        {
            WindowStartupLocation = wsl;
            Topmost = topMost;
            Show();
        }
    }
}
