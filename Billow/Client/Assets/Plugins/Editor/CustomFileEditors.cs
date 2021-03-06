using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using UnityEngine;
using UnityEditor;

public class CustomFileEditors
{
    [MenuItem("Assets/Custom Edit %e")]
    static void Edit()
    {
        if (Selection.activeObject == null)
            return;

        var path = AssetDatabase.GetAssetPath(Selection.activeObject);
        //Debug.Log(path);
        var extension = Path.GetExtension(path);
        if (extension == ".xls")
            XlsEditor.Init(path);
        else if (extension == ".tbl")
            TblEditor.Init(path);
    }
}
