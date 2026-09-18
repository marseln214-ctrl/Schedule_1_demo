using System;
using System.Collections.Generic;
using FluffyUnderware.DevTools.Extensions;
using JetBrains.Annotations;
using UnityEngine;

namespace FluffyUnderware.DevTools
{
	public abstract class UnityObjectPool<T> : DTVersionedMonoBehaviour, IPool where T : UnityEngine.Object
	{
		[NotNull]
		private readonly List<T> pooledObjects = new List<T>();

		[Inline]
		[SerializeField]
		[NotNull]
		private PoolSettings m_Settings = new PoolSettings();

		private double lastProcessingTime;

		private double unprocessedDuration;

		public virtual PoolSettings Settings
		{
			get
			{
				return m_Settings;
			}
			[UsedImplicitly]
			[Obsolete("The setter will be made private. Rather than assigning a new Settings instance, modify the existing one")]
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException("value");
				}
				m_Settings = value;
				m_Settings.Validate();
			}
		}

		[UsedImplicitly]
		[Obsolete("Use GetComponent<PoolManager>() instead")]
		public PoolManager Manager => GetComponent<PoolManager>();

		public int Count => pooledObjects.Count;

		public abstract string Identifier { get; set; }

		private static double Now => DTTime.TimeSinceStartup;

		public virtual void Push(T item)
		{
			if (!(item == null))
			{
				if (item is IPoolable poolable)
				{
					poolable.OnBeforePush();
				}
				GameObject itemGameObject = GetItemGameObject(item);
				if (Application.isPlaying)
				{
					pooledObjects.Add(item);
					ConfigurePushedGameObject(itemGameObject);
				}
				else
				{
					itemGameObject.Destroy(isUndoable: false, doPrefabCheck: true);
				}
				if (Settings.Debug)
				{
					LogMessage("Push " + item);
				}
			}
		}

		[NotNull]
		public virtual T Pop(Transform parent = null)
		{
			T val = RetrievedPoppedItem();
			GameObject itemGameObject = GetItemGameObject(val);
			ConfigurePoppedGameObject(itemGameObject, parent);
			if (val is IPoolable poolable)
			{
				poolable.OnAfterPop();
			}
			if (Settings.Debug)
			{
				LogMessage("Pop " + val);
			}
			return val;
		}

		public virtual void Clear()
		{
			if (Settings.Debug)
			{
				LogMessage("Clear");
			}
			for (int i = 0; i < Count; i++)
			{
				DestroyObject(pooledObjects[i]);
			}
			pooledObjects.Clear();
		}

		public void Update()
		{
			if (Application.isPlaying)
			{
				int adjustmentsCount = GetAdjustmentsCount();
				AdjustItemsCount(Settings.MinimumCount, Settings.MaximumCount, adjustmentsCount, Settings.Debug);
			}
		}

		public new void Reset()
		{
			base.Reset();
			Settings.SetToDefault();
			InstantShit();
		}

		[NotNull]
		protected abstract T CreateObject();

		[NotNull]
		protected abstract GameObject GetItemGameObject([NotNull] T item);

		protected override void OnValidate()
		{
			base.OnValidate();
			Settings.Validate();
		}

		protected override void ResetOnEnable()
		{
			base.ResetOnEnable();
			ResetTimeRelatedFields();
		}

		protected void Initialize([NotNull] PoolSettings settings)
		{
			Settings = settings;
			ResetTimeRelatedFields();
			if (Settings.InitializeCountConstrained)
			{
				InstantShit();
			}
		}

		protected void ConfigureCreatedGameObject([NotNull] GameObject item, string itemName)
		{
			item.name = itemName;
			item.transform.parent = base.transform;
			if (Settings.AutoEnableDisable)
			{
				item.SetActive(value: false);
			}
		}

		[UsedImplicitly]
		private void Start()
		{
			if (Settings.InitializeCountConstrained)
			{
				InstantShit();
			}
		}

		private void DestroyObject([CanBeNull] T item)
		{
			if (!(item == null))
			{
				GetItemGameObject(item).Destroy(isUndoable: false, doPrefabCheck: true);
			}
		}

		[NotNull]
		private T RetrievedPoppedItem()
		{
			T val = null;
			while (val == null && Count > 0)
			{
				val = pooledObjects[0];
				pooledObjects.RemoveAt(0);
			}
			if (val == null)
			{
				if (!Settings.AutoCreate && Application.isPlaying)
				{
					throw new InvalidOperationException($"[Curvy] Could not pop element of type {typeof(T)} from pool. This is because there are not enough elements in the pool, and AutoCreate is not set to true neither. The pool identifier is {Identifier}");
				}
				val = CreateObject();
				if (Settings.Debug)
				{
					LogMessage("Auto create item");
				}
			}
			return val;
		}

		private void ConfigurePushedGameObject([NotNull] GameObject item)
		{
			item.hideFlags = (Settings.Debug ? HideFlags.DontSave : HideFlags.HideAndDontSave);
			if (Settings.AutoEnableDisable)
			{
				item.SetActive(value: false);
			}
			item.transform.parent = base.transform;
		}

		private void ConfigurePoppedGameObject([NotNull] GameObject item, [CanBeNull] Transform parent)
		{
			item.transform.parent = parent;
			item.hideFlags = HideFlags.None;
			if (Settings.AutoEnableDisable)
			{
				item.SetActive(value: true);
			}
		}

		private void LogMessage(string message)
		{
			Debug.Log($"({Count} items) {message} [{Identifier}]");
		}

		private void AdjustItemsCount(int minItemsCount, int maxItemsCount, int maxAdjustmentsCount, bool logOperations)
		{
			if (maxAdjustmentsCount < 0)
			{
				throw new ArgumentOutOfRangeException("maxAdjustmentsCount");
			}
			if (minItemsCount < 0)
			{
				throw new ArgumentOutOfRangeException("minItemsCount");
			}
			if (maxItemsCount < minItemsCount)
			{
				throw new ArgumentOutOfRangeException("maxItemsCount");
			}
			if (Count > maxItemsCount)
			{
				maxAdjustmentsCount = Mathf.Min(maxAdjustmentsCount, Count - maxItemsCount);
				while (maxAdjustmentsCount-- > 0)
				{
					if (logOperations)
					{
						LogMessage("MaximumCount exceeded: Deleting item");
					}
					DestroyObject(pooledObjects[0]);
					pooledObjects.RemoveAt(0);
				}
			}
			else
			{
				if (Count >= minItemsCount)
				{
					return;
				}
				maxAdjustmentsCount = Mathf.Min(maxAdjustmentsCount, minItemsCount - Count);
				while (maxAdjustmentsCount-- > 0)
				{
					if (logOperations)
					{
						LogMessage("Below MinimumCount: Adding item");
					}
					pooledObjects.Add(CreateObject());
				}
			}
		}

		private void InstantShit()
		{
			if (Application.isPlaying)
			{
				AdjustItemsCount(Settings.MinimumCount, Settings.MaximumCount, int.MaxValue, logOperations: false);
				if (Settings.Debug)
				{
					LogMessage("Instant adjustment");
				}
			}
		}

		[UsedImplicitly]
		private void ResetTimeRelatedFields()
		{
			lastProcessingTime = Now;
			unprocessedDuration = 0.0;
		}

		private int GetAdjustmentsCount()
		{
			double num = unprocessedDuration + (Now - lastProcessingTime);
			float countAdjustmentInterval = Settings.CountAdjustmentInterval;
			int result;
			if (countAdjustmentInterval > 0f)
			{
				result = (int)Math.Floor(num / (double)countAdjustmentInterval);
				unprocessedDuration = num % (double)countAdjustmentInterval;
			}
			else
			{
				result = int.MaxValue;
				unprocessedDuration = 0.0;
			}
			lastProcessingTime = Now;
			return result;
		}
	}
}
