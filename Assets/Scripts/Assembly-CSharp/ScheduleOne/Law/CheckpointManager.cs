using FishNet.Connection;
using ScheduleOne.DevUtilities;
using ScheduleOne.Map;
using ScheduleOne.Police;
using UnityEngine;

namespace ScheduleOne.Law
{
	public class CheckpointManager : NetworkSingleton<CheckpointManager>
	{
		public enum ECheckpointLocation
		{
			Western = 0,
			Docks = 1,
			NorthResidential = 2,
			WestResidential = 3
		}

		[Header("References")]
		public RoadCheckpoint WesternCheckpoint;

		public RoadCheckpoint DocksCheckpoint;

		public RoadCheckpoint NorthResidentialCheckpoint;

		public RoadCheckpoint WestResidentialCheckpoint;

		private bool NetworkInitialize___EarlyScheduleOne_002ELaw_002ECheckpointManagerAssembly_002DCSharp_002Edll_Excuted;

		private bool NetworkInitialize__LateScheduleOne_002ELaw_002ECheckpointManagerAssembly_002DCSharp_002Edll_Excuted;

		public override void OnSpawnServer(NetworkConnection connection)
		{
			base.OnSpawnServer(connection);
			if (WesternCheckpoint.ActivationState == RoadCheckpoint.ECheckpointState.Enabled)
			{
				WesternCheckpoint.Enable(connection);
			}
			if (DocksCheckpoint.ActivationState == RoadCheckpoint.ECheckpointState.Enabled)
			{
				DocksCheckpoint.Enable(connection);
			}
			if (NorthResidentialCheckpoint.ActivationState == RoadCheckpoint.ECheckpointState.Enabled)
			{
				NorthResidentialCheckpoint.Enable(connection);
			}
			if (WestResidentialCheckpoint.ActivationState == RoadCheckpoint.ECheckpointState.Enabled)
			{
				WestResidentialCheckpoint.Enable(connection);
			}
		}

		public void SetCheckpointEnabled(ECheckpointLocation checkpoint, bool enabled, int requestedOfficers)
		{
			if (enabled)
			{
				GetCheckpoint(checkpoint).Enable(null);
				for (int i = 0; i < requestedOfficers; i++)
				{
					if (Singleton<ScheduleOne.Map.Map>.Instance.PoliceStation.OfficerPool.Count <= 0)
					{
						break;
					}
					Singleton<ScheduleOne.Map.Map>.Instance.PoliceStation.PullOfficer().AssignToCheckpoint(checkpoint);
				}
			}
			else
			{
				GetCheckpoint(checkpoint).Disable();
			}
		}

		public RoadCheckpoint GetCheckpoint(ECheckpointLocation loc)
		{
			return loc switch
			{
				ECheckpointLocation.Western => WesternCheckpoint, 
				ECheckpointLocation.Docks => DocksCheckpoint, 
				ECheckpointLocation.NorthResidential => NorthResidentialCheckpoint, 
				ECheckpointLocation.WestResidential => WestResidentialCheckpoint, 
				_ => null, 
			};
		}

		public override void NetworkInitialize___Early()
		{
			if (!NetworkInitialize___EarlyScheduleOne_002ELaw_002ECheckpointManagerAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize___EarlyScheduleOne_002ELaw_002ECheckpointManagerAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize___Early();
			}
		}

		public override void NetworkInitialize__Late()
		{
			if (!NetworkInitialize__LateScheduleOne_002ELaw_002ECheckpointManagerAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize__LateScheduleOne_002ELaw_002ECheckpointManagerAssembly_002DCSharp_002Edll_Excuted = true;
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
