using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Reflection;

namespace Eddy.Message
{
    /// <summary>
    /// 标志是一个可以响应消息的方法
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class ExecuteAttribute : Attribute
    {
        #region 具有ExecuteAttribute特性函数的自动提取
        /// <summary>
        /// 得到类中所有具有<see cref="ExecuteAttribute"/>特性的静态方法
        /// </summary>
        public static List<MethodInfo> GetStaticExecuteMethod(Type type)
        {
            return GetExecuteMethod(type, BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Public
                | BindingFlags.Static);
        }

        /// <summary>
        /// 得到所给汇编中所有具有<see cref="ExecuteAttribute"/>特性的静态方法
        /// </summary>
        public static List<MethodInfo> GetStaticExecuteMethod(Assembly assembly)
        {
            var ret = new List<MethodInfo>();

            foreach (Module module in assembly.GetModules())
            {
                foreach (Type type in module.GetTypes())
                {
                    ret.AddRange(GetStaticExecuteMethod(type));
                }
            }

            return ret;
        }

        /// <summary>
        /// 得到对象中所有具有<see cref="ExecuteAttribute"/>特性的方法
        /// </summary>
        public static List<MethodInfo> GetInstanceExecuteMethod(Type targetType)
        {
            return GetExecuteMethod(targetType, BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Public
                | BindingFlags.Instance);
        }

        private static List<MethodInfo> GetExecuteMethod(Type targetType, BindingFlags flags)
        {
            var ret = new List<MethodInfo>();

            foreach (MethodInfo method in targetType.GetMethods(flags))
            {
                ExecuteAttribute exe = Attribute.GetCustomAttribute(method, typeof(ExecuteAttribute)) as ExecuteAttribute;
                if (exe != null)
                    ret.Add(method);
            }

            return ret;
        }
        #endregion
    }
}
