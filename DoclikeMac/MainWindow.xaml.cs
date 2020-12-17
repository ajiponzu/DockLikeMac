using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace DoclikeMac
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //画面解像度(横)
        private double screenWidth;

        //画面解像度(縦)
        private double screenHeight;

        //ウィンドウの表示・非表示アニメーションのためのメンバ
        private DispatcherTimer timer = null;

        //ウィンドウ表示・非表示スピード
        private readonly int spf = 33;
        private float delta = -4f;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            /* 画面中央下に配置 */
            screenWidth = SystemParameters.PrimaryScreenWidth;
            screenHeight = SystemParameters.PrimaryScreenHeight;
            Left = (screenWidth - Width) / 2;
            Top = screenHeight - 1;
            Opacity = 0;
        }

        /// <summary>
        /// updateスレッド生成
        /// </summary>
        /// <returns>t: DispatcherTimer</returns>
        private DispatcherTimer CreateTimer()
        {
            var t = new DispatcherTimer(DispatcherPriority.SystemIdle)
            {
                Interval = TimeSpan.FromMilliseconds(spf),
            };

            t.Tick += (sender, e) =>
            {
                Top -= delta;
                //所定の位置まで動かして止める
                if (Top <= screenHeight - Height && delta > 0)
                {
                    Top = screenHeight - Height - 1;
                    t.Stop();
                }
                else if (Top >= screenHeight && delta < 0)
                {
                    Top = screenHeight - 1;
                    Opacity = 0;
                    t.Stop();
                }
            };

            return t;
        }

        /// <summary>
        /// dpiが変更された時のウィンドウ再配置メソッド
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_DpiChanged(object sender, DpiChangedEventArgs e)
        {
            screenWidth = SystemParameters.PrimaryScreenWidth;
            screenHeight = SystemParameters.PrimaryScreenHeight;
            Left = (screenWidth - Width) / 2;
            Top = screenHeight - 1;
        }

        /// <summary>
        /// 画面下にカーソルを持ってきたときに起動するメソッド
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_MouseEnter(object sender, MouseEventArgs e)
        {
            delta *= -1;
            Opacity = 100;
            if (timer != null) timer.Stop();
            timer = CreateTimer();
            timer.Start();
            GC.Collect();
        }

        /// <summary>
        /// ウィンドウからカーソルが離れたときに起動するメソッド
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_MouseLeave(object sender, MouseEventArgs e)
        {
            delta *= -1;
            if (timer != null) timer.Stop();
            timer = CreateTimer();
            timer.Start();
            GC.Collect();
        }
    }
}