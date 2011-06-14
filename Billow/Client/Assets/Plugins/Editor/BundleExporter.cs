using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.IO;
using System.Xml;
using System.Linq;
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

    public static void ExportBundleIfNeeded(string path)
    {
        if (!path.StartsWith(bundlePath))
            return;

        ExportBundle(path);
    }

    private static string GetRelativePath(string path)
    {
        if (!path.StartsWith(bundlePath))
            throw new InvalidOperationException("路径不在Assets/Bundles下。");
        return path.Substring(bundlePath.Length);
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

    private static void ExportBundle(string path)
    {
        Dictionary<string, int> dependencyTree = BuildDependencyTree(path);
        var sorted = (from dependency in dependencyTree
                      group dependency by dependency.Value into g
                      select new { Layer = g.Key, Dependencies = from pair in g select pair.Key })
                     .OrderByDescending((x) => x.Layer);

        for (int i = 0; i < sorted.Count(); ++i)
        {
            BuildPipeline.PushAssetDependencies();

            foreach (var dependency in sorted.ElementAt(i).Dependencies)
            {
                var relativePath = GetRelativePath(dependency);
                var exportFileName = ResourceManager.GetExportFileName(relativePath);
                var exportPath = Preferences.GetString("客户端资源导出路径") + "/" + exportFileName;

                if (!Directory.Exists(Preferences.GetString("客户端资源导出路径")))
                    Directory.CreateDirectory(Preferences.GetString("客户端资源导出路径"));

                IEnumerable<string> lowerDependencies = null;

                if (i < sorted.Count() && i > 0)
                    lowerDependencies = sorted.ElementAt(i - 1).Dependencies;

                if (ExportBundleWithoutMetaData(dependency, exportPath))
                {
                    WriteMetaData(relativePath, lowerDependencies);
                    Debug.Log(exportPath + " 导出成功。");
                }
            }
        }
		
        for (int i = 0; i < sorted.Count(); ++i)
        {
            BuildPipeline.PopAssetDependencies();
        }
    }

    private static Dictionary<string, int> BuildDependencyTree(string path)
    {
        var dictionary = new Dictionary<string, int>();
        int layer = 0;
        dictionary[path] = layer;
		
        while (dictionary.Values.Any((x) => x == layer))
        {
            var layerElements = (from element in dictionary
                                where element.Value == layer
                                select element).ToList();
            ++layer;
            foreach (var element in layerElements)
            {
                foreach (var dependency in GetDependencies(element.Key))
                {
                    dictionary[dependency] = layer;
                }
            }
        }

        return dictionary;
    }

    private static IEnumerable<string> GetDependencies(string path)
    {
        var asset = AssetDatabase.LoadMainAssetAtPath(path);

        var dependencies = (from obj in EditorUtility.CollectDependencies(new[] { asset })
                            let objPath = AssetDatabase.GetAssetPath(obj)
                            where objPath.StartsWith(bundlePath)
                            && objPath != path 
                            && ResourceManager.IsPackedExportable(objPath)
                            select objPath).Distinct();
        return dependencies;
    }

    private static bool ExportBundleWithoutMetaData(string path,
        string exportPath)
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
        var options = (BuildAssetBundleOptions.CollectDependencies | BuildAssetBundleOptions.CompleteAssets
            | BuildAssetBundleOptions.DeterministicAssetBundle);
        if (!BuildPipeline.BuildAssetBundle(obj, null, exportPath, options))
        {
            throw new InvalidOperationException(exportPath + " 导出失败。");
        }
    }

    private static void WriteMetaData(string relativePath, IEnumerable<string> dependencies)
    {
        var xmlFileName = GetXmlFileName(relativePath);
        var xmlPath = Preferences.GetString("客户端资源导出路径") + "/" + xmlFileName;

        var doc = GetXmlDocument(xmlPath);

        var node = InitXmlNode(doc, relativePath);
        SetXmlNodeFilePath(node, relativePath);
        SetXmlNodeMD5(node, relativePath);
        SetXmlNodeDependencies(doc, node, dependencies);

        doc.Save(xmlPath);

        if (xmlFileName != "Meta.xml")
            WriteMetaData(xmlFileName, null);
    }

    private static void SetXmlNodeFilePath(XmlElement node, string relativePath)
    {
        node.SetAttribute("path", relativePath);
    }

    private static void SetXmlNodeDependencies(XmlDocument doc, 
        XmlElement node, 
        IEnumerable<string> dependencies)
    {
        if (dependencies == null)
            return;

        foreach (var dependency in dependencies)
        {
            var child = doc.CreateElement("dependency");
            child.SetAttribute("path", GetRelativePath(dependency));
            node.AppendChild(child);
        }
    }

    private static void SetXmlNodeMD5(XmlElement node, string relativePath)
    {
        string md5 = GetFileMD5(Preferences.GetString("客户端资源导出路径") + "/" +
            ResourceManager.GetExportFileName(relativePath));
        node.SetAttribute("md5", md5);
    }

    private static XmlDocument GetXmlDocument(string xmlPath)
    {
        XmlDocument doc = new XmlDocument();

        if (File.Exists(xmlPath))
            doc.Load(xmlPath);
        return doc;
    }

    private static XmlElement InitXmlNode(XmlDocument xmlDoc, string relativePath)
    {
        var root = GetXmlRoot(xmlDoc);

        var node = (root.SelectSingleNode("file[@path = '" + relativePath + "']") as XmlElement);

        if (node == null)
        {
            node = xmlDoc.CreateElement("file");
            root.AppendChild(node);
        }
        else
        {
            node.RemoveAll();
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
