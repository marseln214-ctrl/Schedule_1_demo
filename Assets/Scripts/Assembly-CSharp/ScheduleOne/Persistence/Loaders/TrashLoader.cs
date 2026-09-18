using System;
using System.IO;
using ScheduleOne.DevUtilities;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.Trash;
using UnityEngine;

namespace ScheduleOne.Persistence.Loaders
{
	public class TrashLoader : Loader
	{
		public override void Load(string mainPath)
		{
			if (!Directory.Exists(mainPath))
			{
				return;
			}
			string path = Path.Combine(mainPath, "Items");
			if (Directory.Exists(path))
			{
				string[] files = Directory.GetFiles(path);
				for (int i = 0; i < files.Length; i++)
				{
					if (!TryLoadFile(files[i], out var contents, autoAddExtension: false))
					{
						continue;
					}
					TrashItemData trashItemData = null;
					try
					{
						trashItemData = JsonUtility.FromJson<TrashItemData>(contents);
					}
					catch (Exception ex)
					{
						Debug.LogError("Error loading data: " + ex.Message);
					}
					if (trashItemData == null)
					{
						continue;
					}
					TrashItem trashItem = null;
					if (trashItemData.DataType == "TrashBagData")
					{
						TrashBagData trashBagData = null;
						try
						{
							trashBagData = JsonUtility.FromJson<TrashBagData>(contents);
						}
						catch (Exception ex2)
						{
							Debug.LogError("Error loading data: " + ex2.Message);
						}
						if (trashBagData != null)
						{
							trashItem = NetworkSingleton<TrashManager>.Instance.CreateTrashBag(trashBagData.TrashID, trashBagData.Position, trashBagData.Rotation, trashBagData.Contents, Vector3.zero, trashBagData.GUID, startKinematic: true);
						}
					}
					else
					{
						trashItem = NetworkSingleton<TrashManager>.Instance.CreateTrashItem(trashItemData.TrashID, trashItemData.Position, trashItemData.Rotation, Vector3.zero, trashItemData.GUID, startKinematic: true);
					}
					if (trashItem != null)
					{
						trashItem.HasChanged = false;
					}
				}
			}
			string path2 = Path.Combine(mainPath, "Generators");
			if (!Directory.Exists(path2))
			{
				return;
			}
			string[] files2 = Directory.GetFiles(path2);
			for (int j = 0; j < files2.Length; j++)
			{
				if (!TryLoadFile(files2[j], out var contents2, autoAddExtension: false))
				{
					continue;
				}
				TrashGeneratorData trashGeneratorData = null;
				try
				{
					trashGeneratorData = JsonUtility.FromJson<TrashGeneratorData>(contents2);
				}
				catch (Exception ex3)
				{
					Debug.LogError("Error loading data: " + ex3.Message);
				}
				if (trashGeneratorData == null)
				{
					continue;
				}
				TrashGenerator @object = GUIDManager.GetObject<TrashGenerator>(new Guid(trashGeneratorData.GUID));
				if (!(@object != null))
				{
					continue;
				}
				for (int k = 0; k < trashGeneratorData.GeneratedItems.Length; k++)
				{
					TrashItem object2 = GUIDManager.GetObject<TrashItem>(new Guid(trashGeneratorData.GeneratedItems[k]));
					if (object2 != null)
					{
						@object.AddGeneratedTrash(object2);
					}
				}
				@object.HasChanged = false;
			}
		}
	}
}
