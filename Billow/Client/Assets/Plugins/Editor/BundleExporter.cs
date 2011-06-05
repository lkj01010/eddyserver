using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.IO;
using System.Xml;
using ClientCore;
using UnityEngine;
using UnityEditor;

public class BundleExporter
{
    [MenuItem("Eddy/Export Bundles")]
    public static void ExportBundles()
    {
        ExportDirectory("Assets/" + ResourceManager.BundlesPath);
    }

    public static void ExportDirectory(string path)
    {
        string[] files = Directory.GetFiles(path);
        foreach (string file in files)
        {
            var filePath = file.Replace('\\', '/');
            ExportBundle(filePath);
        }

        string[] directories = Directory.GetDirectories(path);
        foreach (var directory in directories)
        {
            var directoryPath = directory.Replace('\\', '/');
            ExportDirectory(directoryPath);
        }
    }

    public static void ExportBundle(string path)
    {
        var bundlePath = "Assets/" + ResourceManager.BundlesPath + "/";
		
        if (!path.StartsWith(bundlePath))
            return;
		
        var relativePath = path.Substring(bundlePath.Length);
        var exportFileName = ResourceManager.GetExportFileName(relativePath);
        var exportPath = ResourceManager.BundlesPath + "/" + exportFileName;
		
		if (!Directory.Exists(ResourceManager.BundlesPath)) 
                Directory.CreateDirectory(ResourceManager.BundlesPath);

        if (ExportBundle(path, exportPath))
        {
            WriteMetaData(relativePath);
            Debug.Log(exportPath + " 导出成功。");
        }
    }

    private static bool ExportBundle(string path, string exportPath)
    {
        if (ResourceManager.IsExportable(path))
        {
            FileUtil.CopyFileOrDirectory(path, exportPath);
        }
        else if (ResourceManager.IsPackedExportable(path))
        {
            var obj = AssetDatabase.LoadMainAssetAtPath(path);
            if (!BuildPipeline.BuildAssetBundle(obj, null, exportPath,
                BuildAssetBundleOptions.CollectDependencies | BuildAssetBundleOptions.CompleteAssets))
            {
                throw new InvalidOperationException(exportPath + " 导出失败。");
            }
        }
        else
        {
            return false;
        }

        return true;
    }

    private static void WriteMetaData(string relativePath)
    {
        var xmlFile = Path.GetDirectoryName(relativePath);
        xmlFile += "/meta.xml";
        xmlFile = ResourceManager.BundlesPath + "/" 
            + ResourceManager.GetExportFileName(xmlFile);

        XmlDocument xmlDoc = new XmlDocument();

        if (File.Exists(xmlFile))
            xmlDoc.Load(xmlFile);

        var root = xmlDoc.SelectSingleNode("root");

        if (root == null)
        {
            root = xmlDoc.CreateElement("root");
            xmlDoc.AppendChild(root);
        }

        var node = (root.SelectSingleNode("file[@path = '" + relativePath + "']") as XmlElement);

        if (node == null)
        {
            node = xmlDoc.CreateElement("file");
            node.SetAttribute("path", relativePath);
            root.AppendChild(node);
        }

        string md5 = GetFileMD5(ResourceManager.BundlesPath + "/" + 
            ResourceManager.GetExportFileName(relativePath));
        node.SetAttribute("md5", md5);

        xmlDoc.Save(xmlFile);
    }

    private static string GetFileMD5(string path)
    {
        FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read);
        var md5 = Eddy.MD5Hash.Get(stream);
        stream.Close();
        return md5;
    }
}
