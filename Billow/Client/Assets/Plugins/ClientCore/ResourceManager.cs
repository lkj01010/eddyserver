using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Eddy;
using Eddy.Extensions;
using UnityEngine;

namespace ClientCore
{
    public static class ResourceManager
    {
        private static Dictionary<string, WeakReference> resources
            = new Dictionary<string, WeakReference>();
        private static Dictionary<string, ResourceMetaData> metaOfResource
            = new Dictionary<string, ResourceMetaData>();
        private static Dictionary<string, string> metaOfMeta;

        public static readonly string BundlesPath = "Bundles";
        public static readonly string LocalDataPath = Application.dataPath
            + "/../" + BundlesPath;
        public static readonly string RemoteDataPath = @"file://" + Application.dataPath
            + "/../RemoteBundles";

        /// <summary>
        /// 需要直接导出的文件类型
        /// </summary>
        public static readonly string[] ExportableFileTypes = { ".tbl", ".xml" };

        /// <summary>
        /// 需要打包成AssetBundle后导出的文件类型
        /// </summary>
        public static readonly string[] PackedExportableFileTypes = { ".prefab", ".fbx", };

        public static string GetExportFileName(string assetPath)
        {
            var bundlePath = assetPath.Replace('/', '-');
            return bundlePath;
        }

        public static bool IsExportable(string path)
        {
            var extension = path.Substring(path.LastIndexOf('.'));
            extension = extension.ToLower();
            return ExportableFileTypes.Contains(extension);
        }

        public static bool IsPackedExportable(string path)
        {
            var extension = path.Substring(path.LastIndexOf('.'));
            extension = extension.ToLower();
            return PackedExportableFileTypes.Contains(extension);
        }

        public static Resource GetResource(string relativePath)
        {
            var refrence = resources.GetValueOrDefault(relativePath);

            if (refrence != null && refrence.Target != null)
                return refrence.Target as Resource;

            var resource = new Resource();
            resource.RelativePath = relativePath;

            refrence = new WeakReference(resource, false);
            resources[relativePath] = refrence;
            return resource;
        }

        internal static IEnumerator LoadAsync(Resource resource)
        {
            if (resource.IsLoading || resource.IsDone)
                yield break;

            resource.IsLoading = true;

            yield return LoadMetaAsync(resource);

            resource.dependencies = GetDependencies(resource.RelativePath);
            var dependencies = resource.dependencies;

            yield return LoadDependenciesAsync(dependencies);
            yield return LoadWWWAsync(resource);

            if (resource.www.assetBundle != null)
                resource.www.assetBundle.LoadAll();

            Debug.Log(resource.RelativePath + " is loaded.");
        }

        private static List<Resource> GetDependencies(string relativePath)
        {
            var dependencyPaths = metaOfResource[relativePath].Dependencies;

            if (dependencyPaths == null || dependencyPaths.Count == 0)
                return null;

            var dependencies = new List<Resource>();

            foreach (var dependencyPath in dependencyPaths)
                dependencies.Add(GetResource(dependencyPath));

            return dependencies;
        }

        private static IEnumerator LoadWWWAsync(Resource resource)
        {
            var exportFileName = GetExportFileName(resource.RelativePath);
            var www = new WWW("file://" + LocalDataPath + "/" + exportFileName);
            yield return www;

            if (www.error == null)
            {
                var md5 = MD5Hash.Get(www.bytes);
                var meta = metaOfResource[resource.RelativePath];
                if (md5 == meta.MD5)
                {
                    resource.www = www;
                    yield break;
                }
            }

            www = new WWW(RemoteDataPath + "/" + exportFileName);
            yield return www;
            resource.www = www;

            var file = new FileStream(LocalDataPath + "/" + exportFileName,
                FileMode.Create,
                FileAccess.Write);
            file.Write(www.bytes, 0, www.bytes.Length);
            file.Close();
        }

        private static IEnumerator LoadMetaAsync(Resource resource)
        {
            yield return LoadMetaOfMetaAsync();
            yield return LoadMetaOfResourceAsync(resource.RelativePath);
        }

        private static IEnumerator LoadMetaOfResourceAsync(string relativePath)
        {
            if (metaOfResource.ContainsKey(relativePath))
                yield break;

            var metaPath = GetMetaFileName(relativePath);
            var file = new FileStream(LocalDataPath + "/" + metaPath,
                FileMode.OpenOrCreate, FileAccess.ReadWrite);

            var md5 = MD5Hash.Get(file);
            file.Position = 0;

            var document = new XmlDocument();
            if (md5 == metaOfMeta[metaPath])
            {
                document.Load(file);
            }
            else
            {
                var www = new WWW(RemoteDataPath + "/" + metaPath);
                yield return www;
                document.LoadXml(www.text);
                file.SetLength(0);
                file.Write(www.bytes, 0, www.bytes.Length);
            }

            file.Close();

            LoadMetaOfResource(document);
        }

        private static void LoadMetaOfResource(XmlDocument document)
        {
            var files = document.SelectNodes("/root/file");
            foreach (var file in files)
            {
                var element = file as XmlElement;
                var meta = new ResourceMetaData();
                meta.RelativePath = element.GetAttribute("path");
                meta.MD5 = element.GetAttribute("md5");

                var dependencies = element.SelectNodes("dependency");

                if (dependencies != null && dependencies.Count > 0)
                    meta.Dependencies = new List<string>();

                foreach (var dependency in dependencies)
                {
                    meta.Dependencies.Add((dependency as XmlElement).GetAttribute("path"));
                }
                metaOfResource[meta.RelativePath] = meta;
            }
        }

        private static string GetMetaFileName(string relativePath)
        {
            var directory = relativePath.Substring(0, relativePath.LastIndexOf('/'));
            return GetExportFileName(directory + "-" + "Meta.xml");
        }

        private static IEnumerator LoadMetaOfMetaAsync()
        {
            if (metaOfMeta != null)
                yield break;

            WWW www = new WWW(RemoteDataPath + "/Meta.xml");
            yield return www;

            FileStream file = new FileStream(LocalDataPath + "/Meta.xml",
                FileMode.Create,
                FileAccess.Write);
            file.Write(www.bytes, 0, www.bytes.Length);
            file.Close();

            XmlDocument document = new XmlDocument();
            document.LoadXml(www.text);

            LoadMetaOfMeta(document);
        }

        private static void LoadMetaOfMeta(XmlDocument document)
        {
            var nodes = document.SelectNodes("/root/file");

            if (nodes == null)
                return;

            if (nodes.Count == 0)
                return;

            metaOfMeta = new Dictionary<string, string>();
            foreach (var node in nodes)
            {
                var element = node as XmlElement;
                metaOfMeta[element.GetAttribute("path")] = element.GetAttribute("md5");
            }
        }

        private static IEnumerator LoadDependenciesAsync(List<Resource> dependencies)
        {
            if (dependencies != null)
            {
                foreach (var dependency in dependencies)
                {
                    yield return dependency.Wait();
                }
            }
        }
    }
}
