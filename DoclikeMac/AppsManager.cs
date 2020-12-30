using IWshRuntimeLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Image = System.Windows.Controls.Image;

namespace DoclikeMac
{
    //アプリデータリストの管理
    internal class AppsManager
    {
        //アプリデータ
        private class AppData
        {
            //実行ファイルのパス
            private string appPath;

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
                //クリック時にアプリを起動する
                iconImage.MouseLeftButtonDown += (sender, e) =>
                {
                    if (MainWindow.isEdit) return;
                    Process.Start(appPath);
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
                        tempScaleMat.ScaleX = expand.ScaleX;
                        tempScaleMat.ScaleY = expand.ScaleY;
                        t.Stop();
                    }
                    else if (tempScaleMat.ScaleX < narrow.ScaleX && delta < 0)
                    {
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
        }

        private const string folder = @"./debugfolder/"; //デバッグ用

        //アプリデータリスト
        private readonly List<AppData> apps;

        public AppsManager()
        {
            apps = new List<AppData>();
            var _pathList = GetPathList();
            var patterns = new string[] { ".exe", ".lnk" };
            var pathList = _pathList.Where(_path => patterns.Any(_pattern => _path.ToLower().EndsWith(_pattern)));
            foreach (var path in pathList)
            {
                apps.Add(new AppData(path));
            }
        }

        /// <summary>
        /// 外部アプリのパスを取得
        /// </summary>
        /// <returns>パスを要素とするstring配列</returns>
        private static string[] GetPathList()
        {
            var pathList = Directory.GetFiles(folder, "*.*");
            return pathList;
        }

        public void InsertAppData(string path)
        {
            apps.Add(new AppData(path));
        }

        /// <summary>
        /// appsの末尾要素を削除
        /// </summary>
        public void RemoveAppData()
        {
            apps.RemoveAt(apps.Count - 1);
        }

        /// <summary>
        /// appsの指定場所のAppDataからアイコンを取得
        /// </summary>
        /// <param name="idx">appsの場所を指定</param>
        /// <returns>指定場所のAppData.appPathに紐づいたアイコン画像</returns>
        public Image GetAppIcon(ref int idx)
        {
            apps[idx].iconImage.SetValue(Grid.RowProperty, 0);
            apps[idx].iconImage.SetValue(Grid.ColumnProperty, idx);
            return apps[idx].iconImage;
        }

        /// <summary>
        /// appsの末尾のAppDataからアイコンを取得
        /// </summary>
        /// <returns>appsの末尾のアイコン: Image</returns>
        public Image GetAppIcon()
        {
            apps[^1].iconImage.SetValue(Grid.RowProperty, 0);
            apps[^1].iconImage.SetValue(Grid.ColumnProperty, apps.Count - 1);
            return apps[^1].iconImage;
        }

        /// <summary>
        /// appsの要素数を返す
        /// </summary>
        /// <returns>apps.Count()の値</returns>
        public int CountOfApps()
        {
            return apps.Count;
        }
    }
}