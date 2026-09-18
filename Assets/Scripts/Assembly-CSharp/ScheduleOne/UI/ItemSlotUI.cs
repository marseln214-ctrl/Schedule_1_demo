using System;
using ScheduleOne.DevUtilities;
using ScheduleOne.ItemFramework;
using ScheduleOne.PlayerScripts;
using ScheduleOne.UI.Items;
using UnityEngine;
using UnityEngine.UI;

namespace ScheduleOne.UI
{
	public class ItemSlotUI : MonoBehaviour
	{
		public Color32 normalColor = new Color32(140, 140, 140, 40);

		public Color32 highlightColor = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, 60);

		[HideInInspector]
		public bool IsBeingDragged;

		[Header("References")]
		public RectTransform Rect;

		public Image Background;

		public GameObject LockContainer;

		public RectTransform ItemContainer;

		public ItemSlot assignedSlot { get; protected set; }

		public ItemUI ItemUI { get; protected set; }

		public virtual void AssignSlot(ItemSlot s)
		{
			if (s == null)
			{
				Console.LogWarning("AssignSlot passed null slot. Use ClearSlot() instead");
			}
			assignedSlot = s;
			ItemSlot itemSlot = assignedSlot;
			itemSlot.onItemInstanceChanged = (Action)Delegate.Combine(itemSlot.onItemInstanceChanged, new Action(UpdateUI));
			ItemSlot itemSlot2 = assignedSlot;
			itemSlot2.onLocked = (Action)Delegate.Combine(itemSlot2.onLocked, new Action(Lock));
			ItemSlot itemSlot3 = assignedSlot;
			itemSlot3.onUnlocked = (Action)Delegate.Combine(itemSlot3.onUnlocked, new Action(Unlock));
			SetHighlighted(h: false);
			if (assignedSlot is HotbarSlot)
			{
				HotbarSlot obj = assignedSlot as HotbarSlot;
				obj.onEquipChanged = (HotbarSlot.EquipEvent)Delegate.Combine(obj.onEquipChanged, new HotbarSlot.EquipEvent(SetHighlighted));
			}
			if (s.IsLocked)
			{
				SetLockVisible(vis: true);
			}
			UpdateUI();
		}

		public virtual void ClearSlot()
		{
			if (assignedSlot != null)
			{
				ItemSlot itemSlot = assignedSlot;
				itemSlot.onItemInstanceChanged = (Action)Delegate.Remove(itemSlot.onItemInstanceChanged, new Action(UpdateUI));
				ItemSlot itemSlot2 = assignedSlot;
				itemSlot2.onLocked = (Action)Delegate.Remove(itemSlot2.onLocked, new Action(Lock));
				ItemSlot itemSlot3 = assignedSlot;
				itemSlot3.onUnlocked = (Action)Delegate.Remove(itemSlot3.onUnlocked, new Action(Unlock));
				if (assignedSlot is HotbarSlot)
				{
					HotbarSlot obj = assignedSlot as HotbarSlot;
					obj.onEquipChanged = (HotbarSlot.EquipEvent)Delegate.Remove(obj.onEquipChanged, new HotbarSlot.EquipEvent(SetHighlighted));
				}
				assignedSlot = null;
				SetLockVisible(vis: false);
				UpdateUI();
			}
		}

		public void OnDestroy()
		{
			if (assignedSlot != null)
			{
				ItemSlot itemSlot = assignedSlot;
				itemSlot.onItemInstanceChanged = (Action)Delegate.Remove(itemSlot.onItemInstanceChanged, new Action(UpdateUI));
			}
		}

		public virtual void UpdateUI()
		{
			if (ItemUI != null)
			{
				ItemUI.Destroy();
				ItemUI = null;
			}
			if (assignedSlot != null && assignedSlot.ItemInstance != null)
			{
				ItemUI original = Singleton<ItemUIManager>.Instance.DefaultItemUIPrefab;
				if (assignedSlot.ItemInstance.Definition.CustomItemUI != null)
				{
					original = assignedSlot.ItemInstance.Definition.CustomItemUI;
				}
				ItemUI = UnityEngine.Object.Instantiate(original, ItemContainer).GetComponent<ItemUI>();
				ItemUI.transform.SetAsLastSibling();
				ItemUI.Setup(assignedSlot.ItemInstance);
			}
		}

		public void SetHighlighted(bool h)
		{
			if (h)
			{
				Background.color = highlightColor;
			}
			else
			{
				Background.color = normalColor;
			}
		}

		public void SetNormalColor(Color color)
		{
			normalColor = color;
			SetHighlighted(h: false);
		}

		public void SetHighlightColor(Color color)
		{
			highlightColor = color;
			SetHighlighted(h: false);
		}

		private void Lock()
		{
			SetLockVisible(vis: true);
		}

		private void Unlock()
		{
			SetLockVisible(vis: false);
		}

		public void SetLockVisible(bool vis)
		{
			LockContainer.gameObject.SetActive(vis);
		}

		public RectTransform DuplicateIcon(Transform parent, int overriddenQuantity = -1)
		{
			if (ItemUI == null)
			{
				return null;
			}
			return ItemUI.DuplicateIcon(parent, overriddenQuantity);
		}

		public void SetVisible(bool shown)
		{
			if (ItemUI != null)
			{
				ItemUI.SetVisible(shown);
			}
		}

		public void OverrideDisplayedQuantity(int quantity)
		{
			if (!(ItemUI == null))
			{
				ItemUI.SetDisplayedQuantity(quantity);
			}
		}
	}
}
