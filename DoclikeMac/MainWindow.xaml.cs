using System;
using System.Windows;

namespace DoclikeMac
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private AppDock dock;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MainWindow()
        {
            Hide();
            dock = new AppDock();
            dock.Show();
        }
    }
}