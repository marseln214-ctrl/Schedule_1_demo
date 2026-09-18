using FishNet;
using ScheduleOne.Combat;
using ScheduleOne.Dialogue;
using ScheduleOne.Law;
using ScheduleOne.Noise;
using ScheduleOne.PlayerScripts;
using ScheduleOne.VoiceOver;
using UnityEngine;

namespace ScheduleOne.NPCs.Responses
{
	public class NPCResponses_Civilian : NPCResponses
	{
		public enum EAttackResponse
		{
			None = 0,
			Panic = 1,
			Flee = 2,
			CallPolice = 3,
			Fight = 4
		}

		public enum EThreatType
		{
			None = 0,
			AimedAt = 1,
			GunshotHeard = 2,
			ExplosionHeard = 3
		}

		private EAttackResponse currentThreatResponse;

		private float timeSinceLastThreat;

		protected override void Update()
		{
			base.Update();
			timeSinceLastThreat += Time.deltaTime;
			if (timeSinceLastThreat > 30f)
			{
				currentThreatResponse = EAttackResponse.None;
			}
		}

		public override void GunshotHeard(NoiseEvent gunshotSound)
		{
			base.GunshotHeard(gunshotSound);
			if (currentThreatResponse == EAttackResponse.None)
			{
				Player player = ((gunshotSound.source != null) ? gunshotSound.source.GetComponent<Player>() : null);
				timeSinceLastThreat = 0f;
				currentThreatResponse = GetThreatResponse(EThreatType.GunshotHeard, player);
				ExecuteThreatResponse(currentThreatResponse, player, new DischargeFirearm());
			}
		}

		public override void ExplosionHeard(NoiseEvent explosionSound)
		{
			base.ExplosionHeard(explosionSound);
			if (currentThreatResponse == EAttackResponse.None)
			{
				Player player = ((explosionSound.source != null) ? explosionSound.source.GetComponent<Player>() : null);
				timeSinceLastThreat = 0f;
				currentThreatResponse = GetThreatResponse(EThreatType.ExplosionHeard, player);
				ExecuteThreatResponse(currentThreatResponse, player);
			}
		}

		public override void PlayerFailedPickpocket(Player player)
		{
			base.PlayerFailedPickpocket(player);
			string line = base.npc.dialogueHandler.Database.GetLine(EDialogueModule.Reactions, "noticed_pickpocket");
			base.npc.dialogueHandler.ShowWorldspaceDialogue(line, 3f);
			base.npc.Avatar.EmotionManager.AddEmotionOverride("Angry", "noticed_pickpocket", 20f, 3);
			if (base.npc.Aggression > 0.5f && Random.value < base.npc.Aggression)
			{
				base.npc.behaviour.CombatBehaviour.SetTarget(null, player.NetworkObject);
				base.npc.behaviour.CombatBehaviour.Enable_Networked(null);
				base.npc.VoiceOverEmitter.Play(EVOLineType.Angry);
				return;
			}
			float value = Random.value;
			if (value > 0.3f)
			{
				base.actions.SetCallPoliceBehaviourCrime(new Theft());
				base.actions.CallPolice_Networked(player);
				base.npc.PlayVO(EVOLineType.Alerted);
			}
			else if (value > 0.1f)
			{
				base.npc.PlayVO(EVOLineType.Alerted);
				base.npc.behaviour.FacePlayerBehaviour.SetTarget(player.NetworkObject);
				base.npc.behaviour.FacePlayerBehaviour.SendEnable();
			}
			else
			{
				base.npc.PlayVO(EVOLineType.Alerted);
				base.npc.behaviour.FleeBehaviour.SetEntityToFlee(player.NetworkObject);
				base.npc.behaviour.FleeBehaviour.Enable_Networked(null);
			}
		}

		protected override void RespondToFirstNonLethalAttack(Player perpetrator, Impact impact)
		{
			base.RespondToFirstNonLethalAttack(perpetrator, impact);
			if (base.npc.Aggression > 0.5f && Random.value < base.npc.Aggression)
			{
				base.npc.behaviour.CombatBehaviour.SetTarget(null, perpetrator.NetworkObject);
				base.npc.behaviour.CombatBehaviour.Enable_Networked(null);
				base.npc.VoiceOverEmitter.Play(EVOLineType.Angry);
				return;
			}
			base.npc.dialogueHandler.PlayReaction("hurt", 2.5f, network: false);
			base.npc.Avatar.EmotionManager.AddEmotionOverride("Annoyed", "hurt", 20f, 3);
			if (InstanceFinder.IsServer)
			{
				base.npc.behaviour.FacePlayerBehaviour.SetTarget(perpetrator.NetworkObject);
				base.npc.behaviour.FacePlayerBehaviour.SendEnable();
			}
		}

		protected override void RespondToAnnoyingImpact(Player perpetrator, Impact impact)
		{
			base.RespondToAnnoyingImpact(perpetrator, impact);
			if (base.npc.Aggression > 0.6f && Random.value * 1.5f < base.npc.Aggression)
			{
				base.npc.behaviour.CombatBehaviour.SetTarget(null, perpetrator.NetworkObject);
				base.npc.behaviour.CombatBehaviour.Enable_Networked(null);
				base.npc.VoiceOverEmitter.Play(EVOLineType.Angry);
				return;
			}
			base.npc.VoiceOverEmitter.Play(EVOLineType.Annoyed);
			base.npc.dialogueHandler.PlayReaction("annoyed", 2.5f, network: false);
			base.npc.Avatar.EmotionManager.AddEmotionOverride("Annoyed", "annoyed", 20f, 3);
			if (InstanceFinder.IsServer)
			{
				base.npc.behaviour.FacePlayerBehaviour.SetTarget(perpetrator.NetworkObject);
				base.npc.behaviour.FacePlayerBehaviour.SendEnable();
			}
		}

		protected override void RespondToLethalAttack(Player perpetrator, Impact impact)
		{
			base.RespondToLethalAttack(perpetrator, impact);
			RespondToLethalOrRepeatedAttack(perpetrator, impact);
		}

		protected override void RespondToRepeatedNonLethalAttack(Player perpetrator, Impact impact)
		{
			base.RespondToRepeatedNonLethalAttack(perpetrator, impact);
			RespondToLethalOrRepeatedAttack(perpetrator, impact);
		}

		private void RespondToLethalOrRepeatedAttack(Player perpetrator, Impact impact)
		{
			float value = Random.value;
			float aggression = base.npc.Aggression;
			if (aggression > 0.5f && value < aggression)
			{
				base.npc.behaviour.CombatBehaviour.SetTarget(null, perpetrator.NetworkObject);
				base.npc.behaviour.CombatBehaviour.Enable_Networked(null);
				base.npc.VoiceOverEmitter.Play(EVOLineType.Angry);
				return;
			}
			if (value > 0.5f)
			{
				if (Impact.IsLethal(impact.ImpactType))
				{
					base.actions.SetCallPoliceBehaviourCrime(new DeadlyAssault());
				}
				else
				{
					base.actions.SetCallPoliceBehaviourCrime(new Assault());
				}
				base.actions.CallPolice_Networked(perpetrator);
				return;
			}
			base.npc.SetPanicked();
			base.npc.dialogueHandler.PlayReaction("panic_start", 3f, network: false);
			if (value > 0.2f)
			{
				base.npc.behaviour.FleeBehaviour.SetEntityToFlee(perpetrator.NetworkObject);
				base.npc.behaviour.FleeBehaviour.Enable_Networked(null);
			}
		}

		public override void RespondToAimedAt(Player player)
		{
			base.RespondToAimedAt(player);
			player.VisualState.ApplyState("aiming_at_npc", PlayerVisualState.EVisualState.Brandishing, 1.5f);
			if (currentThreatResponse == EAttackResponse.None)
			{
				timeSinceLastThreat = 0f;
				currentThreatResponse = GetThreatResponse(EThreatType.AimedAt, player);
				ExecuteThreatResponse(currentThreatResponse, player, new BrandishingWeapon());
			}
		}

		private void ExecuteThreatResponse(EAttackResponse response, Player target, Crime crime = null)
		{
			Console.Log(base.npc.fullName + " executing threat response: " + response.ToString() + " on target " + target);
			if (!(target == null))
			{
				switch (response)
				{
				case EAttackResponse.Panic:
					base.npc.SetPanicked();
					base.npc.dialogueHandler.PlayReaction("panic_start", 3f, network: false);
					break;
				case EAttackResponse.Flee:
					base.npc.SetPanicked();
					base.npc.dialogueHandler.PlayReaction("panic_start", 3f, network: false);
					base.npc.behaviour.FleeBehaviour.SetEntityToFlee(target.NetworkObject);
					base.npc.behaviour.FleeBehaviour.Enable_Networked(null);
					break;
				case EAttackResponse.CallPolice:
					base.actions.SetCallPoliceBehaviourCrime(crime);
					base.actions.CallPolice_Networked(target);
					break;
				case EAttackResponse.Fight:
					base.npc.behaviour.CombatBehaviour.SetTarget(null, target.NetworkObject);
					base.npc.behaviour.CombatBehaviour.Enable_Networked(null);
					break;
				}
			}
		}

		private EAttackResponse GetThreatResponse(EThreatType type, Player threatSource)
		{
			if (base.npc.CurrentVehicle != null)
			{
				return EAttackResponse.Panic;
			}
			switch (type)
			{
			case EThreatType.AimedAt:
				if (Random.Range(0f, 1f) < base.npc.Aggression)
				{
					return EAttackResponse.Fight;
				}
				if (threatSource != null && threatSource.CrimeData.CurrentPursuitLevel == PlayerCrimeData.EPursuitLevel.None)
				{
					return Random.Range(0, 2) switch
					{
						0 => EAttackResponse.Panic, 
						1 => EAttackResponse.Flee, 
						_ => EAttackResponse.CallPolice, 
					};
				}
				if (Random.value < 0.5f)
				{
					return EAttackResponse.Panic;
				}
				return EAttackResponse.Flee;
			case EThreatType.GunshotHeard:
				if (threatSource != null && threatSource.CrimeData.CurrentPursuitLevel == PlayerCrimeData.EPursuitLevel.None)
				{
					return Random.Range(0, 2) switch
					{
						0 => EAttackResponse.Panic, 
						1 => EAttackResponse.Flee, 
						_ => EAttackResponse.CallPolice, 
					};
				}
				if (Random.value < 0.5f)
				{
					return EAttackResponse.Panic;
				}
				return EAttackResponse.Flee;
			case EThreatType.ExplosionHeard:
				if (Random.value < 0.5f)
				{
					return EAttackResponse.Panic;
				}
				return EAttackResponse.Flee;
			default:
				Console.LogError("Unhandled threat type: " + type);
				break;
			case EThreatType.None:
				break;
			}
			return EAttackResponse.None;
		}
	}
}
