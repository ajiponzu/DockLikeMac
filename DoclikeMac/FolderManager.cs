using System.Collections.Generic;
using System.IO;
using System.Windows.Controls;

namespace DocklikeMac
{
  internal class FolderManager
  {
    private static readonly string settingFolderFile = @"setting_folder.json";

    //フォルダリスト
    private readonly List<FolderData> folders;

    public FolderManager()
    {
      //登録フォルダの読み込み
      folders = new List<FolderData>();
      //設定ファイル読み込み，要素の追加
      if (!File.Exists(settingFolderFile)) return;

      var folderPathList = File.ReadAllLines(settingFolderFile);
      foreach (var path in folderPathList)
        folders.Add(new FolderData(path));
    }

    /// <summary>
    /// foldersの末尾に要素を追加, 限定的な同名フォルダ追加防止(結構ザル)
    /// </summary>
    /// <param name="path"></param>
    public bool InsertFolderData(string path)
    {
      foreach (var folder in folders)
      {
        if (path == folder.FolderPath)
          return false;
      }
      folders.Add(new FolderData(path));
      return true;
    }

    /// <summary>
    /// foldersの指定場所のFolderDataからアイコンを取得
    /// </summary>
    /// <param name="idx">foldersの場所を指定</param>
    /// <returns>指定場所のFolderData.folderPathに紐づいたボタン</returns>
    public Button GetFolderButton(int idx)
    {
      return folders[idx].button;
    }

    /// <summary>
    /// foldersの末尾のFolderDataからアイコンを取得
    /// </summary>
    /// <returns>foldersの末尾のアイコン: Button</returns>
    public Button GetFolderButton()
    {
      return folders[^1].button;
    }

    /// <summary>
    /// foldersの要素を削除
    /// </summary>
    public void RemoveFolderData()
    {
      foreach (var folder in folders)
      {
        if (folder.Name == FolderData.removeName)
        {
          folders.Remove(folder);
          return;
        }
      }
    }

    /// <summary>
    /// foldersの要素数を返す
    /// </summary>
    /// <returns>folders.Count()の値</returns>
    public int CountOfApps()
    {
      return folders.Count;
    }

    /// <summary>
    /// 設定ファイル書き込み
    /// </summary>
    public void WriteJson()
    {
      var pathList = new string[folders.Count];
      for (var idx = 0; idx < folders.Count; idx++)
        pathList[idx] = folders[idx].FolderPath;

      File.WriteAllLines(settingFolderFile, pathList);
    }
  }
}