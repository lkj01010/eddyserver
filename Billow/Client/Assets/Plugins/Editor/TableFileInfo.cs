using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Common.Tables;

namespace Editor
{
    public static class TableFileInfo
    {
        private static Dictionary<string, List<Type>> fileToTable = new Dictionary<string, List<Type>>();

        static TableFileInfo()
        {
            var assembly = Assembly.GetAssembly(typeof(Common.Tables.TableAttribute));
            var types = from type in assembly.GetTypes()
                        where type.IsDefined(typeof(TableAttribute), false)
                        select type;
            foreach (var type in types)
            {
                var attributes = type.GetCustomAttributes(typeof(TableAttribute), false);
                var fileName = (attributes.First() as TableAttribute).FileName;

                if (!fileToTable.ContainsKey(fileName))
                    fileToTable.Add(fileName, new List<Type>());

                fileToTable[fileName].Add(type);
            }
        }

        public static Type[] GetTableTypes(string fileName)
        {
            List<Type> types;
            fileToTable.TryGetValue(fileName, out types);
            if (types != null)
                return types.ToArray();
            else
                return null;
        }
    }
}
