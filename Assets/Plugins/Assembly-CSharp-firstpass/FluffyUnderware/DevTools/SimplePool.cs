using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace FluffyUnderware.DevTools
{
	internal class SimplePool<T> where T : new()
	{
		private readonly List<T> freeItemsBackfield;

		private static readonly Func<T> OptimizedInstantiator = ((Expression<Func<T>>)(() => new T())).Compile();

		public SimplePool(int preCreatedElementsCount)
		{
			freeItemsBackfield = new List<T>();
			for (int i = 0; i < preCreatedElementsCount; i++)
			{
				freeItemsBackfield.Add(OptimizedInstantiator());
			}
		}

		public T GetItem()
		{
			T result;
			if (freeItemsBackfield.Count == 0)
			{
				result = OptimizedInstantiator();
			}
			else
			{
				int index = freeItemsBackfield.Count - 1;
				result = freeItemsBackfield[index];
				freeItemsBackfield.RemoveAt(index);
			}
			return result;
		}

		public void ReleaseItem(T item)
		{
			freeItemsBackfield.Add(item);
		}
	}
}
