using IWshRuntimeLibrary;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Image = System.Windows.Controls.Image;
using Point = System.Windows.Point;

namespace DoclikeMac
{
    //アプリデータ
    internal class AppData
    {
        //実行ファイルのパス
        private string appPath;

        public string AppPath
        {
            get
            {
                return appPath;
            }
        }

        //実行ファイルのアイコン
        public Image iconImage;

        //アニメーションタイマー
        private DispatcherTimer timer = null;

        //アイコンの拡大行列
        private static readonly ScaleTransform expand = new ScaleTransform(1.36, 1.36, 40, 40);

        //アイコンの縮小行列
        private static readonly ScaleTransform narrow = new ScaleTransform(0.8, 0.8, 40, 40);

        //アイコンの伸縮のための行列
        private readonly ScaleTransform tempScaleMat = new ScaleTransform(0.8, 0.8, 40, 40);

        //アイコンの伸縮の1フレームあたりの秒数
        private static readonly int spf = 16;

        //アイコンの1フレームあたりの拡大スピード
        private static readonly float deltaExpand = 0.20f;

        //アイコンの1フレームあたりの縮小スピード
        private static readonly float deltaNarrow = -0.06f;

        //アイコンの1フレームあたりの伸縮
        private float delta;

        //定位置
        private Point staticPos;

        //現在位置
        private Point curPos;

        //隣接アイコンとの溝
        private const double pad = 10;

        //移動中true
        public static bool isMovingIcon = false;

        //左のアイコンと入れ替えたときtrue
        public static bool isChangedLeft = false;

        //右のアイコンと入れ替えたときtrue
        public static bool isChangedRight = false;

        //アイコンを削除するときtrue
        public static bool isDeleted = false;

        //移動中アイコン画像
        public static Image movingImage = null;

        public AppData()
        {
        }

        public AppData(string path)
        {
            appPath = path;
            var extension = Path.GetExtension(path);
            if (extension.Equals(".lnk"))
            {
                HandleShortcut();
            }
            iconImage = new Image();
            if (appPath == "") return;
            ReadIcon();
            RegisterIconEvent();
        }

        /// <summary>
        /// ショートカットからリンク先の実行ファイルパスを取得する
        /// </summary>
        private void HandleShortcut()
        {
            var shell = new WshShell();
            var sc = (IWshShortcut)shell.CreateShortcut(appPath);
            appPath = sc.TargetPath;
        }

        /// <summary>
        /// パスからアイコンを読み込む
        /// </summary>
        public void ReadIcon()
        {
            //winFormのアイコンをwpfのイメージに変換して保存
            var icon = Icon.ExtractAssociatedIcon(appPath);
            iconImage.Source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(
                icon.Handle,
                new Int32Rect(0, 0, icon.Width, icon.Height),
                BitmapSizeOptions.FromEmptyOptions());
            iconImage.RenderTransform = tempScaleMat;
        }

        /// <summary>
        /// アイコンのイベントコールバックを動的に登録する
        /// </summary>
        private void RegisterIconEvent()
        {
            //クリック時
            iconImage.MouseLeftButtonDown += (sender, e) =>
            {
                if (MainWindow.isEdit)
                {
                    //アイコンを動かす準備
                    iconImage.Opacity = 0.5;
                    staticPos = e.GetPosition((IInputElement)iconImage.Parent);
                    curPos = e.GetPosition((IInputElement)iconImage.Parent);
                    iconImage.CaptureMouse();
                    isMovingIcon = true;
                    movingImage = iconImage;
                }
                else
                {
                    //アプリを起動する
                    Process.Start(appPath);
                }
            };

            //カーソルに触れるとアイコン拡大
            iconImage.MouseEnter += (sender, e) =>
            {
                if (MainWindow.isEdit) return;
                delta = deltaExpand;
                AnimationIcon();
            };

            //カーソルが離れるとアイコン縮小
            iconImage.MouseLeave += (sender, e) =>
            {
                if (MainWindow.isEdit) return;
                delta = deltaNarrow;
                AnimationIcon();
            };

            //アイテムをアイコン上にドラッグすると，アイテムのパスをコピーしてとっておく
            iconImage.DragOver += (sender, e) =>
            {
                if (MainWindow.isEdit) return;
                if (e.Data.GetDataPresent(DataFormats.FileDrop, true))
                {
                    e.Effects = DragDropEffects.Copy;
                }
                else
                {
                    e.Effects = DragDropEffects.None;
                }
                e.Handled = true;
            };

            //アイテムをドロップすると，ドラッグ時に保存したパスを引数としてアプリ実行
            iconImage.AllowDrop = true;
            iconImage.Drop += (sender, e) =>
            {
                if (MainWindow.isEdit) return;
                if (e.Data.GetData(DataFormats.FileDrop) is not string[] dropFiles) return;
                Process.Start(appPath, dropFiles[0]);
            };

            //アイコンを動かす
            iconImage.MouseMove += (sender, e) =>
            {
                if (MainWindow.isEdit && iconImage.IsMouseCaptured)
                {
                    var cursor = e.GetPosition((IInputElement)iconImage.Parent);
                    MoveIcon(curPos - cursor);
                    curPos = cursor;
                    if (isDeleted) return;
                    CheckSwapIcon();
                }
            };

            //アイコンの位置確定
            iconImage.MouseLeftButtonUp += (sender, e) =>
            {
                if (MainWindow.isEdit)
                {
                    iconImage.Opacity = 1;
                    iconImage.ReleaseMouseCapture();
                    isMovingIcon = false;
                    //アイコン位置を元に戻す
                    MoveIcon(curPos - staticPos);
                    iconImage.RenderTransform = tempScaleMat;
                }
            };
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
                tempScaleMat.ScaleX += delta;
                tempScaleMat.ScaleY += delta;
                //規定サイズを超えると終了
                if (tempScaleMat.ScaleX > expand.ScaleX && delta > 0)
                {
                    //拡大
                    tempScaleMat.ScaleX = expand.ScaleX;
                    tempScaleMat.ScaleY = expand.ScaleY;
                    t.Stop();
                }
                else if (tempScaleMat.ScaleX < narrow.ScaleX && delta < 0)
                {
                    //縮小
                    tempScaleMat.ScaleX = narrow.ScaleX;
                    tempScaleMat.ScaleY = narrow.ScaleY;
                    t.Stop();
                }
            };

            return t;
        }

        /// <summary>
        /// アイコンの伸縮アニメーション
        /// </summary>
        private void AnimationIcon()
        {
            if (timer != null) timer.Stop();
            timer = CreateTimer();
            timer.Start();
        }

        /// <summary>
        /// アイコンの移動．一時的に拡大縮小行列の参照が外れ, 平行移動行列で上書きされることに注意
        /// </summary>
        /// <param name="v"></param>
        private void MoveIcon(Vector v)
        {
            var matrix = iconImage.RenderTransform.Value;
            matrix.Translate(-v.X, -v.Y);
            iconImage.RenderTransform = new MatrixTransform(matrix);
        }

        /// <summary>
        /// アイコンを入れ替えるかどうかチェックする
        /// </summary>
        private void CheckSwapIcon()
        {
            var boundaryWid = iconImage.RenderSize.Width + pad;
            var diff = curPos - staticPos;
            if (diff.X > boundaryWid)
            {
                //右のアイコンと入れ替え
                isChangedRight = true;
                MoveIcon(curPos - staticPos);
                staticPos = curPos;
            }
            if (diff.X < -boundaryWid)
            {
                //左のアイコンと入れ替え
                isChangedLeft = true;
                MoveIcon(curPos - staticPos);
                staticPos = curPos;
            }
        }
    }
}