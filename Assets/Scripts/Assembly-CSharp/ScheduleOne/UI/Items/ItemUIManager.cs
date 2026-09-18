using System.Collections;
using System.Collections.Generic;
using ScheduleOne.DevUtilities;
using ScheduleOne.ItemFramework;
using ScheduleOne.Money;
using ScheduleOne.PlayerScripts;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ScheduleOne.UI.Items
{
	public class ItemUIManager : Singleton<ItemUIManager>
	{
		[Header("References")]
		public Canvas Canvas;

		public GraphicRaycaster[] Raycasters;

		public RectTransform CashDragAmountContainer;

		public RectTransform InputsContainer;

		public ItemInfoPanel InfoPanel;

		public RectTransform ItemQuantityPrompt;

		public Animation CashSlotHintAnim;

		public CanvasGroup CashSlotHintAnimCanvasGroup;

		[Header("Prefabs")]
		public ItemSlotUI ItemSlotUIPrefab;

		public ItemUI DefaultItemUIPrefab;

		public ItemSlotUI HotbarSlotUIPrefab;

		private ItemSlotUI draggedSlot;

		private Vector2 mouseOffset = Vector2.zero;

		private int draggedAmount;

		private RectTransform tempIcon;

		private bool isDraggingCash;

		private float draggedCashAmount;

		private List<ItemSlot> PrimarySlots = new List<ItemSlot>();

		private List<ItemSlot> SecondarySlots = new List<ItemSlot>();

		private bool customDragAmount;

		private Coroutine quantityChangePopRoutine;

		public UnityEvent onDragStart;

		public UnityEvent onItemMoved;

		public bool DraggingEnabled { get; protected set; }

		public ItemSlotUI HoveredSlot { get; protected set; }

		public bool QuickMoveEnabled { get; protected set; }

		protected override void Awake()
		{
			base.Awake();
			InputsContainer.gameObject.SetActive(value: false);
			ItemQuantityPrompt.gameObject.SetActive(value: false);
		}

		protected virtual void Update()
		{
			HoveredSlot = null;
			if (DraggingEnabled)
			{
				CursorManager.ECursorType cursorAppearance = CursorManager.ECursorType.Default;
				HoveredSlot = GetHoveredItemSlot();
				if (HoveredSlot != null && CanDragFromSlot(HoveredSlot))
				{
					cursorAppearance = CursorManager.ECursorType.OpenHand;
				}
				if (HoveredSlot != null && draggedSlot == null && HoveredSlot.assignedSlot != null && HoveredSlot.assignedSlot.Quantity > 0)
				{
					if (InfoPanel.CurrentItem != HoveredSlot.assignedSlot.ItemInstance)
					{
						InfoPanel.Open(HoveredSlot.assignedSlot.ItemInstance, HoveredSlot.Rect);
					}
				}
				else
				{
					ItemDefinitionInfoHoverable hoveredItemInfo = GetHoveredItemInfo();
					if (hoveredItemInfo != null)
					{
						InfoPanel.Open(hoveredItemInfo.AssignedItem, hoveredItemInfo.transform as RectTransform);
					}
					else if (InfoPanel.IsOpen)
					{
						InfoPanel.Close();
					}
				}
				if (draggedSlot != null)
				{
					cursorAppearance = CursorManager.ECursorType.Grab;
					if (!GameInput.GetButton(GameInput.ButtonCode.PrimaryClick) && !GameInput.GetButton(GameInput.ButtonCode.SecondaryClick) && !GameInput.GetButton(GameInput.ButtonCode.TertiaryClick))
					{
						EndDrag();
					}
				}
				else if ((GameInput.GetButtonDown(GameInput.ButtonCode.PrimaryClick) || GameInput.GetButtonDown(GameInput.ButtonCode.SecondaryClick) || GameInput.GetButtonDown(GameInput.ButtonCode.TertiaryClick)) && HoveredSlot != null)
				{
					SlotClicked(HoveredSlot);
				}
				Singleton<CursorManager>.Instance.SetCursorAppearance(cursorAppearance);
			}
			if (draggedSlot != null && customDragAmount)
			{
				if (isDraggingCash)
				{
					CashInstance cashInstance = draggedSlot.assignedSlot.ItemInstance as CashInstance;
					UpdateCashDragAmount(cashInstance);
				}
				else if (GameInput.MouseScrollDelta > 0f)
				{
					SetDraggedAmount(Mathf.Clamp(draggedAmount + 1, 1, draggedSlot.assignedSlot.Quantity));
				}
				else if (GameInput.MouseScrollDelta < 0f)
				{
					SetDraggedAmount(Mathf.Clamp(draggedAmount - 1, 1, draggedSlot.assignedSlot.Quantity));
				}
			}
		}

		protected virtual void LateUpdate()
		{
			if (DraggingEnabled && draggedSlot != null)
			{
				tempIcon.position = new Vector2(UnityEngine.Input.mousePosition.x, UnityEngine.Input.mousePosition.y) - mouseOffset;
				if (customDragAmount)
				{
					ItemQuantityPrompt.position = tempIcon.position + new Vector3(0f, tempIcon.rect.height * 0.5f + 25f, 0f);
				}
			}
			UpdateCashDragSelectorUI();
		}

		private void UpdateCashDragSelectorUI()
		{
			if (draggedSlot != null && draggedSlot.assignedSlot != null && draggedSlot.assignedSlot.ItemInstance != null && draggedSlot.assignedSlot.ItemInstance is CashInstance && customDragAmount)
			{
				_ = draggedSlot.assignedSlot.ItemInstance;
				tempIcon.Find("Balance").GetComponent<TextMeshProUGUI>().text = MoneyManager.FormatAmount(draggedCashAmount);
				CashDragAmountContainer.position = tempIcon.position + new Vector3(0f, tempIcon.rect.height * 0.5f + 15f, 0f);
				CashDragAmountContainer.gameObject.SetActive(value: true);
			}
			else
			{
				CashDragAmountContainer.gameObject.SetActive(value: false);
			}
		}

		private void UpdateCashDragAmount(CashInstance instance)
		{
			float[] array = new float[3] { 50f, 10f, 1f };
			float[] array2 = new float[3] { 100f, 10f, 1f };
			float num = 0f;
			if (GameInput.MouseScrollDelta > 0f)
			{
				for (int i = 0; i < array.Length; i++)
				{
					if (draggedCashAmount >= array2[i])
					{
						num = array[i];
						break;
					}
				}
			}
			else if (GameInput.MouseScrollDelta < 0f)
			{
				for (int j = 0; j < array.Length; j++)
				{
					if (draggedCashAmount > array2[j])
					{
						num = 0f - array[j];
						break;
					}
				}
			}
			if (num != 0f)
			{
				draggedCashAmount = Mathf.Clamp(draggedCashAmount + num, 1f, Mathf.Min(instance.Balance, 1000f));
			}
		}

		public void SetDraggingEnabled(bool enabled, bool modifierPromptsVisible = true)
		{
			DraggingEnabled = enabled;
			if (!DraggingEnabled && draggedSlot != null)
			{
				EndDrag();
			}
			if (InfoPanel.IsOpen)
			{
				InfoPanel.Close();
			}
			if (!enabled)
			{
				DisableQuickMove();
			}
			InputsContainer.gameObject.SetActive(DraggingEnabled && modifierPromptsVisible);
			Singleton<HUD>.Instance.discardSlot.gameObject.SetActive(DraggingEnabled);
		}

		public void EnableQuickMove(List<ItemSlot> primarySlots, List<ItemSlot> secondarySlots)
		{
			QuickMoveEnabled = true;
			PrimarySlots = new List<ItemSlot>();
			PrimarySlots.AddRange(primarySlots);
			SecondarySlots = new List<ItemSlot>();
			SecondarySlots.AddRange(secondarySlots);
			InputsContainer.gameObject.SetActive(QuickMoveEnabled);
		}

		private List<ItemSlot> GetQuickMoveSlots(ItemSlot sourceSlot)
		{
			if (sourceSlot == null || sourceSlot.ItemInstance == null)
			{
				return new List<ItemSlot>();
			}
			List<ItemSlot> obj = (PrimarySlots.Contains(sourceSlot) ? SecondarySlots : PrimarySlots);
			List<ItemSlot> list = new List<ItemSlot>();
			foreach (ItemSlot item in obj)
			{
				if (!item.IsLocked && !item.IsAddLocked && !item.IsRemovalLocked && item.DoesItemMatchFilters(sourceSlot.ItemInstance) && (item.GetCapacityForItem(sourceSlot.ItemInstance) > 0 || sourceSlot.ItemInstance is CashInstance))
				{
					list.Add(item);
				}
			}
			return list;
		}

		public void DisableQuickMove()
		{
			Console.Log("Disabling quick-move...");
			QuickMoveEnabled = false;
		}

		private ItemSlotUI GetHoveredItemSlot()
		{
			PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
			pointerEventData.position = UnityEngine.Input.mousePosition;
			GraphicRaycaster[] raycasters = Raycasters;
			foreach (GraphicRaycaster obj in raycasters)
			{
				List<RaycastResult> list = new List<RaycastResult>();
				obj.Raycast(pointerEventData, list);
				for (int j = 0; j < list.Count; j++)
				{
					if ((bool)list[j].gameObject.GetComponentInParent<ItemSlotUI>())
					{
						return list[j].gameObject.GetComponentInParent<ItemSlotUI>();
					}
				}
			}
			return null;
		}

		private ItemDefinitionInfoHoverable GetHoveredItemInfo()
		{
			PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
			pointerEventData.position = UnityEngine.Input.mousePosition;
			GraphicRaycaster[] raycasters = Raycasters;
			foreach (GraphicRaycaster obj in raycasters)
			{
				List<RaycastResult> list = new List<RaycastResult>();
				obj.Raycast(pointerEventData, list);
				for (int j = 0; j < list.Count; j++)
				{
					ItemDefinitionInfoHoverable componentInParent = list[j].gameObject.GetComponentInParent<ItemDefinitionInfoHoverable>();
					if (componentInParent != null && componentInParent.enabled)
					{
						return componentInParent;
					}
				}
			}
			return null;
		}

		private void SlotClicked(ItemSlotUI ui)
		{
			if (!CanDragFromSlot(ui) || !DraggingEnabled || draggedSlot != null || ui.assignedSlot.ItemInstance == null || ui.assignedSlot.IsLocked || ui.assignedSlot.IsRemovalLocked)
			{
				return;
			}
			mouseOffset = new Vector2(UnityEngine.Input.mousePosition.x, UnityEngine.Input.mousePosition.y) - new Vector2(ui.ItemUI.Rect.position.x, ui.ItemUI.Rect.position.y);
			draggedSlot = ui;
			isDraggingCash = draggedSlot.assignedSlot.ItemInstance is CashInstance;
			if (isDraggingCash)
			{
				StartDragCash();
				return;
			}
			customDragAmount = false;
			draggedAmount = draggedSlot.assignedSlot.Quantity;
			if (GameInput.GetButtonDown(GameInput.ButtonCode.SecondaryClick))
			{
				draggedAmount = 1;
				customDragAmount = true;
				mouseOffset += new Vector2(-10f, -15f);
			}
			if (GameInput.GetButton(GameInput.ButtonCode.QuickMove) && QuickMoveEnabled)
			{
				List<ItemSlot> quickMoveSlots = GetQuickMoveSlots(draggedSlot.assignedSlot);
				if (quickMoveSlots.Count > 0)
				{
					int num = 0;
					for (int i = 0; i < quickMoveSlots.Count; i++)
					{
						if (num >= draggedAmount)
						{
							break;
						}
						if (quickMoveSlots[i].ItemInstance != null && quickMoveSlots[i].ItemInstance.CanStackWith(draggedSlot.assignedSlot.ItemInstance, checkQuantities: false))
						{
							int num2 = Mathf.Min(quickMoveSlots[i].GetCapacityForItem(draggedSlot.assignedSlot.ItemInstance), draggedAmount - num);
							quickMoveSlots[i].AddItem(draggedSlot.assignedSlot.ItemInstance.GetCopy(num2));
							num += num2;
						}
					}
					for (int j = 0; j < quickMoveSlots.Count; j++)
					{
						if (num >= draggedAmount)
						{
							break;
						}
						int num3 = Mathf.Min(quickMoveSlots[j].GetCapacityForItem(draggedSlot.assignedSlot.ItemInstance), draggedAmount - num);
						quickMoveSlots[j].AddItem(draggedSlot.assignedSlot.ItemInstance.GetCopy(num3));
						num += num3;
					}
					draggedSlot.assignedSlot.ChangeQuantity(-num);
				}
				draggedSlot = null;
				if (onItemMoved != null)
				{
					onItemMoved.Invoke();
				}
			}
			else
			{
				if (onDragStart != null)
				{
					onDragStart.Invoke();
				}
				ItemQuantityPrompt.gameObject.SetActive(customDragAmount);
				tempIcon = draggedSlot.DuplicateIcon(Singleton<HUD>.Instance.transform, draggedAmount);
				draggedSlot.IsBeingDragged = true;
				if (draggedAmount == draggedSlot.assignedSlot.Quantity)
				{
					draggedSlot.SetVisible(shown: false);
				}
				else
				{
					draggedSlot.OverrideDisplayedQuantity(draggedSlot.assignedSlot.Quantity - draggedAmount);
				}
			}
		}

		private void StartDragCash()
		{
			CashInstance cashInstance = draggedSlot.assignedSlot.ItemInstance as CashInstance;
			draggedCashAmount = Mathf.Min(cashInstance.Balance, 1000f);
			draggedAmount = 1;
			if (draggedCashAmount <= 0f)
			{
				draggedSlot = null;
			}
			else if (GameInput.GetButton(GameInput.ButtonCode.QuickMove) && QuickMoveEnabled)
			{
				List<ItemSlot> quickMoveSlots = GetQuickMoveSlots(draggedSlot.assignedSlot);
				if (quickMoveSlots.Count > 0)
				{
					Debug.Log("Quick-moving " + draggedAmount + " items...");
					float a = draggedCashAmount;
					float num = 0f;
					for (int i = 0; i < quickMoveSlots.Count; i++)
					{
						if (num >= (float)draggedAmount)
						{
							break;
						}
						ItemSlot itemSlot = quickMoveSlots[i];
						if (itemSlot.ItemInstance != null)
						{
							if (itemSlot.ItemInstance is CashInstance cashInstance2)
							{
								float num2 = 0f;
								num2 = ((!(itemSlot is CashSlot)) ? Mathf.Min(a, 1000f - cashInstance2.Balance) : Mathf.Min(a, float.MaxValue - cashInstance2.Balance));
								cashInstance2.ChangeBalance(num2);
								itemSlot.ReplicateStoredInstance();
								num += num2;
							}
						}
						else
						{
							CashInstance cashInstance3 = Registry.GetItem("cash").GetDefaultInstance() as CashInstance;
							cashInstance3.SetBalance(draggedCashAmount);
							itemSlot.SetStoredItem(cashInstance3);
							num += draggedCashAmount;
						}
					}
					if (num >= cashInstance.Balance)
					{
						draggedSlot.assignedSlot.ClearStoredInstance();
					}
					else
					{
						cashInstance.ChangeBalance(0f - num);
						draggedSlot.assignedSlot.ReplicateStoredInstance();
					}
				}
				if (onItemMoved != null)
				{
					onItemMoved.Invoke();
				}
				draggedSlot = null;
			}
			else
			{
				if (onDragStart != null)
				{
					onDragStart.Invoke();
				}
				if (draggedSlot.assignedSlot != PlayerSingleton<PlayerInventory>.Instance.cashSlot)
				{
					CashSlotHintAnim.Play();
				}
				if (GameInput.GetButtonDown(GameInput.ButtonCode.SecondaryClick))
				{
					draggedAmount = 1;
					draggedCashAmount = Mathf.Min(cashInstance.Balance, 100f);
					mouseOffset += new Vector2(-10f, -15f);
					customDragAmount = true;
				}
				tempIcon = draggedSlot.DuplicateIcon(Singleton<HUD>.Instance.transform, draggedAmount);
				tempIcon.Find("Balance").GetComponent<TextMeshProUGUI>().text = MoneyManager.FormatAmount(draggedCashAmount);
				draggedSlot.IsBeingDragged = true;
				if (draggedCashAmount >= cashInstance.Balance)
				{
					draggedSlot.SetVisible(shown: false);
				}
				else
				{
					(draggedSlot.ItemUI as ItemUI_Cash).SetDisplayedBalance(cashInstance.Balance - draggedCashAmount);
				}
			}
		}

		private void EndDrag()
		{
			if (isDraggingCash)
			{
				EndCashDrag();
				return;
			}
			if (CanDragFromSlot(draggedSlot) && HoveredSlot != null && HoveredSlot != draggedSlot && HoveredSlot.assignedSlot != null && !HoveredSlot.assignedSlot.IsLocked && !HoveredSlot.assignedSlot.IsAddLocked && HoveredSlot.assignedSlot.DoesItemMatchFilters(draggedSlot.assignedSlot.ItemInstance))
			{
				if (HoveredSlot.assignedSlot.ItemInstance == null)
				{
					HoveredSlot.assignedSlot.SetStoredItem(draggedSlot.assignedSlot.ItemInstance.GetCopy(draggedAmount));
					draggedSlot.assignedSlot.ChangeQuantity(-draggedAmount);
				}
				else if (HoveredSlot.assignedSlot.ItemInstance.CanStackWith(draggedSlot.assignedSlot.ItemInstance, checkQuantities: false))
				{
					while (HoveredSlot.assignedSlot.Quantity < HoveredSlot.assignedSlot.ItemInstance.StackLimit && draggedAmount > 0)
					{
						HoveredSlot.assignedSlot.ChangeQuantity(1);
						draggedSlot.assignedSlot.ChangeQuantity(-1);
						draggedAmount--;
					}
				}
				else if (draggedAmount == draggedSlot.assignedSlot.Quantity)
				{
					ItemInstance itemInstance = draggedSlot.assignedSlot.ItemInstance;
					ItemInstance itemInstance2 = HoveredSlot.assignedSlot.ItemInstance;
					draggedSlot.assignedSlot.SetStoredItem(itemInstance2);
					HoveredSlot.assignedSlot.SetStoredItem(itemInstance);
				}
				else if (HoveredSlot.assignedSlot.ItemInstance == null)
				{
					HoveredSlot.assignedSlot.SetStoredItem(draggedSlot.assignedSlot.ItemInstance);
					draggedSlot.assignedSlot.ClearStoredInstance();
				}
				if (onItemMoved != null)
				{
					onItemMoved.Invoke();
				}
			}
			if (draggedSlot != null)
			{
				draggedSlot.SetVisible(shown: true);
				draggedSlot.UpdateUI();
				draggedSlot.IsBeingDragged = false;
				draggedSlot = null;
			}
			if (tempIcon != null)
			{
				Object.Destroy(tempIcon.gameObject);
				tempIcon = null;
			}
			ItemQuantityPrompt.gameObject.SetActive(value: false);
			Singleton<CursorManager>.Instance.SetCursorAppearance(CursorManager.ECursorType.Default);
		}

		private void SetDraggedAmount(int amount)
		{
			draggedAmount = amount;
			TextMeshProUGUI quantityText = tempIcon.Find("Quantity").GetComponent<TextMeshProUGUI>();
			if (quantityText != null && quantityText.gameObject.name == "Quantity")
			{
				quantityText.text = draggedAmount + "x";
				quantityText.enabled = draggedAmount > 1;
			}
			if (draggedAmount == draggedSlot.assignedSlot.Quantity)
			{
				draggedSlot.SetVisible(shown: false);
			}
			else
			{
				draggedSlot.OverrideDisplayedQuantity(draggedSlot.assignedSlot.Quantity - draggedAmount);
				draggedSlot.SetVisible(shown: true);
			}
			if (quantityText != null)
			{
				if (quantityChangePopRoutine != null)
				{
					StopCoroutine(quantityChangePopRoutine);
				}
				quantityChangePopRoutine = StartCoroutine(LerpQuantityTextSize());
			}
			IEnumerator LerpQuantityTextSize()
			{
				RectTransform quantityTransform = quantityText.rectTransform;
				while (quantityTransform != null && quantityTransform.localScale.x < 1.35f)
				{
					float num = Mathf.MoveTowards(quantityTransform.localScale.x, 1.35f, Time.deltaTime * 10f);
					quantityTransform.localScale = Vector3.one * num;
					yield return new WaitForEndOfFrame();
				}
				yield return new WaitForSeconds(0.1f);
				while (quantityTransform != null && quantityTransform.localScale.x > 1f)
				{
					float num2 = Mathf.MoveTowards(quantityTransform.localScale.x, 1f, Time.deltaTime * 5f);
					quantityTransform.localScale = Vector3.one * num2;
					yield return new WaitForEndOfFrame();
				}
				quantityChangePopRoutine = null;
			}
		}

		private void EndCashDrag()
		{
			CashInstance cashInstance = null;
			if (draggedSlot != null && draggedSlot.assignedSlot != null)
			{
				cashInstance = draggedSlot.assignedSlot.ItemInstance as CashInstance;
			}
			CashSlotHintAnim.Stop();
			CashSlotHintAnimCanvasGroup.alpha = 0f;
			if (CanDragFromSlot(draggedSlot) && HoveredSlot != null && CanCashBeDraggedIntoSlot(HoveredSlot) && !HoveredSlot.assignedSlot.IsLocked && !HoveredSlot.assignedSlot.IsAddLocked)
			{
				if (HoveredSlot.assignedSlot is HotbarSlot && !(HoveredSlot.assignedSlot is CashSlot))
				{
					HoveredSlot = Singleton<HUD>.Instance.cashSlotUI.GetComponent<CashSlotUI>();
				}
				float num = Mathf.Min(draggedCashAmount, cashInstance.Balance);
				if (num > 0f)
				{
					float num2 = num;
					if (HoveredSlot.assignedSlot.ItemInstance != null)
					{
						CashInstance cashInstance2 = HoveredSlot.assignedSlot.ItemInstance as CashInstance;
						num2 = ((!(HoveredSlot.assignedSlot is CashSlot)) ? Mathf.Min(num, 1000f - cashInstance2.Balance) : Mathf.Min(num, float.MaxValue - cashInstance2.Balance));
						cashInstance2.ChangeBalance(num2);
						HoveredSlot.assignedSlot.ReplicateStoredInstance();
					}
					else
					{
						CashInstance cashInstance3 = Registry.GetItem("cash").GetDefaultInstance() as CashInstance;
						cashInstance3.SetBalance(num2);
						HoveredSlot.assignedSlot.SetStoredItem(cashInstance3);
					}
					if (num2 >= cashInstance.Balance)
					{
						draggedSlot.assignedSlot.ClearStoredInstance();
					}
					else
					{
						cashInstance.ChangeBalance(0f - num2);
						draggedSlot.assignedSlot.ReplicateStoredInstance();
					}
				}
			}
			draggedSlot.SetVisible(shown: true);
			draggedSlot.UpdateUI();
			draggedSlot.IsBeingDragged = false;
			draggedSlot = null;
			Object.Destroy(tempIcon.gameObject);
			Singleton<CursorManager>.Instance.SetCursorAppearance(CursorManager.ECursorType.Default);
		}

		public bool CanDragFromSlot(ItemSlotUI slotUI)
		{
			if (slotUI == null)
			{
				return false;
			}
			if (slotUI.assignedSlot == null)
			{
				return false;
			}
			if (slotUI.assignedSlot.ItemInstance == null)
			{
				return false;
			}
			if (slotUI.assignedSlot.IsLocked || slotUI.assignedSlot.IsRemovalLocked)
			{
				return false;
			}
			return true;
		}

		public bool CanCashBeDraggedIntoSlot(ItemSlotUI ui)
		{
			if (ui == null)
			{
				return false;
			}
			if (ui.assignedSlot == null)
			{
				return false;
			}
			if (ui.assignedSlot.ItemInstance != null && !(ui.assignedSlot.ItemInstance is CashInstance))
			{
				return false;
			}
			return true;
		}
	}
}
