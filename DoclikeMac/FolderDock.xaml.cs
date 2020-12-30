using System;
using System.Windows;

namespace DoclikeMac
{
    /// <summary>
    /// Interaction logic for FolderDock.xaml
    /// </summary>
    public partial class FolderDock : Window
    {
        public static bool isClosed = true;

        public FolderDock()
        {
            InitializeComponent();
            isClosed = false;
        }

        /* Window（親クラス)のイベントをオーバーライド */

        /// <summary>
        /// 他アプリを表示した時
        /// </summary>
        /// <param name="e"></param>
        protected override void OnDeactivated(EventArgs e)
        {
            base.OnDeactivated(e);
            Hide();
        }

        private void Window_Activated(object sender, System.EventArgs e)
        {
            dockInfo.Text = string.Format("model__ 要素数：{0}, view__要素数: {1}, viewCD__要素数: {2}, counter: {3}", MainWindow.modelLen, MainWindow.viewLen, MainWindow.viewCDLen, MainWindow.counter);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            isClosed = true;
        }
    }
}