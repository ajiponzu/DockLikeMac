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
        //登録アプリの管理メンバ
        private AppsManager manager;

        //画面解像度(横)
        private double screenWidth;

        //画面解像度(縦)
        private double screenHeight;

        //調整高さ幅
        private const int pad = 1;

        //ウィンドウy座標最小値
        private double minY;

        //ウィンドウの表示・非表示アニメーションのためのメンバ
        private DispatcherTimer timer = null;

        //ウィンドウ表示・非表示スピード
        private readonly int spf = 16;

        private float delta = 4f;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MainWindow()
        {
            manager = new AppsManager();
            InitializeComponent();
            initView();
        }

        /// <summary>
        /// 画面の初期化
        /// </summary>
        private void initView()
        {
            /* 画面中央下に配置 */
            screenWidth = SystemParameters.PrimaryScreenWidth;
            screenHeight = SystemParameters.PrimaryScreenHeight - pad;
            Left = (screenWidth - Width) / 2;
            minY = Top = screenHeight - Height + pad * 2;
            AnimationWindow();
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
                if (Top < minY && delta > 0)
                {
                    Top = minY;
                    t.Stop();
                }
                else if (Top > screenHeight && delta < 0)
                {
                    Top = screenHeight;
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
            screenHeight = SystemParameters.PrimaryScreenHeight - pad;
            Left = (screenWidth - Width) / 2;
            Top = screenHeight;
            minY = screenHeight - Height + pad;
        }

        /// <summary>
        /// ウィンドウを動かす処理
        /// </summary>
        private void AnimationWindow()
        {
            delta *= -1;
            if (timer != null) timer.Stop();
            timer = CreateTimer();
            timer.Start();
            GC.Collect();
        }

        private void Window_MouseEnter(object sender, MouseEventArgs e)
        {
            AnimationWindow();
        }

        private void Window_MouseLeave(object sender, MouseEventArgs e)
        {
            AnimationWindow();
        }
    }
}