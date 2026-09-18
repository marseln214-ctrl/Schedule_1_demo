using System.Collections;
using EasyButtons;
using FishNet;
using ScheduleOne.DevUtilities;
using ScheduleOne.NPCs.CharacterClasses;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Property;
using ScheduleOne.UI.Input;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace ScheduleOne.Quests
{
	public class Quest_WelcomeToHylandPoint : Quest
	{
		public QuestEntry ReturnToRVQuest;

		public QuestEntry ReadMessagesQuest;

		public RV RV;

		public UncleNelson Nelson;

		[Header("Settings")]
		public float ExplosionMaxDist = 25f;

		public float ExplosionMinDist = 50f;

		public UnityEvent onExplode;

		private bool exploded;

		private float cameraLookTime;

		protected override void MinPass()
		{
			base.MinPass();
			if (base.QuestState != EQuestState.Active)
			{
				return;
			}
			if (ReturnToRVQuest.State == EQuestState.Active && InstanceFinder.IsServer)
			{
				float distance;
				Player closestPlayer = Player.GetClosestPlayer(RV.transform.position, out distance);
				if (distance < ExplosionMinDist)
				{
					ReturnToRVQuest.Complete();
				}
				else if (distance < ExplosionMaxDist)
				{
					if (Vector3.Angle(closestPlayer.MimicCamera.forward, RV.transform.position - closestPlayer.MimicCamera.position) < 60f)
					{
						cameraLookTime += Time.deltaTime;
						if (cameraLookTime > 0.4f)
						{
							ReturnToRVQuest.Complete();
						}
					}
					else
					{
						cameraLookTime = 0f;
					}
				}
			}
			if (ReadMessagesQuest.State == EQuestState.Active && Nelson.MSGConversation != null && Nelson.MSGConversation.read)
			{
				ReadMessagesQuest.Complete();
			}
		}

		[Button]
		public void Explode()
		{
			Console.Log("RV exploding!");
			if (onExplode != null)
			{
				onExplode.Invoke();
			}
			StartCoroutine(Shake());
			static IEnumerator Shake()
			{
				yield return new WaitForSeconds(0.35f);
				PlayerSingleton<PlayerCamera>.Instance.StartCameraShake(2f, 1f);
			}
		}

		public override void SetQuestState(EQuestState state, bool network = true)
		{
			base.SetQuestState(state, network);
			if (state == EQuestState.Active)
			{
				Singleton<GameInput>.Instance.GetAction(GameInput.ButtonCode.TogglePhone).GetBindingDisplayString(0, out var _, out var controlPath);
				string displayNameForControlPath = Singleton<InputPromptsManager>.Instance.GetDisplayNameForControlPath(controlPath);
				ReadMessagesQuest.SetEntryTitle("Open your phone (press " + displayNameForControlPath + ") and read your messages");
			}
		}
	}
}
