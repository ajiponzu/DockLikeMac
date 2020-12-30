using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using Image = System.Windows.Controls.Image;

namespace DoclikeMac
{
    //アプリデータリストの管理
    class AppsManager
    {
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
        /// appsの末尾に要素を追加
        /// </summary>
        /// <param name="path"></param>
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
    }
}