using ScheduleOne.NPCs.Behaviour;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Police;
using ScheduleOne.Vehicles;
using ScheduleOne.VoiceOver;
using UnityEngine;

namespace ScheduleOne.Dialogue
{
	public class DialogueHandler_Police : ControlledDialogueHandler
	{
		[Header("References")]
		public DialogueContainer CheckpointRequestDialogue;

		private PoliceOfficer officer;

		protected override void Awake()
		{
			base.Awake();
			officer = base.NPC as PoliceOfficer;
		}

		public override void Hovered()
		{
			base.Hovered();
		}

		public override void Interacted()
		{
			base.Interacted();
			if (CanTalk_Checkpoint())
			{
				officer.PlayVO(EVOLineType.Question);
				InitializeDialogue(CheckpointRequestDialogue.name);
			}
		}

		private bool CanTalk_Checkpoint()
		{
			if (officer.behaviour.activeBehaviour != null && officer.behaviour.activeBehaviour is CheckpointBehaviour && !(officer.behaviour.activeBehaviour as CheckpointBehaviour).IsSearching)
			{
				return true;
			}
			return false;
		}

		protected override int CheckBranch(string branchLabel)
		{
			if (branchLabel == "BRANCH_VEHICLE_EXISTS")
			{
				LandVehicle lastDrivenVehicle = Player.Local.LastDrivenVehicle;
				CheckpointBehaviour checkpointBehaviour = officer.behaviour.activeBehaviour as CheckpointBehaviour;
				if (lastDrivenVehicle != null && (checkpointBehaviour.Checkpoint.SearchArea1.vehicles.Contains(lastDrivenVehicle) || checkpointBehaviour.Checkpoint.SearchArea2.vehicles.Contains(lastDrivenVehicle)))
				{
					checkpointBehaviour.StartSearch(lastDrivenVehicle.NetworkObject, Player.Local.NetworkObject);
					return 1;
				}
				return 0;
			}
			return base.CheckBranch(branchLabel);
		}
	}
}
