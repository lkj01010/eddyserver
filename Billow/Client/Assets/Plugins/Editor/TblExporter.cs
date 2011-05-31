using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using SmartXLS;

public class TblExporter
{
    [MenuItem("Assets/Export Table")]
    public static void ExportTable()
    {
        if (Selection.activeObject == null)
            return;

        var path = AssetDatabase.GetAssetPath(Selection.activeObject);

        var extension = Path.GetExtension(path);
        if (extension != ".xls")
            return;

        var fileName = Path.GetFileName(path);
        var types = Common.Tables.TableFileInfo.Instance.GetTableTypes(fileName);

        if (types == null)
            throw new InvalidOperationException("文件" + fileName + "没有相应的数据类型");

        foreach (var type in types)
        {
            MethodInfo method = typeof(TblExporter).GetMethod("XlsToTbl",
                BindingFlags.NonPublic | BindingFlags.Static);
            MethodInfo generic = method.MakeGenericMethod(type);
            generic.Invoke(null, new object[] {path});
        }
    }

    private static void XlsToTbl<T>(string xlsFile) where T : class, new()
    {
        var tblFile = Path.GetDirectoryName(xlsFile) + "/" + typeof(T).Name + ".tbl";

        var tableData = new Common.Tables.TableHolder<T>();
        tableData.Data = Eddy.Editor.ExcelToProto.Export<T>(xlsFile);
        var stream = new FileStream(tblFile, FileMode.Create);
        ProtoBuf.Serializer.Serialize(stream, tableData);
		stream.Close();
    }
}
