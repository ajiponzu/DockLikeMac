using IWshRuntimeLibrary;
using System.Collections.Generic;
using System.IO;
using System.Windows.Controls;

namespace DoclikeMac
{
    //アプリデータリストの管理
    internal class AppsManager
    {
        //アプリデータ
        private class AppData
        {
            //実行ファイルのパス
            public string path;

            //アイコン画像
            public Image icon = null;

            /// <summary>
            ///
            /// </summary>
            /// <param name="path">ファイルパス</param>
            public AppData(string path)
            {
                this.path = path;
                var extension = Path.GetExtension(path);
                if (extension.Equals(".lnk"))
                {
                    handleShortcut();
                }
                ReadIcon();
            }

            private void handleShortcut()
            {
                var shell = new IWshRuntimeLibrary.WshShell();
                var sc = (IWshShortcut)shell.CreateShortcut(path);
                path = sc.TargetPath;
            }

            private void ReadIcon()
            {
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
    }
}