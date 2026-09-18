using System.Collections.Generic;
using ScheduleOne.DevUtilities;
using ScheduleOne.ItemFramework;
using ScheduleOne.Trash;
using UnityEngine;

namespace ScheduleOne.StationFramework
{
	public class StationItem : MonoBehaviour
	{
		public List<ItemModule> Modules;

		public TrashItem TrashPrefab;

		public List<ItemModule> ActiveModules { get; protected set; } = new List<ItemModule>();


		protected virtual void Awake()
		{
			LayerUtility.SetLayerRecursively(base.gameObject, LayerMask.NameToLayer("Task"));
		}

		public virtual void Initialize(StorableItemDefinition itemDefinition)
		{
		}

		public void ActivateModule<T>() where T : ItemModule
		{
			ItemModule module = GetModule<T>();
			if (module == null)
			{
				Console.LogWarning(module.GetType().Name + " is not a valid module for " + base.name);
				return;
			}
			ActiveModules.Add(module);
			module.ActivateModule(this);
		}

		public void Destroy()
		{
			Object.Destroy(base.gameObject);
		}

		public bool HasModule<T>() where T : ItemModule
		{
			return Modules.Exists((ItemModule x) => x.GetType() == typeof(T));
		}

		public T GetModule<T>() where T : ItemModule
		{
			return (T)Modules.Find((ItemModule x) => x.GetType() == typeof(T));
		}
	}
}
