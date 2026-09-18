using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace FluffyUnderware.DevTools
{
	[UsedImplicitly]
	[Obsolete]
	public class Pool<T> : IPool
	{
		private readonly List<T> mObjects = new List<T>();

		private double mLastTime;

		private double mDeltaTime;

		public string Identifier { get; set; }

		public PoolSettings Settings { get; protected set; }

		public Type Type => typeof(T);

		public int Count => mObjects.Count;

		public Pool(PoolSettings settings = null)
		{
			Settings = settings ?? new PoolSettings();
			Identifier = typeof(T).FullName;
			mLastTime = DTTime.TimeSinceStartup + (double)UnityEngine.Random.Range(0f, Settings.CountAdjustmentInterval);
			if (Settings.InitializeCountConstrained)
			{
				Reset();
			}
		}

		public void Update()
		{
			mDeltaTime += DTTime.TimeSinceStartup - mLastTime;
			mLastTime = DTTime.TimeSinceStartup;
			if (Settings.CountAdjustmentInterval > 0f)
			{
				int num = (int)(mDeltaTime / (double)Settings.CountAdjustmentInterval);
				mDeltaTime -= num;
				if (Count > Settings.MaximumCount)
				{
					num = Mathf.Min(num, Count - Settings.MaximumCount);
					while (num-- > 0)
					{
						destroy(mObjects[0]);
						mObjects.RemoveAt(0);
						log("MaximumCount exceeded: Deleting item");
					}
				}
				else if (Count < Settings.MinimumCount)
				{
					num = Mathf.Min(num, Settings.MinimumCount - Count);
					while (num-- > 0)
					{
						mObjects.Add(create());
						log("Below MinimumCount: Adding item");
					}
				}
			}
			else
			{
				mDeltaTime = 0.0;
			}
		}

		public void Reset()
		{
			if (Application.isPlaying)
			{
				while (Count < Settings.MinimumCount)
				{
					mObjects.Add(create());
				}
				while (Count > Settings.MaximumCount)
				{
					destroy(mObjects[0]);
					mObjects.RemoveAt(0);
				}
				log("Prewarm/Reset");
			}
		}

		public void Clear()
		{
			log("Clear");
			for (int i = 0; i < Count; i++)
			{
				destroy(mObjects[i]);
			}
			mObjects.Clear();
		}

		public virtual T Pop(Transform parent = null)
		{
			T val = default(T);
			if (Count > 0)
			{
				val = mObjects[0];
				mObjects.RemoveAt(0);
			}
			else if (Settings.AutoCreate || !Application.isPlaying)
			{
				log("Auto create item");
				val = create();
			}
			if (val != null)
			{
				sendAfterPop(val);
				setParent(val, parent);
				T val2 = val;
				log("Pop " + val2);
			}
			return val;
		}

		public virtual void Push(T item)
		{
			T val = item;
			log("Push " + val);
			if (Application.isPlaying && item != null)
			{
				sendBeforePush(item);
				mObjects.Add(item);
			}
		}

		protected virtual void sendBeforePush(T item)
		{
			if (item is IPoolable poolable)
			{
				poolable.OnBeforePush();
			}
		}

		protected virtual void sendAfterPop(T item)
		{
			if (item is IPoolable poolable)
			{
				poolable.OnAfterPop();
			}
		}

		protected virtual void setParent(T item, Transform parent)
		{
		}

		protected virtual T create()
		{
			return Activator.CreateInstance<T>();
		}

		protected virtual void destroy(T item)
		{
		}

		private void log(string msg)
		{
			if (Settings.Debug)
			{
				Debug.Log($"[{Identifier}] ({Count} items) {msg}");
			}
		}
	}
}
