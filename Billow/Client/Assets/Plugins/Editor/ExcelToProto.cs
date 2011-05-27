using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using ProtoBuf;
using SmartXLS;

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
            var book = new WorkBook();
            book.read(path);
            book.Sheet = 0;
            var columnNameIndex = GetColumnNameIndex(book);
            var list = new List<T>();

            for (int i = 1; i <= book.LastRow; ++i)
            {
                list.Add(GetProto<T>(book, i, columnNameIndex));
            }

            return list;
        }

        private static T GetProto<T>(WorkBook book, int row, Dictionary<string, ushort> columnNameIndex) where T : class, new()
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
                var columnName = pair.columnName;

                if (columnName == null)
                    columnName = pair.propertyName;

                if (!columnNameIndex.ContainsKey(columnName))
                    throw new InvalidOperationException("未定义字段：" + columnName);

                var column = columnNameIndex[columnName];
                var text = book.getText(row, column);

                if (text != null && text.Length != 0)
                    SetField(proto, pair.propertyName, text);
            }
            return proto;
        }

        private static void SetField(object proto, string name, string text)
        {
            var type = proto.GetType();
            var propertyInfo = type.GetProperty(name);
            var propertyType = propertyInfo.PropertyType;
            if (propertyType == typeof(int))
            {
                propertyInfo.SetValue(proto, int.Parse(text), null);
            }
            else if (propertyType == typeof(bool))
            {
                propertyInfo.SetValue(proto, Convert.ToBoolean(int.Parse(text)), null);
            }
            else if (propertyType == typeof(uint))
            {
                propertyInfo.SetValue(proto, uint.Parse(text), null);
            }
            else if (propertyType == typeof(float))
            {
                propertyInfo.SetValue(proto, float.Parse(text), null);
            }
            else if (propertyType == typeof(double))
            {
                propertyInfo.SetValue(proto, double.Parse(text), null);
            }
            else if (propertyType == typeof(string))
            {
                propertyInfo.SetValue(proto, text, null);
            }
            else
            {
                throw new InvalidOperationException("不支持的类型：" + propertyType);
            }
        }

        private static Dictionary<string, ushort> GetColumnNameIndex(WorkBook book)
        {
            Dictionary<string, ushort> names = new Dictionary<string, ushort>();

            for (ushort i = 0; i <= book.LastCol; ++i)
            {
                names.Add(book.getText(0, i), i);
            }

            return names;
        }
    }
}
