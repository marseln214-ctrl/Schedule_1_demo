using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ScheduleOne.DevUtilities;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.Property;
using UnityEngine;

namespace ScheduleOne.Persistence.Loaders
{
	public class PropertyLoader : Loader
	{
		public override void Load(string mainPath)
		{
			if (TryLoadFile(mainPath, "Property", out var contents) || TryLoadFile(mainPath, "Business", out contents))
			{
				PropertyData propertyData = null;
				try
				{
					propertyData = JsonUtility.FromJson<PropertyData>(contents);
				}
				catch (Exception ex)
				{
					Console.LogError(GetType()?.ToString() + " error reading data: " + ex);
				}
				if (propertyData != null)
				{
					Singleton<PropertyManager>.Instance.LoadProperty(propertyData, mainPath);
				}
			}
			string text = Path.Combine(mainPath, "Objects");
			if (Directory.Exists(text))
			{
				List<string> list = new List<string>();
				Dictionary<string, int> objectPriorities = new Dictionary<string, int>();
				BuildableItemLoader buildableItemLoader = new BuildableItemLoader();
				List<DirectoryInfo> directories = GetDirectories(text);
				for (int i = 0; i < directories.Count; i++)
				{
					BuildableItemData buildableItemData = buildableItemLoader.GetBuildableItemData(directories[i].FullName);
					if (buildableItemData != null)
					{
						list.Add(directories[i].FullName);
						objectPriorities.Add(directories[i].FullName, buildableItemData.LoadOrder);
					}
				}
				list = list.OrderBy((string x) => objectPriorities[x]).ToList();
				for (int j = 0; j < list.Count; j++)
				{
					new LoadRequest(list[j], buildableItemLoader);
				}
			}
			string text2 = Path.Combine(mainPath, "Employees");
			if (!Directory.Exists(text2))
			{
				return;
			}
			List<DirectoryInfo> directories2 = GetDirectories(text2);
			for (int k = 0; k < directories2.Count; k++)
			{
				if (TryLoadFile(directories2[k].FullName, "NPC", out var contents2))
				{
					NPCData nPCData = null;
					try
					{
						nPCData = JsonUtility.FromJson<NPCData>(contents2);
					}
					catch (Exception ex2)
					{
						Console.LogWarning("Failed to load NPC data from " + directories2[k].FullName + "\n Exception: " + ex2);
						continue;
					}
					NPCLoader nPCLoader = Singleton<LoadManager>.Instance.GetNPCLoader(nPCData.DataType);
					if (nPCLoader != null)
					{
						new LoadRequest(directories2[k].FullName, nPCLoader);
					}
				}
			}
		}
	}
}
