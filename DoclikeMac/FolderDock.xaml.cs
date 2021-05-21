using System.Windows;

namespace DocklikeMac
{
  /// <summary>
  /// Interaction logic for FolderDock.xaml
  /// </summary>
  public partial class FolderDock : Window
  {
    private readonly FolderManager manager;

    public FolderDock()
    {
      manager = new FolderManager();
      InitializeComponent();
      InitFolderList();
    }

    /// <summary>
    /// フォルダリスト(grid)の初期処理
    /// </summary>
    private void InitFolderList()
    {
      for (var idx = 0; idx < manager.CountOfApps(); idx++)
        folderList.Children.Add(manager.GetFolderButton(idx));

      /* ウィンドウの高さは登録フォルダの数に比例する */
      var count = manager.CountOfApps();
      Height += (count <= 0) ? 0 : title.Height * count;
    }

    /* xamlイベント */

    /// <summary>
    /// ドラッグ時のカーソル横のアイコンを変更
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Window_DragOver(object sender, DragEventArgs e)
    {
      if (e.Data.GetDataPresent(DataFormats.FileDrop, true))
        e.Effects = DragDropEffects.Copy;
      else
        e.Effects = DragDropEffects.None;

      e.Handled = true;
    }

    /// <summary>
    /// フォルダパスを追加したときの処理
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Window_Drop(object sender, DragEventArgs e)
    {
      if (e.Data.GetData(DataFormats.FileDrop) is not string[] dropFiles) return;
      //同じパスのフォルダがない場合のみ追加する
      if (manager.InsertFolderData(dropFiles[0]))
      {
        folderList.Children.Add(manager.GetFolderButton());
        manager.WriteJson();
      }
    }

    /// <summary>
    /// 右クリックを検知し，要素を削除
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void StackPanel_MouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
      folderList.Children.Remove(FolderData.removeButton);
      manager.RemoveFolderData();
      manager.WriteJson();
    }

    /// <summary>
    /// ウィンドウを閉じる
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Button_Click(object sender, RoutedEventArgs e)
    {
      this.Close();
    }
  }
}