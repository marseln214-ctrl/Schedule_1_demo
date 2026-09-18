using System.Linq;
using ScheduleOne.DevUtilities;
using ScheduleOne.ItemFramework;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Storage;
using ScheduleOne.UI.Items;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ScheduleOne.UI
{
	public class StorageMenu : Singleton<StorageMenu>
	{
		[Header("References")]
		public Canvas Canvas;

		public RectTransform Container;

		public TextMeshProUGUI TitleLabel;

		public TextMeshProUGUI SubtitleLabel;

		public RectTransform SlotContainer;

		public ItemSlotUI[] SlotsUIs;

		public GridLayoutGroup SlotGridLayout;

		public RectTransform CloseButton;

		public UnityEvent onClosed;

		public bool IsOpen { get; protected set; }

		public StorageEntity OpenedStorageEntity { get; protected set; }

		protected override void Awake()
		{
			base.Awake();
			Canvas.enabled = false;
			Container.gameObject.SetActive(value: false);
			GameInput.RegisterExitListener(Exit, 3);
		}

		public virtual void Open(IItemSlotOwner owner, string title, string subtitle)
		{
			IsOpen = true;
			OpenedStorageEntity = null;
			SlotGridLayout.constraintCount = 1;
			Open(title, subtitle, owner);
		}

		public virtual void Open(StorageEntity entity)
		{
			IsOpen = true;
			OpenedStorageEntity = entity;
			SlotGridLayout.constraintCount = entity.DisplayRowCount;
			Open(entity.StorageEntityName, entity.StorageEntitySubtitle, entity);
		}

		private void Open(string title, string subtitle, IItemSlotOwner owner)
		{
			IsOpen = true;
			TitleLabel.text = title;
			SubtitleLabel.text = subtitle;
			for (int i = 0; i < SlotsUIs.Length; i++)
			{
				if (owner.ItemSlots.Count > i)
				{
					SlotsUIs[i].gameObject.SetActive(value: true);
					SlotsUIs[i].AssignSlot(owner.ItemSlots[i]);
				}
				else
				{
					SlotsUIs[i].ClearSlot();
					SlotsUIs[i].gameObject.SetActive(value: false);
				}
			}
			int constraintCount = SlotGridLayout.constraintCount;
			CloseButton.anchoredPosition = new Vector2(0f, (float)constraintCount * (0f - SlotGridLayout.cellSize.y) - 60f);
			PlayerSingleton<PlayerMovement>.Instance.canMove = false;
			PlayerSingleton<PlayerCamera>.Instance.AddActiveUIElement(base.name);
			PlayerSingleton<PlayerCamera>.Instance.FreeMouse();
			PlayerSingleton<PlayerCamera>.Instance.SetCanLook(c: false);
			PlayerSingleton<PlayerCamera>.Instance.SetDoFActive(active: true, 0.06f);
			PlayerSingleton<PlayerInventory>.Instance.SetInventoryEnabled(enabled: true);
			PlayerSingleton<PlayerInventory>.Instance.SetEquippingEnabled(enabled: false);
			Singleton<ItemUIManager>.Instance.SetDraggingEnabled(enabled: true);
			Singleton<ItemUIManager>.Instance.EnableQuickMove(PlayerSingleton<PlayerInventory>.Instance.GetAllInventorySlots(), owner.ItemSlots.ToList());
			Singleton<InputPromptsCanvas>.Instance.LoadModule("exitonly");
			Canvas.enabled = true;
			Container.gameObject.SetActive(value: true);
		}

		public void Close()
		{
			if (OpenedStorageEntity != null)
			{
				OpenedStorageEntity.Close();
			}
			else
			{
				CloseMenu();
			}
		}

		public virtual void CloseMenu()
		{
			IsOpen = false;
			OpenedStorageEntity = null;
			for (int i = 0; i < SlotsUIs.Length; i++)
			{
				SlotsUIs[i].ClearSlot();
				SlotsUIs[i].gameObject.SetActive(value: false);
			}
			PlayerSingleton<PlayerMovement>.Instance.canMove = true;
			PlayerSingleton<PlayerCamera>.Instance.RemoveActiveUIElement(base.name);
			PlayerSingleton<PlayerCamera>.Instance.LockMouse();
			PlayerSingleton<PlayerCamera>.Instance.SetCanLook(c: true);
			PlayerSingleton<PlayerCamera>.Instance.SetDoFActive(active: false, 0.06f);
			PlayerSingleton<PlayerInventory>.Instance.SetEquippingEnabled(enabled: true);
			Singleton<ItemUIManager>.Instance.SetDraggingEnabled(enabled: false);
			Singleton<InputPromptsCanvas>.Instance.UnloadModule();
			Canvas.enabled = false;
			Container.gameObject.SetActive(value: false);
			if (onClosed != null)
			{
				onClosed.Invoke();
			}
		}

		private void Exit(ExitAction action)
		{
			if (!action.used && IsOpen && action.exitType == ExitType.Escape)
			{
				action.used = true;
				if (OpenedStorageEntity != null)
				{
					OpenedStorageEntity.Close();
				}
				else
				{
					CloseMenu();
				}
			}
		}
	}
}
