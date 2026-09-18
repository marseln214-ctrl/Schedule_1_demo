using System;
using ScheduleOne.DevUtilities;
using ScheduleOne.NPCs.Relation;
using ScheduleOne.Variables;
using UnityEngine;

namespace ScheduleOne.NPCs.CharacterClasses
{
	public class Lily : NPC
	{
		[Header("References")]
		public Transform TutorialScheduleGroup;

		public Transform RegularScheduleGroup;

		public Conditions TutorialConditions;

		private bool NetworkInitialize___EarlyScheduleOne_002ENPCs_002ECharacterClasses_002ELilyAssembly_002DCSharp_002Edll_Excuted;

		private bool NetworkInitialize__LateScheduleOne_002ENPCs_002ECharacterClasses_002ELilyAssembly_002DCSharp_002Edll_Excuted;

		public override void Awake()
		{
			NetworkInitialize___Early();
			Awake_UserLogic_ScheduleOne_002ENPCs_002ECharacterClasses_002ELily_Assembly_002DCSharp_002Edll();
			NetworkInitialize__Late();
		}

		private void Unlocked(NPCRelationData.EUnlockType type, bool b)
		{
			NetworkSingleton<VariableDatabase>.Instance.SetVariableValue("Lily_Unlocked", "true");
		}

		protected override void MinPass()
		{
			base.MinPass();
		}

		public override void NetworkInitialize___Early()
		{
			if (!NetworkInitialize___EarlyScheduleOne_002ENPCs_002ECharacterClasses_002ELilyAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize___EarlyScheduleOne_002ENPCs_002ECharacterClasses_002ELilyAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize___Early();
			}
		}

		public override void NetworkInitialize__Late()
		{
			if (!NetworkInitialize__LateScheduleOne_002ENPCs_002ECharacterClasses_002ELilyAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize__LateScheduleOne_002ENPCs_002ECharacterClasses_002ELilyAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize__Late();
			}
		}

		public override void NetworkInitializeIfDisabled()
		{
			NetworkInitialize___Early();
			NetworkInitialize__Late();
		}

		protected virtual void Awake_UserLogic_ScheduleOne_002ENPCs_002ECharacterClasses_002ELily_Assembly_002DCSharp_002Edll()
		{
			base.Awake();
			NPCRelationData relationData = RelationData;
			relationData.onUnlocked = (Action<NPCRelationData.EUnlockType, bool>)Delegate.Combine(relationData.onUnlocked, new Action<NPCRelationData.EUnlockType, bool>(Unlocked));
		}
	}
}
