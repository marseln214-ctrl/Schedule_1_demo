using ToolBuddy.Pooling.Pools;

namespace ToolBuddy.Pooling.Collections
{
	public struct SubArrayList<T>
	{
		private readonly ArrayPool<T> typePool;

		private SubArray<T> subArray;

		public T[] Array => subArray.Array;

		public int Count { get; private set; }

		public SubArrayList(int initialCapacity, ArrayPool<T> typePool)
		{
			this.typePool = typePool;
			subArray = typePool.Allocate(initialCapacity, clearArray: false);
			Count = 0;
		}

		public void Add(T element)
		{
			if (Count == subArray.Count)
			{
				int newMinimalSize = ((subArray.Count == 0) ? 4 : (subArray.Count * 2));
				typePool.Resize(ref subArray, newMinimalSize, clearNewSpace: false);
			}
			subArray.Array[Count] = element;
			Count++;
		}

		public SubArray<T> ToSubArray()
		{
			return new SubArray<T>(subArray.Array, Count);
		}

		public bool Equals(SubArrayList<T> other)
		{
			if (subArray.Equals(other.subArray))
			{
				return Count == other.Count;
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj is SubArrayList<T> other)
			{
				return Equals(other);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return (subArray.GetHashCode() * 397) ^ Count;
		}

		public static bool operator ==(SubArrayList<T> a, SubArrayList<T> b)
		{
			return a.Equals(b);
		}

		public static bool operator !=(SubArrayList<T> a, SubArrayList<T> b)
		{
			return !(a == b);
		}
	}
}
