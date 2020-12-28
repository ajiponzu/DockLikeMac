using IWshRuntimeLibrary;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
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

            //アイコンの拡大行列
            private static readonly ScaleTransform expand = new ScaleTransform(1, 1, 30, 30);

            //アイコンの縮小行列
            private static readonly ScaleTransform narrow = new ScaleTransform(0.6, 0.6, 30, 30);

            public AppData(string path)
            {
                appPath = path;
                var extension = Path.GetExtension(path);
                if (extension.Equals(".lnk"))
                {
                    HandleShortcut();
                }
                ReadIcon();
                RegisterIconEvent();
            }

            /// <summary>
            /// ショートカットからリンク先の実行ファイルパスを取得する
            /// </summary>
            private void HandleShortcut()
            {
                var shell = new IWshRuntimeLibrary.WshShell();
                var sc = (IWshShortcut)shell.CreateShortcut(appPath);
                appPath = sc.TargetPath;
            }

            /// <summary>
            /// パスからアイコンを読み込む
            /// </summary>
            public void ReadIcon()
            {
                iconImage = new Image();
                //winFormのアイコンをwpfのイメージに変換して保存
                var icon = Icon.ExtractAssociatedIcon(appPath);
                iconImage.Source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(
                    icon.Handle,
                    new Int32Rect(0, 0, icon.Width, icon.Height),
                    BitmapSizeOptions.FromEmptyOptions());
                iconImage.RenderTransform = narrow;
            }

            /// <summary>
            /// アイコンのイベントコールバックを動的に登録する
            /// </summary>
            private void RegisterIconEvent()
            {
                //クリック時にアプリを起動する
                iconImage.MouseLeftButtonDown += (sender, e) =>
                {
                    Process.Start(appPath);
                };

                //カーソルに触れるとアイコン拡大
                iconImage.MouseEnter += (sender, e) =>
                {
                    iconImage.RenderTransform = expand;
                };

                //カーソルが離れるとアイコン縮小
                iconImage.MouseLeave += (sender, e) =>
                {
                    iconImage.RenderTransform = narrow;
                };
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

        /// <summary>
        /// appsの指定場所にAppDataを登録
        /// </summary>
        /// <param name="idx">appsの場所を指定</param>
        /// <param name="path">AppData.appPathへ代入</param>
        public void InsertAppData(ref int idx, string path)
        {
            apps.Insert(idx, new AppData(path));
        }

        /// <summary>
        /// appsの指定場所のAppDataを削除
        /// </summary>
        /// <param name="idx"></param>
        public void RemoveAppData(ref int idx)
        {
            apps.RemoveAt(idx);
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
        /// appsの要素数を返す
        /// </summary>
        /// <returns>apps.Count()の値</returns>
        public int CountOfApps()
        {
            return apps.Count;
        }
    }
}