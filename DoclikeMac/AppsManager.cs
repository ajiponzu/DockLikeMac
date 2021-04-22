using System.Collections.Generic;
using System.IO;
using System.Windows.Controls;
using Image = System.Windows.Controls.Image;

namespace DocklikeMac
{
    //アプリデータリストの管理
    internal class AppsManager
    {
        private static readonly string settingFile = @"setting.json";

        //アプリデータリスト
        private readonly List<AppData> apps;

        public AppData this[int idx]
        {
            get
            {
                return apps[idx];
            }
        }

        public AppsManager()
        {
            //登録アプリの読み込み
            apps = new List<AppData>();
            //設定ファイル読み込み，要素の追加
            if (!File.Exists(settingFile)) return;
            var pathList = File.ReadAllLines(settingFile);
            foreach (var path in pathList)
                apps.Add(new AppData(path));
        }

        /// <summary>
        /// appsの末尾に要素を追加, 限定的な同名ファイル追加防止(結構ザル)
        /// </summary>
        /// <param name="path"></param>
        public bool InsertAppData(string path)
        {
            foreach (var app in apps)
            {
                if (path == app.AppPath)
                    return false;
            }
            apps.Add(new AppData(path));
            return true;
        }

        /// <summary>
        /// appsの末尾要素を削除
        /// </summary>
        public void RemoveAppData()
        {
            apps.RemoveAt(apps.Count - 1);
        }

        /// <summary>
        /// appsの要素を削除
        /// </summary>
        /// <param name="idx">削除対象のインデックス</param>
        public void RemoveAppData(int idx)
        {
            apps.RemoveAt(idx);
        }

        /// <summary>
        /// appsの指定場所のAppDataからアイコンを取得
        /// </summary>
        /// <param name="idx">appsの場所を指定</param>
        /// <returns>指定場所のAppData.appPathに紐づいたアイコン画像</returns>
        public Image GetAppIcon(int idx)
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

        /// <summary>
        /// データの位置を入れ替え(配置場所も入れ替え)
        /// </summary>
        /// <param name="idx1"></param>
        /// <param name="idx2"></param>
        public void Swap(int idx1, int idx2)
        {
            apps[idx1].iconImage.SetValue(Grid.ColumnProperty, idx2);
            apps[idx2].iconImage.SetValue(Grid.ColumnProperty, idx1);
            var temp = apps[idx1];
            apps[idx1] = apps[idx2];
            apps[idx2] = temp;
        }

        /// <summary>
        /// 画像データの参照から，インデックスを特定
        /// </summary>
        /// <param name="image">画像データへの参照</param>
        /// <returns>idx: 画像データに対応するインデックス</returns>
        public int GetIndexByImage(Image image)
        {
            int idx = -1;
            foreach (var app in apps)
            {
                if (image == app.iconImage)
                {
                    idx = apps.IndexOf(app);
                    break;
                }
            }
            return idx;
        }

        /// <summary>
        /// Gridにおける位置を再設定
        /// </summary>
        /// <param name="idx"></param>
        public void FixGridPosition(int idx)
        {
            for (; idx < apps.Count; idx++)
                apps[idx].iconImage.SetValue(Grid.ColumnProperty, idx);
        }

        /// <summary>
        /// 設定ファイル書き込み
        /// </summary>
        public void WriteJson()
        {
            var pathList = new string[apps.Count];
            for (var idx = 0; idx < apps.Count; idx++)
                pathList[idx] = apps[idx].AppPath;

            File.WriteAllLines(settingFile, pathList);
        }
    }
}