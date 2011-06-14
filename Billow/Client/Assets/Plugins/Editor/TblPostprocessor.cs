using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEditor;

public class TblPostprocessor : IPostprocessor
{
    #region IPostprocessor Members

    public void OnReimported(string path)
    {
        BundleExporter.ExportBundleIfNeeded(path);
    }

    public void OnDeleted(string path)
    {
    }

    public void OnMoved(string from, string to)
    {
        OnDeleted(from);
    }

    #endregion
}
