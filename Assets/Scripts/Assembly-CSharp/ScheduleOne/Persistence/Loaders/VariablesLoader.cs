using System;
using System.IO;
using ScheduleOne.DevUtilities;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.Variables;
using UnityEngine;

namespace ScheduleOne.Persistence.Loaders
{
	public class VariablesLoader : Loader
	{
		public override void Load(string mainPath)
		{
			if (!Directory.Exists(mainPath))
			{
				return;
			}
			Console.Log("Loading variables");
			string[] files = Directory.GetFiles(mainPath);
			for (int i = 0; i < files.Length; i++)
			{
				if (TryLoadFile(files[i], out var contents, autoAddExtension: false))
				{
					VariableData variableData = null;
					try
					{
						variableData = JsonUtility.FromJson<VariableData>(contents);
					}
					catch (Exception ex)
					{
						Debug.LogError("Error loading quest data: " + ex.Message);
					}
					if (variableData != null)
					{
						NetworkSingleton<VariableDatabase>.Instance.Load(variableData);
					}
				}
			}
		}
	}
}
