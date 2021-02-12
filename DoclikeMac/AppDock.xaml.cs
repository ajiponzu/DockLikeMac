using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace DocklikeMac
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class AppDock : Window
    {
        //登録アプリの管理メンバ
        private readonly AppsManager manager;

        //画面解像度(横)
        private double screenWidth;

        //画面解像度(縦)
        private double screenHeight;

        //最初のウィンドウ幅
        private double startWidth;

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

        //ウィンドウアニメーションフラグ
        private bool animationFlag = false;

        //ウィンドウ自動非表示フラグ
        private bool isUnlock = false;

        //lockButtonテキスト1
        private const string txtAtUnlock = "lock";

        //lockButtonテキスト2
        private const string txtAtLock = "unlock";

        //openボタンで表示するウィンドウ
        private FolderDock fdWindow = null;

        //編集モード
        public static bool isEdit = false;

        //editButtonテキスト1
        private const string txtAtExe = "edit";

        //editButtonテキスト2
        private const string txtAtEdit = "exe";

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public AppDock()
        {
            manager = new AppsManager();
            fdWindow = new FolderDock();
            InitializeComponent();
            InitWindow();
            InitIconList();
        }

        /// <summary>
        /// ウィンドウに関する初期処理
        /// </summary>
        private void InitWindow()
        {
            startWidth = Width;
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
                iconList.Children.Add(manager.GetAppIcon(idx));
            }
        }

        /// <summary>
        /// ウィンドウアニメーション生成(上下に動く)
        /// </summary>
        /// <returns>t: DispatcherTimer</returns>
        private DispatcherTimer CreateWindowMoveTimer()
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

        public void End()
        {
            manager.WriteJson();
        }

        /* Window（親クラス)のイベントをオーバーライド */

        /// <summary>
        /// 他アプリを表示した時
        /// </summary>
        /// <param name="e"></param>
        protected override void OnDeactivated(EventArgs e)
        {
            base.OnDeactivated(e);
            if (isUnlock)
            {
                Top = screenHeight;
                grid.Opacity = 0;
                animationFlag = true;
                lockButton.Content = txtAtUnlock;
            }
            else
            {
                Top = minY;
                grid.Opacity = 1;
                animationFlag = false;
                lockButton.Content = txtAtLock;
            }
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
            timer = CreateWindowMoveTimer();
            timer.Start();
            GC.Collect();
        }

        /// <summary>
        /// ウィンドウを引っ張ってくる
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_MouseEnter(object sender, MouseEventArgs e)
        {
            if (animationFlag)
            {
                delta = deltaAbs;
                grid.Opacity = 1;
                AnimationWindow();
            }
        }

        /// <summary>
        /// ウィンドウを画面外にしまう
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_MouseLeave(object sender, MouseEventArgs e)
        {
            if (animationFlag)
            {
                delta = -deltaAbs;
                Thread.Sleep(320);
                AnimationWindow();
            }
        }

        /// <summary>
        /// ウィンドウ固定・非固定モード切替．アニメーションモードと対応．ウィンドウ非固定モードのときはアプリ実行モードにする．
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LockButton_Click(object sender, RoutedEventArgs e)
        {
            //アニメーションモードのとき
            if (animationFlag)
            {
                //アニメーションをオフにし，ウィンドウを固定
                isUnlock = animationFlag = false;
                lockButton.Content = txtAtLock;
                Top = minY;
            }
            else
            {
                //アニメーションをオンにする
                isUnlock = animationFlag = true;
                lockButton.Content = txtAtUnlock;
                isEdit = false;
                editButton.Content = txtAtExe;
            }
        }

        /// <summary>
        /// フォルダーランチャーを開く
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            fdWindow = new FolderDock();
            fdWindow.Top = (screenHeight - fdWindow.Height) / 2;
            fdWindow.Left = screenWidth - fdWindow.Width;
            fdWindow.Show();
        }

        /// <summary>
        /// ドラッグ時のカーソル横のアイコンを変更
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_DragOver(object sender, DragEventArgs e)
        {
            if (isEdit)
            {
                if (e.Data.GetDataPresent(DataFormats.FileDrop, true))
                {
                    e.Effects = DragDropEffects.Copy;
                }
                else
                {
                    e.Effects = DragDropEffects.None;
                }
                e.Handled = true;
            }
        }

        /// <summary>
        /// アプリを追加したときの処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Drop(object sender, DragEventArgs e)
        {
            if (isEdit)
            {
                iconList.Children.RemoveAt(iconList.Children.Count - 1);
                iconList.ColumnDefinitions.RemoveAt(iconList.ColumnDefinitions.Count - 1);
                if (e.Data.GetData(DataFormats.FileDrop) is not string[] dropFiles) return;
                if (dropFiles[0].EndsWith(".exe") || dropFiles[0].EndsWith(".lnk"))
                {
                    Width += startWidth;
                    Left = (screenWidth - Width) / 2;
                    //同じパスのファイルがない場合のみ追加する
                    if (manager.InsertAppData(dropFiles[0]))
                    {
                        iconList.ColumnDefinitions.Add(new ColumnDefinition());
                        iconList.Children.Add(manager.GetAppIcon());
                        manager.WriteJson();
                    }
                }
            }
        }

        /// <summary>
        /// アプリを追加しようとするときの処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_DragEnter(object sender, DragEventArgs e)
        {
            if (isEdit)
            {
                iconList.ColumnDefinitions.Add(new ColumnDefinition());
                iconList.Children.Add(new Image());
            }
        }

        /// <summary>
        /// アプリを追加しなかったときの処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_DragLeave(object sender, DragEventArgs e)
        {
            if (isEdit)
            {
                iconList.Children.RemoveAt(iconList.Children.Count - 1);
                iconList.ColumnDefinitions.RemoveAt(iconList.ColumnDefinitions.Count - 1);
            }
        }

        /// <summary>
        /// 編集モードオンオフ，編集モード時は，必ずウィンドウ固定モードになる．
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            //編集モードのとき
            if (isEdit)
            {
                //アプリ実行モードへ
                isEdit = false;
                editButton.Content = txtAtExe;
            }
            else
            {
                //編集モードへ
                isEdit = true;
                editButton.Content = txtAtEdit;
                isUnlock = animationFlag = false;
                lockButton.Content = txtAtLock;
                Top = minY;
            }
        }

        /// <summary>
        /// アイコン移動を検知し，データのインデックスを入れ替える．
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void IconList_MouseMove(object sender, MouseEventArgs e)
        {
            if (AppData.isMovingIcon)
            {
                if (e.GetPosition(iconList).Y < 0)
                {
                    AppData.isDeleted = true;
                    return;
                }
                if (AppData.isChangedLeft)
                {
                    //左と入れ替え
                    AppData.isChangedLeft = false;
                    var idx = manager.GetIndexByImage(AppData.movingImage);
                    if (idx - 1 < 0)
                    {
                        //iconListからはみ出たら削除
                        AppData.isDeleted = true;
                        return;
                    }
                    manager.Swap(idx, idx - 1);
                }
                if (AppData.isChangedRight)
                {
                    //右と入れ替え
                    AppData.isChangedRight = false;
                    var idx = manager.GetIndexByImage(AppData.movingImage);
                    if (idx + 1 >= manager.CountOfApps())
                    {
                        //iconListからはみ出たら削除
                        AppData.isDeleted = true;
                        return;
                    }
                    manager.Swap(idx, idx + 1);
                }
                AppData.isDeleted = false;
            }
        }

        /// <summary>
        /// アイコンの削除
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void IconList_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (isEdit)
            {
                if (AppData.isDeleted)
                {
                    var idx = manager.GetIndexByImage(AppData.movingImage);
                    iconList.Children.Remove(AppData.movingImage);
                    manager.RemoveAppData(idx);
                    manager.FixGridPosition(idx);
                    iconList.ColumnDefinitions.RemoveAt(idx);
                    AppData.movingImage = null;
                    Width -= startWidth;
                    Left = (screenWidth - Width) / 2;
                    AppData.isDeleted = false;
                }
                manager.WriteJson();
            }
        }
    }
}