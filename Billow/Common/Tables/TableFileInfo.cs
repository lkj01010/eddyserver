using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Common.Tables
{
    public class TableFileInfo
    {
        private static readonly TableFileInfo instance = new TableFileInfo();
        public static TableFileInfo Instance { get { return instance; } }

        private Dictionary<string, List<Type>> fileToTable = new Dictionary<string, List<Type>>();

        private TableFileInfo()
        {
            var assembly = Assembly.GetAssembly(typeof(TableFileInfo));
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

        public Type[] GetTableTypes(string fileName)
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
