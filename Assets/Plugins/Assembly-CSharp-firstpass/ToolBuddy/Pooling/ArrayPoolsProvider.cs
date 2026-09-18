using System;
using System.Collections.Generic;
using ToolBuddy.Pooling.Pools;

namespace ToolBuddy.Pooling
{
	public class ArrayPoolsProvider
	{
		private static readonly Dictionary<Type, object> arrayPools = new Dictionary<Type, object>();

		private static readonly object lockObject = new object();

		public static ArrayPool<T> GetPool<T>()
		{
			Type typeFromHandle = typeof(T);
			if (!arrayPools.TryGetValue(typeFromHandle, out var value))
			{
				lock (lockObject)
				{
					if (!arrayPools.TryGetValue(typeFromHandle, out value))
					{
						value = (arrayPools[typeFromHandle] = new ArrayPool<T>(1000000L));
					}
				}
			}
			return (ArrayPool<T>)value;
		}
	}
}
