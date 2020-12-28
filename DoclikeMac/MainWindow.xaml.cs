using System;
using System.Windows;
using System.Windows.Controls;
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
        private readonly AppsManager manager;

        //画面解像度(横)
        private double screenWidth;

        //画面解像度(縦)
        private double screenHeight;

        //調整高さ幅
        private const int pad = 1;

        //ウィンドウy座標最小値
        private double minY;

        //アニメーションタイマー
        private DispatcherTimer timer = null;

        //ウィンドウ表示・非表示の1フレームあたりの秒数
        public readonly int spf = 16;

        //ウィンドウの1フレームあたりの移動距離
        private float delta = 4.3f;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MainWindow()
        {
            manager = new AppsManager();
            InitializeComponent();
            InitWindow();
            InitIconList();
            AnimationWindow();
        }

        /// <summary>
        /// ウィンドウに関する初期処理
        /// </summary>
        private void InitWindow()
        {
            /* ウィンドウの幅はアイコンの数に比例する */
            var count = manager.CountOfApps();
            Width *= (count <= 0) ? 1 : count;
            /* 画面中央下に配置 */
            screenWidth = SystemParameters.PrimaryScreenWidth;
            screenHeight = SystemParameters.PrimaryScreenHeight - pad;
            Left = (screenWidth - Width) / 2;
            minY = Top = screenHeight - Height + pad * 2;
        }

        /// <summary>
        /// アイコンリスト(grid)の初期処理
        /// </summary>
        private void InitIconList()
        {
            for (var idx = 0; idx < manager.CountOfApps(); idx++)
            {
                iconList.ColumnDefinitions.Add(new ColumnDefinition());
                iconList.Children.Add(manager.GetAppIcon(ref idx));
            }
        }

        /// <summary>
        /// updateスレッド生成
        /// </summary>
        /// <returns>t: DispatcherTimer</returns>
        private DispatcherTimer CreateTimer()
        {
            var t = new DispatcherTimer(DispatcherPriority.Render)
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
                    iconList.Opacity = 0;
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
            minY = Top = screenHeight - Height + pad * 2;
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
            iconList.Opacity = 1;
            AnimationWindow();
        }

        private void Window_MouseLeave(object sender, MouseEventArgs e)
        {
            AnimationWindow();
        }
    }
}