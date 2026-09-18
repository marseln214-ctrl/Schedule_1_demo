using System;
using EasyButtons;
using ScheduleOne.Economy;
using ScheduleOne.NPCs;
using ScheduleOne.NPCs.Relation;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ScheduleOne.UI.Relations
{
	public class RelationCircle : MonoBehaviour
	{
		public const float NotchMinRot = 90f;

		public const float NotchMaxRot = -90f;

		public static Color PortraitColor_ZeroDependence = new Color32(60, 60, 60, byte.MaxValue);

		public static Color PortraitColor_MaxDependence = new Color32(120, 15, 15, byte.MaxValue);

		public string AssignedNPC_ID = string.Empty;

		public NPC AssignedNPC;

		public Action onClicked;

		public Action onHoverStart;

		public Action onHoverEnd;

		public bool AutoSetName;

		[Header("References")]
		public RectTransform Rect;

		public Image PortraitBackground;

		public Image HeadshotImg;

		public RectTransform NotchPivot;

		public RectTransform Locked;

		public Button Button;

		public EventTrigger Trigger;

		private void Awake()
		{
			LoadNPCData();
			if (AssignedNPC != null)
			{
				AssignNPC(AssignedNPC);
			}
			else if (AssignedNPC_ID != string.Empty)
			{
				Console.LogWarning("Failed to find NPC with ID '" + AssignedNPC_ID + "'");
			}
			Button.onClick.AddListener(ButtonClicked);
			EventTrigger.Entry entry = new EventTrigger.Entry();
			entry.eventID = EventTriggerType.PointerEnter;
			entry.callback.AddListener(delegate
			{
				HoverStart();
			});
			Trigger.triggers.Add(entry);
			EventTrigger.Entry entry2 = new EventTrigger.Entry();
			entry2.eventID = EventTriggerType.PointerExit;
			entry2.callback.AddListener(delegate
			{
				HoverEnd();
			});
			Trigger.triggers.Add(entry2);
		}

		private void OnValidate()
		{
			if (AssignedNPC != null)
			{
				AssignedNPC_ID = AssignedNPC.ID;
				HeadshotImg.sprite = AssignedNPC.MugshotSprite;
			}
			if (AutoSetName && AssignedNPC != null)
			{
				base.gameObject.name = AssignedNPC_ID;
			}
		}

		public void AssignNPC(NPC npc)
		{
			if (npc != null)
			{
				UnassignNPC();
			}
			AssignedNPC = npc;
			NPCRelationData relationData = AssignedNPC.RelationData;
			relationData.onRelationshipChange = (Action<float>)Delegate.Combine(relationData.onRelationshipChange, new Action<float>(RelationshipChange));
			NPCRelationData relationData2 = AssignedNPC.RelationData;
			relationData2.onUnlocked = (Action<NPCRelationData.EUnlockType, bool>)Delegate.Combine(relationData2.onUnlocked, new Action<NPCRelationData.EUnlockType, bool>(SetUnlocked));
			foreach (NPC connection in AssignedNPC.RelationData.Connections)
			{
				NPCRelationData relationData3 = connection.RelationData;
				relationData3.onUnlocked = (Action<NPCRelationData.EUnlockType, bool>)Delegate.Combine(relationData3.onUnlocked, (Action<NPCRelationData.EUnlockType, bool>)delegate
				{
					UpdateBlackout();
				});
			}
			if (npc.RelationData.Unlocked)
			{
				SetUnlocked(npc.RelationData.UnlockType, notify: false);
			}
			else
			{
				SetLocked();
			}
			HeadshotImg.sprite = AssignedNPC.MugshotSprite;
			RefreshNotchPosition();
			RefreshDependenceDisplay();
			UpdateBlackout();
		}

		private void UnassignNPC()
		{
			if (AssignedNPC != null)
			{
				NPCRelationData relationData = AssignedNPC.RelationData;
				relationData.onRelationshipChange = (Action<float>)Delegate.Remove(relationData.onRelationshipChange, new Action<float>(RelationshipChange));
				NPCRelationData relationData2 = AssignedNPC.RelationData;
				relationData2.onUnlocked = (Action<NPCRelationData.EUnlockType, bool>)Delegate.Remove(relationData2.onUnlocked, new Action<NPCRelationData.EUnlockType, bool>(SetUnlocked));
			}
		}

		private void RelationshipChange(float change)
		{
			RefreshNotchPosition();
		}

		public void SetNotchPosition(float relationshipDelta)
		{
			NotchPivot.localRotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(90f, -90f, relationshipDelta / 5f));
		}

		private void RefreshNotchPosition()
		{
			SetNotchPosition(AssignedNPC.RelationData.RelationDelta);
		}

		private void RefreshDependenceDisplay()
		{
			Customer component = AssignedNPC.GetComponent<Customer>();
			if (component == null)
			{
				PortraitBackground.color = PortraitColor_ZeroDependence;
			}
			else
			{
				PortraitBackground.color = Color.Lerp(PortraitColor_ZeroDependence, PortraitColor_MaxDependence, component.CurrentAddiction);
			}
		}

		[Button]
		public void SetLocked()
		{
			Locked.gameObject.SetActive(value: true);
			NotchPivot.gameObject.SetActive(value: false);
		}

		[Button]
		public void SetUnlocked(NPCRelationData.EUnlockType unlockType, bool notify = true)
		{
			Locked.gameObject.SetActive(value: false);
			NotchPivot.gameObject.SetActive(value: true);
			SetBlackedOut(blackedOut: false);
		}

		[Button]
		public void LoadNPCData()
		{
			AssignedNPC = NPCManager.GetNPC(AssignedNPC_ID);
		}

		private void UpdateBlackout()
		{
			SetBlackedOut(!AssignedNPC.RelationData.Unlocked && !AssignedNPC.RelationData.IsMutuallyKnown());
		}

		public void SetBlackedOut(bool blackedOut)
		{
			HeadshotImg.color = (blackedOut ? Color.black : Color.white);
		}

		private void ButtonClicked()
		{
			if (onClicked != null)
			{
				onClicked();
			}
		}

		private void HoverStart()
		{
			if (onHoverStart != null)
			{
				onHoverStart();
			}
		}

		private void HoverEnd()
		{
			if (onHoverEnd != null)
			{
				onHoverEnd();
			}
		}
	}
}
