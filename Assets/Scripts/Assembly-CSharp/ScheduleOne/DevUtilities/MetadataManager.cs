using System;
using System.Collections.Generic;
using ScheduleOne.Persistence;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.Persistence.Loaders;
using UnityEngine;

namespace ScheduleOne.DevUtilities
{
	public class MetadataManager : Singleton<MetadataManager>, IBaseSaveable, ISaveable
	{
		private MetadataLoader loader = new MetadataLoader();

		public DateTime CreationDate { get; protected set; }

		public string CreationVersion { get; protected set; } = string.Empty;


		public string SaveFolderName => "Metadata";

		public string SaveFileName => "Metadata";

		public Loader Loader => loader;

		public bool ShouldSaveUnderFolder => false;

		public List<string> LocalExtraFiles { get; set; } = new List<string>();


		public List<string> LocalExtraFolders { get; set; } = new List<string>();


		public bool HasChanged { get; set; }

		protected override void Awake()
		{
			base.Awake();
			InitializeSaveable();
			if (CreationVersion == string.Empty)
			{
				CreationVersion = Application.version;
			}
		}

		public virtual void InitializeSaveable()
		{
			Singleton<SaveManager>.Instance.RegisterSaveable(this);
		}

		public virtual string GetSaveString()
		{
			DateTime now = DateTime.Now;
			return new MetaData(new DateTimeData(CreationDate), new DateTimeData(now), CreationVersion, Application.version, playTutorial: false).GetJson();
		}

		public void Load(MetaData data)
		{
			CreationDate = data.CreationDate.GetDateTime();
			CreationVersion = data.CreationVersion;
			HasChanged = true;
		}
	}
}
