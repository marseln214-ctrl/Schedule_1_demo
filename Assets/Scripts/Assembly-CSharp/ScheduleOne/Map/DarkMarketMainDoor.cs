using System.Collections;
using ScheduleOne.DevUtilities;
using ScheduleOne.Dialogue;
using ScheduleOne.Doors;
using ScheduleOne.Interaction;
using ScheduleOne.Levelling;
using ScheduleOne.NPCs.CharacterClasses;
using ScheduleOne.PlayerScripts;
using UnityEngine;

namespace ScheduleOne.Map
{
	public class DarkMarketMainDoor : MonoBehaviour
	{
		public AudioSource KnockSound;

		public InteractableObject InteractableObject;

		public Peephole Peephole;

		public Igor Igor;

		public DialogueContainer FailDialogue;

		public DialogueContainer SuccessDialogue;

		public DialogueContainer SuccessDialogueNotOpen;

		private Coroutine knockRoutine;

		public bool KnockingEnabled { get; private set; } = true;


		private void Start()
		{
			Igor.gameObject.SetActive(value: false);
		}

		public void SetKnockingEnabled(bool enabled)
		{
			InteractableObject.gameObject.SetActive(enabled);
			KnockingEnabled = enabled;
		}

		public void Hovered()
		{
			if (KnockingEnabled && knockRoutine == null && Player.Local.CrimeData.CurrentPursuitLevel == PlayerCrimeData.EPursuitLevel.None)
			{
				InteractableObject.SetMessage("Knock");
				InteractableObject.SetInteractableState(InteractableObject.EInteractableState.Default);
			}
			else
			{
				InteractableObject.SetInteractableState(InteractableObject.EInteractableState.Disabled);
			}
		}

		public void Interacted()
		{
			Knocked();
		}

		private void Knocked()
		{
			knockRoutine = StartCoroutine(Knock());
			IEnumerator Knock()
			{
				KnockSound.Play();
				Igor.gameObject.SetActive(value: true);
				Igor.Avatar.LookController.ForceLookTarget = PlayerSingleton<PlayerCamera>.Instance.transform;
				yield return new WaitForSeconds(0.75f);
				Peephole.Open();
				yield return new WaitForSeconds(0.3f);
				if (Vector3.Distance(Player.Local.transform.position, base.transform.position) < 3f)
				{
					DialogueContainer container = ((!(NetworkSingleton<LevelManager>.Instance.GetFullRank() >= NetworkSingleton<DarkMarket>.Instance.UnlockRank)) ? FailDialogue : (NetworkSingleton<DarkMarket>.Instance.IsOpen ? SuccessDialogue : SuccessDialogueNotOpen));
					Igor.dialogueHandler.InitializeDialogue(container);
					yield return new WaitUntil(() => !Igor.dialogueHandler.IsPlaying);
				}
				else
				{
					yield return new WaitForSeconds(1f);
				}
				yield return new WaitForSeconds(0.2f);
				Peephole.Close();
				yield return new WaitForSeconds(0.2f);
				yield return new WaitForSeconds(0.5f);
				Igor.gameObject.SetActive(value: false);
				knockRoutine = null;
			}
		}
	}
}
