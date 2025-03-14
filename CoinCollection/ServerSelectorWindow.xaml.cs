// <copyright file="ServerSelectorWindow.xaml.cs" company="Karatekid2501">
// Copyright (c) Karatekid2501. All rights reserved.
// </copyright>

using System.Windows;

namespace CoinCollection
{
    /// <summary>
    /// Interaction logic for ServerSelectorWindow.xaml.
    /// </summary>
    public partial class ServerSelectorWindow : AdvanceWindow
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ServerSelectorWindow"/> class.
        /// </summary>
        public ServerSelectorWindow()
            : base(false, "!", "!")
        {
            InitializeComponent();
        }

        /// <summary>
        /// Gets a value indicating whether the user has closed the window by cancling.
        /// </summary>
        public bool IsCancled { get; private set; } = false;

        /// <summary>
        /// Closes the window.
        /// </summary>
        public void CloseWindow()
        {
            Close();
            App.GetInstance().GetService<MainWindow>().Close();
        }

        /// <inheritdoc/>
        protected override void Close_Click(object sender, RoutedEventArgs e)
        {
            IsCancled = true;
            CloseWindow();
        }

        private void ButtonNew(object sender, RoutedEventArgs e)
        {
            Misc.SaveFile.Check(this);
        }

        private void ButtonSelect(object sender, RoutedEventArgs e)
        {
            Misc.OpenFile.Check(this);
        }
    }
}
