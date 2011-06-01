using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEditor;

namespace Editor
{
    public class XlsPostprocessor : IPostprocessor
    {
        #region IPostprocessor Members

        public void OnReimported(string path)
        {
            TblExporter.Export(path);
            Debug.Log(path + " 导出成功。");
        }

        public void OnDeleted(string path)
        {
            var tblPath = path.Substring(0, path.LastIndexOf(".xls"));
            tblPath += ".tbl";
            FileUtil.DeleteFileOrDirectory(tblPath);
            tblPath += ".meta";
            FileUtil.DeleteFileOrDirectory(tblPath);
        }

        public void OnMoved(string from, string to)
        {
            OnDeleted(from);
        }

        #endregion
    }
}
