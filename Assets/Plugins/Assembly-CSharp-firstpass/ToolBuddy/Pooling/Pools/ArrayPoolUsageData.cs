namespace ToolBuddy.Pooling.Pools
{
	public readonly struct ArrayPoolUsageData
	{
		public long ElementsCount { get; }

		public int ArraysCount { get; }

		public long ElementsCapacity { get; }

		public ArrayPoolUsageData(long elementsCount, int arraysCount, long elementsCapacity)
		{
			ElementsCount = elementsCount;
			ArraysCount = arraysCount;
			ElementsCapacity = elementsCapacity;
		}

		public bool Equals(ArrayPoolUsageData other)
		{
			if (ElementsCount == other.ElementsCount && ArraysCount == other.ArraysCount)
			{
				return ElementsCapacity == other.ElementsCapacity;
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj is ArrayPoolUsageData other)
			{
				return Equals(other);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return (((ElementsCount.GetHashCode() * 397) ^ ArraysCount) * 397) ^ ElementsCapacity.GetHashCode();
		}

		public static bool operator ==(ArrayPoolUsageData a, ArrayPoolUsageData b)
		{
			return a.Equals(b);
		}

		public static bool operator !=(ArrayPoolUsageData a, ArrayPoolUsageData b)
		{
			return !(a == b);
		}

		public override string ToString()
		{
			return string.Format("{0}: {1}, {2}: {3}, {4}: {5}", "ElementsCount", ElementsCount, "ArraysCount", ArraysCount, "ElementsCapacity", ElementsCapacity);
		}
	}
}
