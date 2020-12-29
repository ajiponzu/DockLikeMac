using System;
using System.Threading;
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

        //ウィンドウの1フレームあたりの移動距離の絶対値
        private readonly float deltaAbs = 4.3f;

        //ウィンドウの1フレームあたりの移動距離
        private float delta;

        private static bool animationFlag = false;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MainWindow()
        {
            manager = new AppsManager();
            InitializeComponent();
            InitWindow();
            InitIconList();
        }

        /// <summary>
        /// ウィンドウに関する初期処理
        /// </summary>
        private void InitWindow()
        {
            /* ウィンドウの幅はアイコンの数に比例する */
            var count = manager.CountOfApps();
            Width *= (count <= 0) ? 1 : count;
            SetWindowInitPos();
        }

        /// <summary>
        /// ウィンドウの初期位置を設定
        /// </summary>
        private void SetWindowInitPos()
        {
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
                    grid.Opacity = 0;
                    t.Stop();
                }
            };

            return t;
        }

        /* Window（親クラス)のイベントをオーバーライド */

        /// <summary>
        /// ウィンドウを選択したとき(曖昧)
        /// </summary>
        /// <param name="e"></param>
        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            Top = minY;
            grid.Opacity = 1;
            animationFlag = false;
        }

        /// <summary>
        /// 他アプリを表示した時
        /// </summary>
        /// <param name="e"></param>
        protected override void OnDeactivated(EventArgs e)
        {
            base.OnDeactivated(e);
            Top = screenHeight;
            grid.Opacity = 0;
            animationFlag = true;
        }

        /* xamlイベント */

        /// <summary>
        /// dpiが変更された時のウィンドウ再配置メソッド
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_DpiChanged(object sender, DpiChangedEventArgs e)
        {
            SetWindowInitPos();
        }

        /// <summary>
        /// ウィンドウを動かす処理
        /// </summary>
        private void AnimationWindow()
        {
            if (timer != null) timer.Stop();
            timer = CreateTimer();
            timer.Start();
            GC.Collect();
        }

        private void Window_MouseEnter(object sender, MouseEventArgs e)
        {
            if (animationFlag)
            {
                delta = deltaAbs;
                grid.Opacity = 1;
                AnimationWindow();
            }
        }

        private void Window_MouseLeave(object sender, MouseEventArgs e)
        {
            if (animationFlag)
            {
                delta = -deltaAbs;
                Thread.Sleep(320);
                AnimationWindow();
            }
        }
    }
}