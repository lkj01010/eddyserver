using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Eddy;
using Eddy.Extensions;
using UnityEditor;
using UnityEngine;

public class CustomPostprocessors : AssetPostprocessor
{
    private static Dictionary<string, IPostprocessor> postprocessors = new Dictionary<string, IPostprocessor>();

    static CustomPostprocessors()
    {
        postprocessors[".xls"] = new XlsPostprocessor();
        postprocessors[".tbl"] = new TblPostprocessor();
    }

    private static void OnPostprocessAllAssets(
        string[] importedAssets,
        string[] deletedAssets,
        string[] movedAssets,
        string[] movedFromAssetPaths
        )
    {
        foreach (var str in importedAssets)
            OnReimported(str);

        foreach (var str in deletedAssets)
            OnDeleted(str);

        for (var i = 0; i < movedAssets.Length; i++)
            OnMoved(movedFromAssetPaths[i], movedAssets[i]);
    }

    public static void OnReimported(string path)
    {
        var processor = postprocessors.GetValueOrDefault(Path.GetExtension(path));
        if (processor != null)
            processor.OnReimported(path);
    }

    public static void OnDeleted(string path)
    {
        var processor = postprocessors.GetValueOrDefault(Path.GetExtension(path));
        if (processor != null)
            processor.OnDeleted(path);
    }

    public static void OnMoved(string from, string to)
    {
        var processor = postprocessors.GetValueOrDefault(Path.GetExtension(from));
        if (processor != null)
            processor.OnMoved(from, to);
    }
}
