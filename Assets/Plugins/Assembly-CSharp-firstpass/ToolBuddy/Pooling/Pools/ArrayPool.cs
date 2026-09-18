using System;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using ToolBuddy.Pooling.Collections;
using UnityEngine;

namespace ToolBuddy.Pooling.Pools
{
	public class ArrayPool<T>
	{
		private readonly SubArray<T> emptySubArray = new SubArray<T>(new T[0]);

		private readonly System.Random random = new System.Random();

		private const int keysInitialCapacity = 200;

		private int[] poolKeys = new int[200];

		private T[][] poolValues = new T[200][];

		private int arraysCount;

		private long elementsCount;

		private long elementsCapacity;

		public long ElementsCapacity
		{
			get
			{
				return elementsCapacity;
			}
			set
			{
				if (elementsCapacity != value)
				{
					lock (this)
					{
						elementsCapacity = value;
						ApplyCapacity(elementsCapacity);
					}
				}
			}
		}

		public bool LogAllocations { get; set; }

		public ArrayPoolUsageData UsageData => new ArrayPoolUsageData(elementsCount, arraysCount, elementsCapacity);

		public ArrayPool(long elementsCapacity)
		{
			if (elementsCapacity < 0)
			{
				throw new ArgumentOutOfRangeException("elementsCapacity", "Must be strictly positive.");
			}
			this.elementsCapacity = elementsCapacity;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SubArray<T> Allocate(int minimalSize, bool clearArray = true)
		{
			bool isArrayCleared;
			return Allocate(minimalSize, exactSize: false, clearArray, out isArrayCleared);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SubArray<T> AllocateExactSize(int exactSize, bool clearArray = true)
		{
			bool isArrayCleared;
			return Allocate(exactSize, exactSize: true, clearArray, out isArrayCleared);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Free(SubArray<T> subArray)
		{
			if (subArray.Array != null)
			{
				Free(subArray.Array);
			}
		}

		public void Free([NotNull] T[] array)
		{
			if (array.Length > elementsCapacity || array.Length == 0)
			{
				return;
			}
			lock (this)
			{
				ApplyCapacity(elementsCapacity - array.Length);
				int num = BinarySearch(poolKeys, arraysCount, array.Length);
				int num2 = ((num >= 0) ? num : (~num));
				if (arraysCount == poolKeys.Length)
				{
					int newSize = 2 * (arraysCount + 1);
					Array.Resize(ref poolValues, newSize);
					Array.Resize(ref poolKeys, newSize);
				}
				if (num2 < arraysCount)
				{
					Array.Copy(poolKeys, num2, poolKeys, num2 + 1, arraysCount - num2);
					Array.Copy(poolValues, num2, poolValues, num2 + 1, arraysCount - num2);
				}
				poolKeys[num2] = array.Length;
				poolValues[num2] = array;
				arraysCount++;
				elementsCount += array.Length;
			}
		}

		public void Resize(ref SubArray<T> subArray, int newMinimalSize, bool clearNewSpace = true)
		{
			if (subArray.Count == newMinimalSize)
			{
				return;
			}
			if (newMinimalSize < 0)
			{
				throw new ArgumentOutOfRangeException("newMinimalSize", "Must be positive.");
			}
			if (newMinimalSize == 0)
			{
				Free(subArray);
				subArray = emptySubArray;
				return;
			}
			int count = subArray.Count;
			bool isArrayCleared;
			if (newMinimalSize > subArray.Array.Length)
			{
				SubArray<T> subArray2 = Allocate(newMinimalSize, exactSize: false, clearArray: false, out isArrayCleared);
				Array.Copy(subArray.Array, 0, subArray2.Array, 0, subArray.Count);
				Free(subArray);
				subArray = subArray2;
			}
			else
			{
				subArray = new SubArray<T>(subArray.Array, newMinimalSize);
				isArrayCleared = false;
			}
			if (clearNewSpace && !isArrayCleared && newMinimalSize > count)
			{
				Array.Clear(subArray.Array, count, newMinimalSize - count);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void ResizeAndClear(ref SubArray<T> subArray, int newMinimalSize)
		{
			if (subArray.Count == newMinimalSize)
			{
				Array.Clear(subArray.Array, 0, newMinimalSize);
				return;
			}
			if (newMinimalSize < 0)
			{
				throw new ArgumentOutOfRangeException("newMinimalSize", "Must be positive.");
			}
			if (newMinimalSize == 0)
			{
				Free(subArray);
				subArray = emptySubArray;
			}
			else if (newMinimalSize > subArray.Array.Length)
			{
				bool isArrayCleared;
				SubArray<T> subArray2 = Allocate(newMinimalSize, exactSize: false, clearArray: true, out isArrayCleared);
				Free(subArray);
				subArray = subArray2;
			}
			else
			{
				subArray = new SubArray<T>(subArray.Array, newMinimalSize);
				Array.Clear(subArray.Array, 0, newMinimalSize);
			}
		}

		public void ResizeCopyless(ref SubArray<T> subArray, int newMinimalSize)
		{
			if (subArray.Count != newMinimalSize)
			{
				if (newMinimalSize < 0)
				{
					throw new ArgumentOutOfRangeException("newMinimalSize", "Must be positive.");
				}
				if (newMinimalSize == 0)
				{
					Free(subArray);
					subArray = emptySubArray;
				}
				else if (newMinimalSize > subArray.Array.Length)
				{
					Free(subArray);
					subArray = Allocate(newMinimalSize, exactSize: false, clearArray: false, out var _);
				}
				else
				{
					subArray = new SubArray<T>(subArray.Array, newMinimalSize);
				}
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SubArray<T> Clone(T[] source)
		{
			SubArray<T> result = Allocate(source.Length, clearArray: false);
			Array.Copy(source, 0, result.Array, 0, source.Length);
			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SubArray<T> Clone(SubArray<T> source)
		{
			SubArray<T> result = Allocate(source.Count, clearArray: false);
			Array.Copy(source.Array, 0, result.Array, 0, source.Count);
			return result;
		}

		private SubArray<T> Allocate(int size, bool exactSize, bool clearArray, out bool isArrayCleared)
		{
			if (size > elementsCapacity)
			{
				isArrayCleared = true;
				if (LogAllocations)
				{
					Debug.Log($"[ArrayPools] Type: {typeof(T).Name}. Allocated array size {size}. The requested size is bigger than the pool's capacity {elementsCapacity}");
				}
				return new SubArray<T>(new T[size], size);
			}
			if (size == 0)
			{
				isArrayCleared = true;
				return emptySubArray;
			}
			if (size < 0)
			{
				throw new ArgumentOutOfRangeException("size", "Must be positive.");
			}
			lock (this)
			{
				int num = BinarySearch(poolKeys, arraysCount, size);
				int num2 = ((num >= 0) ? num : (exactSize ? arraysCount : (~num)));
				T[] array;
				if (num2 < arraysCount)
				{
					array = RemoveElementAt(num2);
					if (clearArray)
					{
						Array.Clear(array, 0, array.Length);
					}
					isArrayCleared = clearArray;
				}
				else
				{
					if (LogAllocations)
					{
						Debug.Log(string.Format("[ArrayPools] Type: {0}. Allocated array size {1}. The size of the biggest array available is {2}", typeof(T).Name, size, (arraysCount == 0) ? "None" : poolKeys[arraysCount - 1].ToString()));
					}
					array = new T[size];
					isArrayCleared = true;
				}
				return new SubArray<T>(array, size);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void ApplyCapacity(long capacity)
		{
			while (elementsCount > capacity)
			{
				RemoveElementAt(random.Next(0, arraysCount));
			}
		}

		private T[] RemoveElementAt(int elementIndex)
		{
			T[] array = poolValues[elementIndex];
			arraysCount--;
			if (elementIndex < arraysCount)
			{
				Array.Copy(poolKeys, elementIndex + 1, poolKeys, elementIndex, arraysCount - elementIndex);
				Array.Copy(poolValues, elementIndex + 1, poolValues, elementIndex, arraysCount - elementIndex);
			}
			elementsCount -= array.Length;
			return array;
		}

		private static int BinarySearch(int[] array, int length, int value)
		{
			int num = 0;
			int num2 = length - 1;
			while (num <= num2)
			{
				int num3 = num + (num2 - num >> 1);
				int num4 = array[num3] - value;
				if (num4 == 0)
				{
					return num3;
				}
				if (num4 < 0)
				{
					num = num3 + 1;
				}
				else
				{
					num2 = num3 - 1;
				}
			}
			return ~num;
		}
	}
}
