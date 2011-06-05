using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using SmartXLS;
using Tables;

namespace Editor
{
    public class TblExporter
    {
        public static void Export(string xlsPath)
        {
            var extension = Path.GetExtension(xlsPath);
            if (extension != ".xls")
                return;

            var fileName = Path.GetFileName(xlsPath);
            var types = Editor.TableFileInfo.GetTableTypes(fileName);

            if (types == null)
                throw new InvalidOperationException("文件" + fileName + "没有相应的数据类型，无法导出tbl");

            foreach (var type in types)
            {
                MethodInfo method = typeof(TblExporter).GetMethod("ExportImpl", BindingFlags.NonPublic | BindingFlags.Static, null, new Type[] { typeof(string) }, null);
                MethodInfo generic = method.MakeGenericMethod(type);
                generic.Invoke(null, new object[] { xlsPath });
            }
        }

        private static void ExportImpl<T>(string xlsPath) where T : class, new()
        {
            var attribute = typeof(T).GetCustomAttributes(
                typeof(TableAttribute), false)[0] as TableAttribute;

            if ((attribute.Locations & TableLocations.Client) == TableLocations.Client)
            {
                var tblPath = "Assets/"
                    + ClientCore.ResourceManager.BundlesPath + "/Tables";

                if (!Directory.Exists(tblPath))
                    Directory.CreateDirectory(tblPath);

                tblPath = tblPath + "/" + typeof(T).FullName + ".tbl";
                ExportImpl<T>(xlsPath, tblPath);
            }

            if ((attribute.Locations & TableLocations.Server) == TableLocations.Server)
            {
                var tblPath = Preferences.GetString("服务器数据导出路径") + "/Tables";

                if (!Directory.Exists(tblPath))
                    Directory.CreateDirectory(tblPath);

                tblPath =  tblPath +"/" + typeof(T).FullName + ".tbl";
                ExportImpl<T>(xlsPath, tblPath);
            }
        }

        private static void ExportImpl<T>(string xlsPath, string tblPath) where T : class, new()
        {
            var tableData = new Tables.TableHolder<T>();
            tableData.Data = Editor.ExcelToProto.Export<T>(xlsPath);
            var stream = new FileStream(tblPath, FileMode.Create);
            ProtoBuf.Serializer.Serialize(stream, tableData);
            stream.Close();
        }
    }
}