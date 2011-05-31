using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common.Tables
{
    /// <summary>
    /// 表属性，用以关联表类型和excel文件
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class TableAttribute : Attribute
    {
        public string FileName { get; set; }

        public TableAttribute(string fileName)
        {
            this.FileName = fileName;
        }
    }
}
