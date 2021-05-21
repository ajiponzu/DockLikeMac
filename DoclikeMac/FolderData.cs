using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DocklikeMac
{
  internal class FolderData
  {
    //フォルダのパス
    private readonly string folderPath;

    public string FolderPath
    {
      get
      {
        return folderPath;
      }
    }

    //フォルダ名
    private readonly string name;

    public string Name
    {
      get
      {
        return name;
      }
    }

    public Button button;

    //フォルダーボタンの色
    private static readonly SolidColorBrush brushFolder = new SolidColorBrush(Color.FromArgb(0x8c, 0xff, 0xed, 0x55));

    //フォルダー以外を開くボタンの色
    private static readonly SolidColorBrush brushFile = new SolidColorBrush(Color.FromArgb(0x8c, 0x55, 0xed, 0xff));

    //削除対象のボタン
    public static Button removeButton;

    //削除対象のフォルダ名
    public static string removeName;

    public FolderData()
    {
    }

    public FolderData(string path)
    {
      folderPath = path;
      var temp = path.Split('\\');
      name = temp.Last();
      if (name == "")
        name = "Cドライブ";

      button = new Button
      {
        Content = name,
        VerticalAlignment = VerticalAlignment.Top,
        HorizontalAlignment = HorizontalAlignment.Stretch,
      };

      if (Path.GetExtension(path) == "")
        button.Background = brushFolder;
      else
        button.Background = brushFile;

      RegisterEvent();
    }

    /// <summary>
    /// ボタンイベントを登録
    /// </summary>
    private void RegisterEvent()
    {
      button.Click += (sender, e) =>
      {
        Process.Start("explorer.exe", folderPath);
      };
      button.MouseRightButtonDown += (sender, e) =>
      {
        removeButton = button;
        removeName = name;
      };
    }
  }
}