using FishNet;
using ScheduleOne.DevUtilities;
using ScheduleOne.GameTime;
using ScheduleOne.Police;
using UnityEngine;

namespace ScheduleOne.NPCs.Behaviour
{
	public class FootPatrolBehaviour : Behaviour
	{
		public const float MOVE_SPEED = 0.08f;

		public const int FLASHLIGHT_MIN_TIME = 1930;

		public int FLASHLIGHT_MAX_TIME = 500;

		public const string FLASHLIGHT_ASSET_PATH = "Tools/Flashlight/Flashlight_AvatarEquippable";

		public bool UseFlashlight = true;

		private bool flashlightEquipped;

		private bool NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EFootPatrolBehaviourAssembly_002DCSharp_002Edll_Excuted;

		private bool NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EFootPatrolBehaviourAssembly_002DCSharp_002Edll_Excuted;

		public PatrolGroup Group { get; protected set; }

		protected override void Begin()
		{
			base.Begin();
			if (InstanceFinder.IsServer && Group == null)
			{
				Console.LogError("Foot patrol behaviour started without a group!");
			}
			base.Npc.Movement.SpeedController.AddSpeedControl(new NPCSpeedController.SpeedControl("footpatrol", 1, 0.08f));
			(base.Npc as PoliceOfficer).BodySearchChance = 0.4f;
		}

		protected override void Resume()
		{
			base.Resume();
			if (InstanceFinder.IsServer && Group == null)
			{
				Console.LogError("Foot patrol behaviour resumed without a group!");
			}
			base.Npc.Movement.SpeedController.AddSpeedControl(new NPCSpeedController.SpeedControl("footpatrol", 1, 0.08f));
			(base.Npc as PoliceOfficer).BodySearchChance = 0.25f;
		}

		protected override void Pause()
		{
			base.Pause();
			base.Npc.Movement.SpeedController.RemoveSpeedControl("footpatrol");
			(base.Npc as PoliceOfficer).BodySearchChance = 0.1f;
			if (flashlightEquipped)
			{
				SetFlashlightEquipped(equipped: false);
			}
		}

		protected override void End()
		{
			base.End();
			if (Group != null)
			{
				Group.Members.Remove(base.Npc);
			}
			base.Npc.Movement.SpeedController.RemoveSpeedControl("footpatrol");
			if (flashlightEquipped)
			{
				SetFlashlightEquipped(equipped: false);
			}
			(base.Npc as PoliceOfficer).BodySearchChance = 0.1f;
		}

		public override void ActiveMinPass()
		{
			base.ActiveMinPass();
			if (!InstanceFinder.IsServer)
			{
				return;
			}
			if (NetworkSingleton<ScheduleOne.GameTime.TimeManager>.Instance.IsCurrentTimeWithinRange(1930, FLASHLIGHT_MAX_TIME))
			{
				if (UseFlashlight && !flashlightEquipped && Group.Members.Count > 0 && Group.Members[0] == base.Npc)
				{
					SetFlashlightEquipped(equipped: true);
				}
			}
			else if (flashlightEquipped)
			{
				SetFlashlightEquipped(equipped: false);
			}
		}

		private void SetFlashlightEquipped(bool equipped)
		{
			flashlightEquipped = equipped;
			if (equipped)
			{
				base.Npc.SetEquippable_Networked(null, "Tools/Flashlight/Flashlight_AvatarEquippable");
			}
			else
			{
				base.Npc.SetEquippable_Networked(null, string.Empty);
			}
		}

		public override void BehaviourUpdate()
		{
			base.BehaviourUpdate();
			if (!InstanceFinder.IsServer || Group == null)
			{
				return;
			}
			if (!Group.Members.Contains(base.Npc))
			{
				Console.LogWarning("Foot patrol behaviour is not in group members list! Adding now");
				SetGroup(Group);
			}
			if (Group.IsPaused())
			{
				if (base.Npc.Movement.IsMoving)
				{
					base.Npc.Movement.Stop();
				}
			}
			else
			{
				if (base.Npc.Movement.IsMoving)
				{
					return;
				}
				if (IsReadyToAdvance())
				{
					if (Group.Members.Count > 0 && Group.Members[0] == base.Npc && Group.IsGroupReadyToAdvance())
					{
						Group.AdvanceGroup();
					}
				}
				else
				{
					base.Npc.Movement.SetDestination(Group.GetDestination(base.Npc));
				}
			}
		}

		public void SetGroup(PatrolGroup group)
		{
			Group = group;
			Group.Members.Add(base.Npc);
		}

		public bool IsReadyToAdvance()
		{
			Vector3 destination = Group.GetDestination(base.Npc);
			if (Vector3.Distance(base.transform.position, destination) < 2f)
			{
				return true;
			}
			if (base.Npc.Movement.IsMoving)
			{
				return false;
			}
			if (base.Npc.Movement.IsAsCloseAsPossible(Group.GetDestination(base.Npc), 3f))
			{
				return true;
			}
			return false;
		}

		public override void NetworkInitialize___Early()
		{
			if (!NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EFootPatrolBehaviourAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EFootPatrolBehaviourAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize___Early();
			}
		}

		public override void NetworkInitialize__Late()
		{
			if (!NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EFootPatrolBehaviourAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EFootPatrolBehaviourAssembly_002DCSharp_002Edll_Excuted = true;
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
