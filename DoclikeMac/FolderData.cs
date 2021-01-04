using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocklikeMac
{
    class FolderData
    {
        //フォルダのパス
        private string folderPath;
        //フォルダ名
        private string name;

        FolderData()
        {

        }

        FolderData(string path)
        {
            folderPath = path;
            var temp = path.Split('\\');
            name = temp.Last();
        }

        public void OpenFolder()
        {
            Process.Start(folderPath);
        }
    }
}
