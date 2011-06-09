using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Eddy;

namespace ClientCore
{
    public static class ResourceManager
    {
        public static readonly string BundlesPath = "Bundles";

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
    }
}
