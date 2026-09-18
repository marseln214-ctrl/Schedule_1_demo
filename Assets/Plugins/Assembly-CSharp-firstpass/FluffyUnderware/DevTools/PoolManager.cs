using System;
using System.Collections.Generic;
using FluffyUnderware.DevTools.Extensions;
using JetBrains.Annotations;
using UnityEngine;

namespace FluffyUnderware.DevTools
{
	[HelpURL("https://curvyeditor.com/doclink/dtpoolmanager")]
	[ExecuteAlways]
	[DisallowMultipleComponent]
	public class PoolManager : DTVersionedMonoBehaviour
	{
		[Section("General", true, false, 100)]
		[SerializeField]
		[Tooltip("Whether pools should be automatically created when requested but not found")]
		private bool m_AutoCreatePools = true;

		[AsGroup(null, Expanded = false)]
		[SerializeField]
		private PoolSettings m_DefaultSettings = new PoolSettings();

		public Dictionary<string, IPool> Pools = new Dictionary<string, IPool>();

		[UsedImplicitly]
		[Obsolete("TypePools are no more part of Curvy Splines")]
		public Dictionary<Type, IPool> TypePools = new Dictionary<Type, IPool>();

		[UsedImplicitly]
		[Obsolete]
		private IPool[] mPools = new IPool[0];

		public bool AutoCreatePools
		{
			get
			{
				return m_AutoCreatePools;
			}
			set
			{
				if (m_AutoCreatePools != value)
				{
					m_AutoCreatePools = value;
				}
			}
		}

		public PoolSettings DefaultSettings
		{
			get
			{
				return m_DefaultSettings;
			}
			set
			{
				if (m_DefaultSettings != value)
				{
					m_DefaultSettings = value;
				}
				if (m_DefaultSettings != null)
				{
					m_DefaultSettings.Validate();
				}
			}
		}

		public bool IsInitialized { get; private set; }

		public int Count => Pools.Count + TypePools.Count;

		protected override void OnValidate()
		{
			base.OnValidate();
			DefaultSettings = m_DefaultSettings;
		}

		protected override void ResetOnEnable()
		{
			base.ResetOnEnable();
			IsInitialized = false;
		}

		protected override void OnDisable()
		{
			base.OnDisable();
			IsInitialized = false;
		}

		[UsedImplicitly]
		private void Update()
		{
			if (!IsInitialized)
			{
				Initialize();
			}
			if (mPools.Length != TypePools.Count)
			{
				Array.Resize(ref mPools, TypePools.Count);
				TypePools.Values.CopyTo(mPools, 0);
			}
			for (int i = 0; i < mPools.Length; i++)
			{
				mPools[i].Update();
			}
		}

		private void Initialize()
		{
			Pools.Clear();
			IPool[] components = GetComponents<IPool>();
			foreach (IPool pool in components)
			{
				if (pool is ComponentPool)
				{
					if (!Pools.ContainsKey(pool.Identifier))
					{
						Pools.Add(pool.Identifier, pool);
						continue;
					}
					DTLog.Log("[DevTools] Found a duplicated ComponentPool for type " + pool.Identifier + ". The duplicated pool will be destroyed", this);
					(pool as ComponentPool).Destroy(isUndoable: false, doPrefabCheck: false);
				}
				else
				{
					pool.Identifier = GetUniqueIdentifier(pool.Identifier);
					Pools.Add(pool.Identifier, pool);
				}
			}
			IsInitialized = true;
		}

		[NotNull]
		public string GetUniqueIdentifier([NotNull] string ident)
		{
			int num = 0;
			string text = ident;
			while (Pools.ContainsKey(text))
			{
				int num2 = ++num;
				text = ident + num2;
			}
			return text;
		}

		[UsedImplicitly]
		[Obsolete("TypePools are no more part of Curvy Splines")]
		public Pool<T> GetTypePool<T>()
		{
			if (!TypePools.TryGetValue(typeof(T), out var value) && AutoCreatePools)
			{
				value = CreateTypePool<T>();
			}
			return (Pool<T>)value;
		}

		public ComponentPool GetComponentPool<T>() where T : Component
		{
			if (!IsInitialized)
			{
				Initialize();
			}
			if (!Pools.TryGetValue(typeof(T).AssemblyQualifiedName, out var value) && AutoCreatePools)
			{
				value = CreateComponentPool<T>();
			}
			return (ComponentPool)value;
		}

		public PrefabPool GetPrefabPool([NotNull] string identifier, params GameObject[] prefabs)
		{
			if (!IsInitialized)
			{
				Initialize();
			}
			if (!Pools.TryGetValue(identifier, out var value) && AutoCreatePools)
			{
				value = CreatePrefabPool(identifier, null, prefabs);
			}
			return (PrefabPool)value;
		}

		[UsedImplicitly]
		[Obsolete("TypePools are no more part of Curvy Splines")]
		public Pool<T> CreateTypePool<T>(PoolSettings settings = null)
		{
			PoolSettings settings2 = settings ?? new PoolSettings(DefaultSettings);
			if (!TypePools.TryGetValue(typeof(T), out var value))
			{
				value = new Pool<T>(settings2);
				TypePools.Add(typeof(T), value);
			}
			return (Pool<T>)value;
		}

		public ComponentPool CreateComponentPool<T>(PoolSettings settings = null) where T : Component
		{
			if (!IsInitialized)
			{
				Initialize();
			}
			PoolSettings settings2 = settings ?? new PoolSettings(DefaultSettings);
			if (!Pools.TryGetValue(typeof(T).AssemblyQualifiedName, out var value))
			{
				value = base.gameObject.AddComponent<ComponentPool>();
				((ComponentPool)value).Initialize(typeof(T), settings2);
				Pools.Add(value.Identifier, value);
			}
			return (ComponentPool)value;
		}

		public PrefabPool CreatePrefabPool([NotNull] string name, PoolSettings settings = null, params GameObject[] prefabs)
		{
			if (!IsInitialized)
			{
				Initialize();
			}
			PoolSettings settings2 = settings ?? new PoolSettings(DefaultSettings);
			if (!Pools.TryGetValue(name, out var value))
			{
				PrefabPool prefabPool = base.gameObject.AddComponent<PrefabPool>();
				prefabPool.Initialize(name, settings2, prefabs);
				Pools.Add(name, prefabPool);
				return prefabPool;
			}
			return (PrefabPool)value;
		}

		public List<IPool> FindPools(string identifierStartsWith)
		{
			List<IPool> list = new List<IPool>();
			foreach (KeyValuePair<string, IPool> pool in Pools)
			{
				if (pool.Key.StartsWith(identifierStartsWith))
				{
					list.Add(pool.Value);
				}
			}
			return list;
		}

		public void DeletePools(string startsWith)
		{
			List<IPool> list = FindPools(startsWith);
			for (int num = list.Count - 1; num >= 0; num--)
			{
				DeletePool(list[num]);
			}
		}

		public void DeletePool(IPool pool)
		{
			if (pool is PrefabPool || pool is ComponentPool)
			{
				((MonoBehaviour)pool).Destroy(isUndoable: false, doPrefabCheck: false);
				Pools.Remove(pool.Identifier);
			}
		}

		[UsedImplicitly]
		[Obsolete("TypePools are no more part of Curvy Splines")]
		public void DeletePool<T>()
		{
			TypePools.Remove(typeof(T));
		}
	}
}
