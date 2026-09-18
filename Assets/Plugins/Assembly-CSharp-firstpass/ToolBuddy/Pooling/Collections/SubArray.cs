using System;
using JetBrains.Annotations;
using ToolBuddy.Pooling.Pools;

namespace ToolBuddy.Pooling.Collections
{
	public readonly struct SubArray<T>
	{
		public readonly T[] Array;

		public readonly int Count;

		public T this[int index]
		{
			get
			{
				return Array[index];
			}
			set
			{
				Array[index] = value;
			}
		}

		public SubArray([NotNull] T[] array)
		{
			if (array == null)
			{
				throw new ArgumentNullException("array");
			}
			Array = array;
			Count = array.Length;
		}

		public SubArray(T[] array, int count)
		{
			if (array == null)
			{
				throw new ArgumentNullException("array");
			}
			if (count < 0)
			{
				throw new ArgumentOutOfRangeException("count");
			}
			if (count > array.Length)
			{
				throw new ArgumentOutOfRangeException("count");
			}
			Array = array;
			Count = count;
		}

		public T[] CopyToArray(ArrayPool<T> arrayPool)
		{
			T[] array = arrayPool.AllocateExactSize(Count, clearArray: false).Array;
			System.Array.Copy(Array, 0, array, 0, Count);
			return array;
		}

		public override int GetHashCode()
		{
			if (Array == null)
			{
				return 0;
			}
			return Array.GetHashCode() ^ Count;
		}

		public override bool Equals(object obj)
		{
			if (obj is SubArray<T> obj2)
			{
				return Equals(obj2);
			}
			return false;
		}

		public bool Equals(SubArray<T> obj)
		{
			if (obj.Array == Array)
			{
				return obj.Count == Count;
			}
			return false;
		}

		public static bool operator ==(SubArray<T> a, SubArray<T> b)
		{
			return a.Equals(b);
		}

		public static bool operator !=(SubArray<T> a, SubArray<T> b)
		{
			return !(a == b);
		}
	}
}
