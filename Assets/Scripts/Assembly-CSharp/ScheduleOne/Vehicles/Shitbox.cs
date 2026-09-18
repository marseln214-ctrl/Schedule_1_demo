using System;
using System.Collections.Generic;
using System.IO;
using ScheduleOne.Persistence;
using ScheduleOne.Persistence.Datas;
using UnityEngine;

namespace ScheduleOne.Vehicles
{
	public class Shitbox : LandVehicle
	{
		[Serializable]
		public class LoanSharkVisualsData : SaveData
		{
			public bool Enabled;

			public bool NoteVisible;
		}

		public LoanSharkCarVisuals LoanSharkVisuals;

		private bool NetworkInitialize___EarlyScheduleOne_002EVehicles_002EShitboxAssembly_002DCSharp_002Edll_Excuted;

		private bool NetworkInitialize__LateScheduleOne_002EVehicles_002EShitboxAssembly_002DCSharp_002Edll_Excuted;

		public override List<string> WriteData(string parentFolderPath)
		{
			List<string> list = new List<string>();
			if (LoanSharkVisuals != null && LoanSharkVisuals.BulletHoleDecals.activeSelf)
			{
				LoanSharkVisualsData loanSharkVisualsData = new LoanSharkVisualsData
				{
					Enabled = LoanSharkVisuals.BulletHoleDecals.activeSelf,
					NoteVisible = LoanSharkVisuals.Note.activeSelf
				};
				((ISaveable)this).WriteSubfile(parentFolderPath, "LoanSharkCarData", loanSharkVisualsData.GetJson());
				list.Add("LoanSharkCarData.json");
			}
			list.AddRange(base.WriteData(parentFolderPath));
			return list;
		}

		public override void Load(VehicleData data, string containerPath)
		{
			base.Load(data, containerPath);
			if (LoanSharkVisuals != null && File.Exists(Path.Combine(containerPath, "LoanSharkCarData.json")) && base.Loader.TryLoadFile(containerPath, "LoanSharkCarData", out var contents))
			{
				LoanSharkVisualsData loanSharkVisualsData = null;
				try
				{
					loanSharkVisualsData = JsonUtility.FromJson<LoanSharkVisualsData>(contents);
				}
				catch (Exception ex)
				{
					Console.LogWarning("Failed to deserialize LoanSharkVisualsData: " + ex.Message);
					return;
				}
				LoanSharkVisuals.Configure(loanSharkVisualsData.Enabled, loanSharkVisualsData.NoteVisible);
			}
		}

		public override void NetworkInitialize___Early()
		{
			if (!NetworkInitialize___EarlyScheduleOne_002EVehicles_002EShitboxAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize___EarlyScheduleOne_002EVehicles_002EShitboxAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize___Early();
			}
		}

		public override void NetworkInitialize__Late()
		{
			if (!NetworkInitialize__LateScheduleOne_002EVehicles_002EShitboxAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize__LateScheduleOne_002EVehicles_002EShitboxAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize__Late();
			}
		}

		public override void NetworkInitializeIfDisabled()
		{
			NetworkInitialize___Early();
			NetworkInitialize__Late();
		}

		public override void Awake()
		{
			NetworkInitialize___Early();
			base.Awake();
			NetworkInitialize__Late();
		}
	}
}
