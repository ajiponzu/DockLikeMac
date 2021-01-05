using System.Windows;

namespace DocklikeMac
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private AppDock dock;

        public MainWindow()
        {
            InitializeComponent();
            Hide();
            dock = new AppDock();
            dock.Show();
        }

        /// <summary>
        /// タスクバーのボタンを押して，表示・非表示切替
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Menu1_Click(object sender, RoutedEventArgs e)
        {
            if (dock == null) return;
            if (dock.Visibility == Visibility.Hidden)
            {
                dock.Show();
            }
            else
            {
                dock.Hide();
            }
        }

        /// <summary>
        /// タスクバーのボタンその2を押してDockを閉じたり開いたり
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Menu2_Click(object sender, RoutedEventArgs e)
        {
            if (dock != null)
            {
                dock.End();
                dock.Close();
                dock = null;
            }
            else
            {
                dock = new AppDock();
                dock.Show();
            }
        }

        /// <summary>
        /// タスクバーのボタンその3を押してアプリケーション自体を終了
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Menu3_Click(object sender, RoutedEventArgs e)
        {
            if (dock != null) dock.End();
            Application.Current.Shutdown();
        }
    }
}