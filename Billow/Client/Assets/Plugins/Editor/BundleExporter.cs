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
    private static readonly string bundlePath = "Assets/" + ResourceManager.BundlesPath + "/";

    [MenuItem("Eddy/Export All Bundles")]
    public static void ExportAllBundles()
    {
        ExportDirectory("Assets/" + ResourceManager.BundlesPath);
    }

    [MenuItem("Assets/Export Bundles")]
    public static void ExportBundles()
    {
        var path = AssetDatabase.GetAssetPath(Selection.activeObject);
        if (File.Exists(path))
            ExportBundleIfNeeded(path);
        else if (Directory.Exists(path))
            ExportDirectoryIfNeeded(path);
    }

    public static void ExportDirectoryIfNeeded(string path)
    {
        if (!path.StartsWith(bundlePath))
            return;

        ExportDirectory(path);
    }

    private static void ExportDirectory(string path)
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

    public static void ExportBundleIfNeeded(string path)
    {
        if (!path.StartsWith(bundlePath))
            return;

        ExportBundle(path);
    }

    private static void ExportBundle(string path)
    {
        var relativePath = path.Substring(bundlePath.Length);
        var exportFileName = ResourceManager.GetExportFileName(relativePath);
        var exportPath = ResourceManager.BundlesPath + "/" + exportFileName;

        if (!Directory.Exists(ResourceManager.BundlesPath))
            Directory.CreateDirectory(ResourceManager.BundlesPath);

        if (ExportBundleWithoutMetaData(path, exportPath))
        {
            WriteMetaData(relativePath);
            Debug.Log(exportPath + " 导出成功。");
        }
    }

    private static bool ExportBundleWithoutMetaData(string path, string exportPath)
    {
        if (ResourceManager.IsExportable(path))
        {
            ExportExportable(path, exportPath);
        }
        else if (ResourceManager.IsPackedExportable(path))
        {
            ExportPackedExportable(path, exportPath);
        }
        else
        {
            return false;
        }

        return true;
    }

    private static void ExportExportable(string path, string exportPath)
    {
        FileUtil.CopyFileOrDirectory(path, exportPath);
    }

    private static void ExportPackedExportable(string path, string exportPath)
    {
        var obj = AssetDatabase.LoadMainAssetAtPath(path);
        if (!BuildPipeline.BuildAssetBundle(obj, null, exportPath,
            BuildAssetBundleOptions.CollectDependencies | BuildAssetBundleOptions.CompleteAssets))
        {
            throw new InvalidOperationException(exportPath + " 导出失败。");
        }
    }

    private static void WriteMetaData(string relativePath)
    {
        var xmlFileName = GetXmlFileName(relativePath);
        var xmlPath = ResourceManager.BundlesPath + "/" + xmlFileName;

        WriteXml(relativePath, xmlPath);

        if (xmlFileName != "Meta.xml")
            WriteMetaData(xmlFileName);
    }

    private static void WriteXml(string relativePath, string xmlPath)
    {
        XmlDocument xmlDoc = GetXmlDocument(xmlPath);

        var node = GetXmlNode(xmlDoc, relativePath);
        SetXmlNodeMD5(relativePath, node);

        xmlDoc.Save(xmlPath);
    }

    private static void SetXmlNodeMD5(string relativePath, XmlElement node)
    {
        string md5 = GetFileMD5(ResourceManager.BundlesPath + "/" +
            ResourceManager.GetExportFileName(relativePath));
        node.SetAttribute("md5", md5);
    }

    private static XmlDocument GetXmlDocument(string xmlPath)
    {
        XmlDocument xmlDoc = new XmlDocument();

        if (File.Exists(xmlPath))
            xmlDoc.Load(xmlPath);
        return xmlDoc;
    }

    private static XmlElement GetXmlNode(XmlDocument xmlDoc, string relativePath)
    {
        var root = GetXmlRoot(xmlDoc);

        var node = (root.SelectSingleNode("file[@path = '" + relativePath + "']") as XmlElement);

        if (node == null)
        {
            node = xmlDoc.CreateElement("file");
            node.SetAttribute("path", relativePath);
            root.AppendChild(node);
        }
        return node;
    }

    private static XmlNode GetXmlRoot(XmlDocument xmlDoc)
    {
        var root = xmlDoc.SelectSingleNode("root");

        if (root == null)
        {
            root = xmlDoc.CreateElement("root");
            xmlDoc.AppendChild(root);
        }
        return root;
    }

    private static string GetXmlFileName(string relativePath)
    {
        var xmlFileName = Path.GetDirectoryName(relativePath);

        if (xmlFileName != null && xmlFileName.Length > 0)
            xmlFileName += "/";

        xmlFileName += "Meta.xml";
        xmlFileName = ResourceManager.GetExportFileName(xmlFileName);
        return xmlFileName;
    }

    private static string GetFileMD5(string path)
    {
        FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read);
        var md5 = Eddy.MD5Hash.Get(stream);
        stream.Close();
        return md5;
    }
}
