using System.Collections;
using FluffyUnderware.DevTools.Extensions;
using ScheduleOne.Audio;
using ScheduleOne.DevUtilities;
using ScheduleOne.Interaction;
using ScheduleOne.Map;
using ScheduleOne.NPCs;
using ScheduleOne.UI;
using UnityEngine;

namespace ScheduleOne.Doors
{
	public class StaticDoor : MonoBehaviour
	{
		public const float KNOCK_COOLDOWN = 2f;

		public const float SUMMON_DURATION = 8f;

		[Header("References")]
		public Transform AccessPoint;

		public InteractableObject IntObj;

		public AudioSourceController KnockSound;

		public NPCEnterableBuilding Building;

		[Header("Settings")]
		public bool Usable = true;

		public bool CanKnock = true;

		private float timeSinceLastKnock = 2f;

		protected virtual void Awake()
		{
			IntObj.onHovered.AddListener(Hovered);
			IntObj.onInteractStart.AddListener(Interacted);
			if (Building == null)
			{
				Building = GetComponentInParent<NPCEnterableBuilding>();
				if (Building == null && (Usable || CanKnock))
				{
					Console.LogWarning("StaticDoor " + base.name + " has no NPCEnterableBuilding!");
					Usable = false;
					CanKnock = false;
				}
			}
		}

		protected virtual void OnValidate()
		{
			if (Building == null)
			{
				Building = GetComponentInParent<NPCEnterableBuilding>();
			}
			if (Building != null && !base.transform.IsChildOf(Building.transform))
			{
				Console.LogWarning("StaticDoor " + base.name + " is not a child of " + Building.BuildingName);
			}
		}

		protected virtual void Update()
		{
			if (timeSinceLastKnock < 2f)
			{
				timeSinceLastKnock += Time.deltaTime;
			}
		}

		protected virtual void Hovered()
		{
			if (CanKnockNow())
			{
				if (IsKnockValid(out var message))
				{
					IntObj.SetMessage("Knock");
					IntObj.SetInteractableState(InteractableObject.EInteractableState.Default);
				}
				else
				{
					IntObj.SetMessage(message);
					IntObj.SetInteractableState(InteractableObject.EInteractableState.Invalid);
				}
			}
			else
			{
				IntObj.SetInteractableState(InteractableObject.EInteractableState.Disabled);
			}
		}

		protected virtual bool CanKnockNow()
		{
			if (CanKnock && timeSinceLastKnock >= 2f)
			{
				return Building != null;
			}
			return false;
		}

		protected virtual bool IsKnockValid(out string message)
		{
			message = string.Empty;
			return true;
		}

		protected virtual void Interacted()
		{
			Knock();
		}

		protected virtual void Knock()
		{
			timeSinceLastKnock = 0f;
			if (KnockSound != null)
			{
				KnockSound.Play();
			}
			StartCoroutine(knockRoutine());
			IEnumerator knockRoutine()
			{
				yield return new WaitForSeconds(0.7f);
				if (Building.OccupantCount > 0)
				{
					if (Building.OccupantCount == 1)
					{
						NPCSelected(Building.GetSummonableNPCs()[0]);
					}
					else
					{
						Singleton<NPCSummonMenu>.Instance.Open(Building.GetSummonableNPCs(), NPCSelected);
					}
				}
				else
				{
					Console.Log("Building is empty!");
				}
			}
		}

		protected virtual void NPCSelected(NPC npc)
		{
			npc.behaviour.Summon(Building.GUID.ToString(), Building.Doors.IndexOf(this), 8f);
		}
	}
}
