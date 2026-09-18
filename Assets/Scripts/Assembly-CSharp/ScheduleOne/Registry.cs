using System;
using System.Collections.Generic;
using FishNet.Object;
using ScheduleOne.ConstructableScripts;
using ScheduleOne.DevUtilities;
using ScheduleOne.ItemFramework;
using ScheduleOne.Persistence;
using UnityEngine;

namespace ScheduleOne
{
	public class Registry : PersistentSingleton<Registry>
	{
		[Serializable]
		public class ObjectRegister
		{
			public string ID;

			public string AssetPath;

			public NetworkObject Prefab;
		}

		[Serializable]
		public class ItemRegister
		{
			public string ID;

			public string AssetPath;

			public ItemDefinition Definition;
		}

		[SerializeField]
		private List<ObjectRegister> ObjectRegistry = new List<ObjectRegister>();

		[SerializeField]
		private List<ItemRegister> ItemRegistry = new List<ItemRegister>();

		[SerializeField]
		private List<ItemRegister> ItemsAddedAtRuntime = new List<ItemRegister>();

		public static GameObject GetPrefab(string id)
		{
			return Singleton<Registry>.Instance.ObjectRegistry.Find((ObjectRegister x) => x.ID.ToLower() == id.ToString())?.Prefab.gameObject;
		}

		public static ItemDefinition GetItem(string ID)
		{
			return Singleton<Registry>.Instance._GetItem(ID);
		}

		public static T GetItem<T>(string ID) where T : ItemDefinition
		{
			return Singleton<Registry>.Instance._GetItem(ID) as T;
		}

		public ItemDefinition _GetItem(string ID)
		{
			if (string.IsNullOrEmpty(ID))
			{
				return null;
			}
			ItemRegister itemRegister = ItemRegistry.Find((ItemRegister x) => x.ID.ToLower() == ID.ToLower());
			if (itemRegister == null)
			{
				return null;
			}
			if (!Application.isEditor && !itemRegister.Definition.AvailableInDemo)
			{
				Console.LogError("Item '" + ID + "' is not available in demo!");
				return null;
			}
			return itemRegister.Definition;
		}

		public static Constructable GetConstructable(string id)
		{
			GameObject prefab = GetPrefab(id);
			if (!(prefab != null))
			{
				return null;
			}
			return prefab.GetComponent<Constructable>();
		}

		private static string RemoveAssetsAndPrefab(string originalString)
		{
			int num = originalString.IndexOf("Assets/");
			if (num != -1)
			{
				originalString = originalString.Substring(num + "Assets/".Length);
			}
			int num2 = originalString.LastIndexOf(".prefab");
			if (num2 != -1)
			{
				originalString = originalString.Substring(0, num2);
			}
			return originalString;
		}

		protected override void Start()
		{
			base.Start();
			Singleton<LoadManager>.Instance.onPreSceneChange.AddListener(RemoveRuntimeItems);
		}

		public void AddToRegistry(ItemDefinition item)
		{
			Console.Log("Adding " + item.ID + " to registry: " + item);
			ItemRegistry.Add(new ItemRegister
			{
				Definition = item,
				ID = item.ID,
				AssetPath = string.Empty
			});
			if (Application.isPlaying)
			{
				ItemsAddedAtRuntime.Add(new ItemRegister
				{
					Definition = item,
					ID = item.ID,
					AssetPath = string.Empty
				});
			}
		}

		public void RemoveRuntimeItems()
		{
			foreach (ItemRegister item in ItemsAddedAtRuntime)
			{
				RemoveFromRegistry(item.Definition);
			}
			ItemsAddedAtRuntime.Clear();
		}

		public void RemoveFromRegistry(ItemDefinition item)
		{
			ItemRegister itemRegister = ItemRegistry.Find((ItemRegister x) => x.Definition == item);
			if (itemRegister != null)
			{
				ItemRegistry.Remove(itemRegister);
			}
		}
	}
}
