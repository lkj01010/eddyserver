using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Eddy
{
	#region StaticSingleton
	/// <summary>
	/// Implementing the Singleton Pattern in C#
	/// Thread-safety without using locks
	/// 所管理的对象将在第一时间由框架自动初始化并持续。
	/// </summary>
	/// <ref>http://www.yoda.arachsys.com/csharp/singleton.html</ref>
	/// <example>
	/// <code>
	/// class Demo
	/// {
	/// 	public static Demo Instance { get { return StaticSingleton&lt;Demo&gt;.Instance; } }
	/// }
	/// </code>
	/// </example>
	public sealed class StaticSingleton<T>
		where T : class, new()
	{
		/// <remarks>
		/// Explicit static constructor to tell C# compiler not to mark type as beforfieldinit.
		/// </remarks>
		static StaticSingleton()
		{
		}
		private static readonly T instance = new T();
		public static T Instance { get { return instance; } }
	}
	#endregion

	#region LazySingleton
	/// <summary>
	/// Implementing the Singleton Pattern in C#
	/// Thread-safety using double-check locking
	/// 所管理对象的初始化和释放用户可控
	/// </summary>
	/// <example>
	/// <code>
	/// class Demo
	/// {
	///		private static readonly LazySingleton&lt;Demo&gt; singleton = new LazySingleton&lt;Demo&gt;(() =&gt; new Demo());
	/// 	public static Demo Instance { get { return singleton.GetInstance(); } }
	/// 	private Demo(){}
	/// }
	/// </code>
	/// </example>
	public sealed class LazySingleton<T>
		where T : class
	{
		private Func<T> ctor;
		private T instance;
		private readonly object instanceLock = new object();

		public LazySingleton(Func<T> factoryMethod)
		{
			ctor = factoryMethod;
		}

		/// <summary>
		/// 确保得到对象单件实例，绝不会返回null
		/// </summary>
		public T GetInstance()
		{
			if (instance == null)
			{
				lock (instanceLock)
				{
					if (instance == null)
					{
						NewInstance();
					}
				}
			}
			Debug.Assert(instance != null);
			return instance;
		}

		/// <summary>
		/// 尝试得到对象单件实例，若尚未构造则返回null
		/// </summary>
		public T TryGetInstance()
		{
			return instance;
		}

		/// <summary>
		/// 创建单件实例
		/// </summary>
		public T NewInstance()
		{
			Debug.Assert(instance == null);
			instance = ctor();
			return instance;
		}

		/// <summary>
		/// 将给定的实例指定为单件实例
		/// </summary>
		/// <param name="instance">要指定为单件实例的实例</param>
		public T NewInstance(T instance)
		{
			Debug.Assert(this.instance == null);
			this.instance = instance;
			return instance;
		}

		/// <summary>
		/// 删除单件实例
		/// </summary>
		public T DeleteInstance()
		{
			T ret = instance;
			instance = null;
			return ret;
		}
	}

	#endregion
}
