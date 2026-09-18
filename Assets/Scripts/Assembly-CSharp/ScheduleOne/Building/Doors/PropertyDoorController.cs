using ScheduleOne.Doors;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Police;
using ScheduleOne.Property;
using UnityEngine;

namespace ScheduleOne.Building.Doors
{
	public class PropertyDoorController : DoorController
	{
		public const float WANTED_PLAYER_CLOSE_DISTANCE = 20f;

		public ScheduleOne.Property.Property Property;

		private bool IsUnlocked;

		private bool NetworkInitialize___EarlyScheduleOne_002EBuilding_002EDoors_002EPropertyDoorControllerAssembly_002DCSharp_002Edll_Excuted;

		private bool NetworkInitialize__LateScheduleOne_002EBuilding_002EDoors_002EPropertyDoorControllerAssembly_002DCSharp_002Edll_Excuted;

		public override void Awake()
		{
			NetworkInitialize___Early();
			Awake_UserLogic_ScheduleOne_002EBuilding_002EDoors_002EPropertyDoorController_Assembly_002DCSharp_002Edll();
			NetworkInitialize__Late();
		}

		public void Unlock()
		{
			PlayerAccess = EDoorAccess.Open;
			IsUnlocked = true;
		}

		private void CheckClose()
		{
			if (base.IsOpen && IsUnlocked && !(base.timeInCurrentState < 2f))
			{
				Player nearestWantedPlayer = GetNearestWantedPlayer();
				if (!(nearestWantedPlayer == null) && Vector3.Distance(base.transform.position, nearestWantedPlayer.Avatar.CenterPoint) < 20f)
				{
					SetIsOpen_Server(open: false, EDoorSide.Interior, openedForPlayer: false);
				}
			}
		}

		protected override bool CanPlayerAccess(EDoorSide side, out string reason)
		{
			if (side == EDoorSide.Exterior)
			{
				Player nearestWantedPlayer = GetNearestWantedPlayer();
				if (nearestWantedPlayer != null && Vector3.Distance(nearestWantedPlayer.transform.position, base.transform.position) < 15f)
				{
					PoliceOfficer nearestOfficer = nearestWantedPlayer.CrimeData.NearestOfficer;
					float num = 100000f;
					if (nearestOfficer != null)
					{
						num = Vector3.Distance(nearestOfficer.Avatar.CenterPoint, nearestWantedPlayer.Avatar.CenterPoint);
					}
					if (nearestWantedPlayer.CrimeData.TimeSinceSighted < 5f || num < 15f)
					{
						reason = "Police are nearby!";
						return false;
					}
				}
			}
			return base.CanPlayerAccess(side, out reason);
		}

		private Player GetNearestWantedPlayer()
		{
			Player player = null;
			for (int i = 0; i < Player.PlayerList.Count; i++)
			{
				if (Player.PlayerList[i].CrimeData.CurrentPursuitLevel != 0 && (player == null || Vector3.Distance(base.transform.position, Player.PlayerList[i].Avatar.CenterPoint) < Vector3.Distance(base.transform.position, player.Avatar.CenterPoint)))
				{
					player = Player.PlayerList[i];
				}
			}
			return player;
		}

		public override void NetworkInitialize___Early()
		{
			if (!NetworkInitialize___EarlyScheduleOne_002EBuilding_002EDoors_002EPropertyDoorControllerAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize___EarlyScheduleOne_002EBuilding_002EDoors_002EPropertyDoorControllerAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize___Early();
			}
		}

		public override void NetworkInitialize__Late()
		{
			if (!NetworkInitialize__LateScheduleOne_002EBuilding_002EDoors_002EPropertyDoorControllerAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize__LateScheduleOne_002EBuilding_002EDoors_002EPropertyDoorControllerAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize__Late();
			}
		}

		public override void NetworkInitializeIfDisabled()
		{
			NetworkInitialize___Early();
			NetworkInitialize__Late();
		}

		protected virtual void Awake_UserLogic_ScheduleOne_002EBuilding_002EDoors_002EPropertyDoorController_Assembly_002DCSharp_002Edll()
		{
			base.Awake();
			PlayerAccess = EDoorAccess.ExitOnly;
			if (Property != null)
			{
				Property.onThisPropertyAcquired.AddListener(Unlock);
			}
			InvokeRepeating("CheckClose", 0f, 1f);
		}
	}
}
