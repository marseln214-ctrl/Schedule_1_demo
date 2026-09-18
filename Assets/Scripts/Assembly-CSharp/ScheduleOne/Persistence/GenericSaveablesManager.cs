using System;
using System.Collections.Generic;
using System.IO;
using ScheduleOne.DevUtilities;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.Persistence.Loaders;

namespace ScheduleOne.Persistence
{
	public class GenericSaveablesManager : Singleton<GenericSaveablesManager>, IBaseSaveable, ISaveable
	{
		protected List<IGenericSaveable> Saveables = new List<IGenericSaveable>();

		private GenericSaveablesLoader loader = new GenericSaveablesLoader();

		public string SaveFolderName => "GenericSaveables";

		public string SaveFileName => "GenericSaveables";

		public Loader Loader => loader;

		public bool ShouldSaveUnderFolder => true;

		public List<string> LocalExtraFiles { get; set; } = new List<string>();


		public List<string> LocalExtraFolders { get; set; } = new List<string>();


		public bool HasChanged { get; set; }

		protected override void Awake()
		{
			base.Awake();
			InitializeSaveable();
		}

		public virtual void InitializeSaveable()
		{
			Singleton<SaveManager>.Instance.RegisterSaveable(this);
		}

		public void RegisterSaveable(IGenericSaveable saveable)
		{
			if (!Saveables.Contains(saveable))
			{
				Saveables.Add(saveable);
			}
		}

		public virtual string GetSaveString()
		{
			return string.Empty;
		}

		public virtual List<string> WriteData(string parentFolderPath)
		{
			List<string> list = new List<string>();
			string containerFolder = ((ISaveable)this).GetContainerFolder(parentFolderPath);
			for (int i = 0; i < Saveables.Count; i++)
			{
				if (Saveables[i] != null)
				{
					string json = Saveables[i].GetSaveData().GetJson();
					string text = Saveables[i].GUID.ToString().Substring(0, 6) + ".json";
					list.Add(text);
					string text2 = Path.Combine(containerFolder, text);
					try
					{
						File.WriteAllText(text2, json);
					}
					catch (Exception ex)
					{
						Console.LogWarning("Failed to write generic saveable file: " + text2 + " - " + ex.Message);
					}
				}
			}
			return list;
		}

		public void LoadSaveable(GenericSaveData data)
		{
			if (!GUIDManager.IsGUIDValid(data.GUID))
			{
				Console.LogWarning("Invalid GUID found in generic save data: " + data.GUID);
				return;
			}
			Guid guid = new Guid(data.GUID);
			IGenericSaveable genericSaveable = Saveables.Find((IGenericSaveable x) => x.GUID == guid);
			if (genericSaveable == null)
			{
				Guid guid2 = guid;
				Console.LogWarning("No saveable found with GUID: " + guid2.ToString());
			}
			else
			{
				genericSaveable.Load(data);
			}
		}
	}
}
