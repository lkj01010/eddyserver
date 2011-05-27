using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using ProtoBuf;
using org.in2bits.MyXls;

namespace Eddy.Editor
{
    public class ExcelToProto
    {
        /// <summary>
        /// 把excel文件导出为protobuf对象
        /// 只导出第一个sheet，其中第一行视为列名，列名为protobuf字段属性的Name
        /// </summary>
        /// <typeparam name="T">protobuf对象类型</typeparam>
        /// <param name="file">文件路径</param>
        /// <returns></returns>
        public static List<T> Export<T>(string path) where T : class, new()
        {
            if (!typeof(T).IsDefined(typeof(ProtoContractAttribute), false))
                throw new InvalidOperationException("只有ProtoBuf类型可以导出");
            //var filestream = new FileStream(path, FileMode.Open, FileAccess.Read);
            var document = new XlsDocument(path);
            var sheet = document.Workbook.Worksheets[0];
            var rows = sheet.Rows;
            var nameRow = rows[(ushort)rows.MinRow];
            var columnNameIndex = GetColumnNameIndex(nameRow);
            var list = new List<T>();

            for (uint i = rows.MinRow + 1; i <= rows.MaxRow; ++i)
            {
                var row = rows[(ushort)i];
                list.Add(GetProto<T>(row, columnNameIndex));
            }

            return list;
        }

        private static T GetProto<T>(Row row, Dictionary<string, ushort> columnNameIndex) where T : class, new()
        {
            var proto = new T();
            var properties = typeof(T).GetProperties();
            var pairs = from property in properties
                        where property.IsDefined(typeof(ProtoMemberAttribute), false)
                        select
                        new
                        {
                            columnName = (Attribute.GetCustomAttribute(property, typeof(ProtoMemberAttribute)) as ProtoMemberAttribute).Name,
                            propertyName = property.Name
                        };

            foreach (var pair in pairs)
            {
                if (!columnNameIndex.ContainsKey(pair.columnName))
                    throw new InvalidOperationException("未定义字段：" + pair.columnName);

                var columnIndex = columnNameIndex[pair.columnName];
                if (row.CellExists(columnIndex))
                {
                    var cell = row.CellAtCol(columnIndex);
                    SetField(proto, pair.propertyName, cell);
                }
            }
            return proto;
        }

        private static void SetField(object proto, string name, Cell cell)
        {
            var type = proto.GetType();
            var propertyInfo = type.GetProperty(name);
            var propertyType = propertyInfo.PropertyType;
            if (propertyType == typeof(int))
            {
                propertyInfo.SetValue(proto, int.Parse(cell.Value.ToString()), null);
            }
            else if (propertyType == typeof(bool))
            {
                propertyInfo.SetValue(proto, Convert.ToBoolean(int.Parse(cell.Value.ToString())), null);
            }
            else if (propertyType == typeof(uint))
            {
                propertyInfo.SetValue(proto, uint.Parse(cell.Value.ToString()), null);
            }
            else if (propertyType == typeof(float))
            {
                propertyInfo.SetValue(proto, float.Parse(cell.Value.ToString()), null);
            }
            else if (propertyType == typeof(double))
            {
                propertyInfo.SetValue(proto, double.Parse(cell.Value.ToString()), null);
            }
            else if (propertyType == typeof(string))
            {
                propertyInfo.SetValue(proto, cell.Value.ToString(), null);
            }
            else
            {
                throw new InvalidOperationException("不支持的类型：" + propertyType);
            }
        }

        private static Dictionary<string, ushort> GetColumnNameIndex(Row nameRow)
        {
            Dictionary<string, ushort> names = new Dictionary<string, ushort>();

            for (ushort i = nameRow.MinCellCol; i <= nameRow.MaxCellCol; ++i)
            {
                names.Add(nameRow.GetCell(i).Value as string, nameRow.GetCell(i).Column);
            }

            return names;
        }
    }
}
