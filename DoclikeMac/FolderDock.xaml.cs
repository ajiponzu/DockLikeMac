using System.Windows;

namespace DoclikeMac
{
    /// <summary>
    /// Interaction logic for FolderDock.xaml
    /// </summary>
    public partial class FolderDock : Window
    {
        public FolderDock()
        {
            InitializeComponent();
        }

        private void Window_Activated(object sender, System.EventArgs e)
        {
            dockInfo.Text = string.Format("model__ 要素数：{0}, view__要素数: {1}, viewCD__要素数: {2}, counter: {3}", MainWindow.modelLen, MainWindow.viewLen, MainWindow.viewCDLen, MainWindow.counter);
        }
    }
}
