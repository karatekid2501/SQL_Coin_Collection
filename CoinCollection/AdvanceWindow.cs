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
        public virtual void Show(WindowStartupLocation wsl, bool topMost = false)
        {
            WindowStartupLocation = wsl;
            Topmost = topMost;
            Show();
        }

        public virtual bool? ShowDialog(WindowStartupLocation wsl, bool topMost = false)
        {
            WindowStartupLocation = wsl;
            Topmost = topMost;
            return ShowDialog();
        }
    }
}
