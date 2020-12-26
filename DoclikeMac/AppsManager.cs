using IWshRuntimeLibrary;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows;
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
            public string appPath;

            public AppData(string path)
            {
                appPath = path;
                var extension = Path.GetExtension(path);
                if (extension.Equals(".lnk"))
                {
                    handleShortcut();
                }
            }

            /// <summary>
            /// ショートカットからリンク先の実行ファイルパスを取得する
            /// </summary>
            private void handleShortcut()
            {
                var shell = new IWshRuntimeLibrary.WshShell();
                var sc = (IWshShortcut)shell.CreateShortcut(appPath);
                appPath = sc.TargetPath;
            }

            /// <summary>
            /// パスからアイコンを読み込む
            /// </summary>
            /// <returns>読み込んだアイコン画像</returns>
            public Image ReadIcon()
            {
                var appIcon = new Image();
                //winFormのアイコンをwpfのイメージに変換してからImage型のメンバに登録
                var icon = Icon.ExtractAssociatedIcon(appPath);
                appIcon.Source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(
                    icon.Handle,
                    new Int32Rect(0, 0, icon.Width, icon.Height),
                    BitmapSizeOptions.FromEmptyOptions());
                return appIcon;
            }
        }

        //アプリデータリスト
        private List<AppData> apps;

        public AppsManager()
        {
            apps = new List<AppData>();
            ///デバッグ用
            var pathList = Directory.GetFiles(@"./debugfolder/", "*");
            foreach (var path in pathList)
            {
                apps.Add(new AppData(path));
            }
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
            return apps[idx].ReadIcon();
        }

        /// <summary>
        /// appsの指定場所のAppDataに登録されたファイルを実行
        /// </summary>
        /// <param name="idx">appsの場所を指定</param>
        public void RunApp(ref int idx)
        {
            Process.Start(apps[idx].appPath);
        }
    }
}