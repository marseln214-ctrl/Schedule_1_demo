using System;
using System.Collections;
using System.Collections.Generic;
using ScheduleOne.DevUtilities;
using ScheduleOne.ItemFramework;
using ScheduleOne.NPCs;
using ScheduleOne.PlayerScripts;
using ScheduleOne.UI.Items;
using ScheduleOne.Variables;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ScheduleOne.UI
{
	public class BodySearchScreen : Singleton<BodySearchScreen>
	{
		public const float MAX_SPEED_BOOST = 2.5f;

		public Color SlotRedColor = new Color(1f, 0f, 0f, 0.5f);

		public Color SlotHighlightRedColor = new Color(1f, 0f, 0f, 0.5f);

		public float GapTime = 0.2f;

		[Header("References")]
		public Canvas Canvas;

		public RectTransform Container;

		public RectTransform MinigameController;

		public RectTransform SlotContainer;

		public ItemSlotUI ItemSlotPrefab;

		public RectTransform SearchIndicator;

		public RectTransform SearchIndicatorStart;

		public RectTransform SearchIndicatorEnd;

		public Animation IndicatorAnimation;

		public Animation TutorialAnimation;

		public RectTransform TutorialContainer;

		public Animation ResetAnimation;

		private List<ItemSlotUI> slots = new List<ItemSlotUI>();

		public UnityEvent onSearchClear;

		public UnityEvent onSearchFail;

		private Color defaultSlotColor = new Color(0f, 0f, 0f, 0f);

		private Color defaultSlotHighlightColor = new Color(0f, 0f, 0f, 0f);

		private ItemSlotUI concealedSlot;

		private ItemSlotUI hoveredSlot;

		private Color[] defaultItemIconColors;

		private float speedBoost;

		private NPC searcher;

		public bool IsOpen { get; private set; }

		public bool TutorialOpen { get; private set; }

		protected override void Start()
		{
			base.Start();
			if (Player.Local != null)
			{
				SetupSlots();
			}
			else
			{
				Player.onLocalPlayerSpawned = (Action)Delegate.Combine(Player.onLocalPlayerSpawned, new Action(SetupSlots));
			}
			Canvas.enabled = false;
			Container.gameObject.SetActive(value: false);
		}

		private void SetupSlots()
		{
			Player.onLocalPlayerSpawned = (Action)Delegate.Remove(Player.onLocalPlayerSpawned, new Action(SetupSlots));
			for (int i = 0; i < 8; i++)
			{
				ItemSlotUI slot = UnityEngine.Object.Instantiate(ItemSlotPrefab, SlotContainer);
				slot.AssignSlot(PlayerSingleton<PlayerInventory>.Instance.hotbarSlots[i]);
				slots.Add(slot);
				EventTrigger eventTrigger = slot.Rect.gameObject.AddComponent<EventTrigger>();
				eventTrigger.triggers = new List<EventTrigger.Entry>();
				EventTrigger.Entry entry = new EventTrigger.Entry();
				entry.eventID = EventTriggerType.PointerDown;
				entry.callback.AddListener(delegate
				{
					SlotHeld(slot);
				});
				eventTrigger.triggers.Add(entry);
				EventTrigger.Entry entry2 = new EventTrigger.Entry();
				entry2.eventID = EventTriggerType.PointerUp;
				entry2.callback.AddListener(delegate
				{
					SlotReleased(slot);
				});
				eventTrigger.triggers.Add(entry2);
			}
			defaultSlotColor = slots[0].normalColor;
			defaultSlotHighlightColor = slots[0].highlightColor;
		}

		private void Update()
		{
			if (hoveredSlot != null)
			{
				hoveredSlot.SetHighlighted(hoveredSlot != concealedSlot);
			}
			if (IsOpen)
			{
				if (GameInput.GetButton(GameInput.ButtonCode.Jump))
				{
					speedBoost = Mathf.MoveTowards(speedBoost, 2.5f, Time.deltaTime * 6f);
				}
				else
				{
					speedBoost = Mathf.MoveTowards(speedBoost, 0f, Time.deltaTime * 6f);
				}
				PlayerSingleton<PlayerCamera>.Instance.LookAt(searcher.dialogueHandler.LookPosition.position, 0f);
				if (Player.Local != null && Player.Local.CrimeData.CurrentPursuitLevel != 0)
				{
					Close(clear: false);
				}
			}
		}

		public void Open(NPC _searcher, float searchTime = 0f)
		{
			IsOpen = true;
			searcher = _searcher;
			Singleton<GameInput>.Instance.ExitAll();
			PlayerSingleton<PlayerInventory>.Instance.SetInventoryEnabled(enabled: false);
			PlayerSingleton<PlayerMovement>.Instance.canMove = false;
			PlayerSingleton<PlayerCamera>.Instance.SetCanLook(c: false);
			PlayerSingleton<PlayerCamera>.Instance.AddActiveUIElement(base.name);
			PlayerSingleton<PlayerCamera>.Instance.FreeMouse();
			PlayerSingleton<PlayerCamera>.Instance.SetDoFActive(active: true, 0f);
			Singleton<ItemUIManager>.Instance.SetDraggingEnabled(enabled: false);
			Singleton<HUD>.Instance.SetCrosshairVisible(vis: false);
			for (int i = 0; i < slots.Count; i++)
			{
				if (slots[i].assignedSlot.ItemInstance != null && slots[i].assignedSlot.ItemInstance.Definition.legalStatus != 0)
				{
					slots[i].SetNormalColor(SlotRedColor);
					slots[i].SetHighlightColor(SlotHighlightRedColor);
				}
				else
				{
					slots[i].SetNormalColor(defaultSlotColor);
					slots[i].SetHighlightColor(defaultSlotHighlightColor);
				}
				slots[i].SetHighlighted(h: false);
			}
			concealedSlot = null;
			StartCoroutine(Search());
			IEnumerator Search()
			{
				SearchIndicator.anchoredPosition = SearchIndicatorStart.anchoredPosition;
				SearchIndicator.GetComponent<CanvasGroup>().alpha = 0f;
				Canvas.enabled = true;
				Container.gameObject.SetActive(value: true);
				yield return new WaitForSeconds(0.5f);
				if (!NetworkSingleton<VariableDatabase>.Instance.GetValue<bool>("BodySearchTutorialDone"))
				{
					_ = GameManager.IS_TUTORIAL;
					searchTime = 8f;
					NetworkSingleton<VariableDatabase>.Instance.SetVariableValue("BodySearchTutorialDone", true.ToString());
					MinigameController.gameObject.SetActive(value: false);
					OpenTutorial();
					yield return new WaitUntil(() => !TutorialOpen);
					MinigameController.gameObject.SetActive(value: true);
				}
				IndicatorAnimation.Play("Police icon start");
				yield return new WaitForSeconds(1f);
				float num = searchTime * GapTime;
				int count = slots.Count;
				float perGap = num / (float)count;
				float num2 = (searchTime - num) / (float)slots.Count;
				float perBlock = perGap + num2;
				for (float j = 0f; j < searchTime; j += Time.deltaTime * (1f + speedBoost))
				{
					float t = j / searchTime;
					SearchIndicator.anchoredPosition = Vector3.Lerp(SearchIndicatorStart.anchoredPosition, SearchIndicatorEnd.anchoredPosition, t);
					int num3 = Mathf.FloorToInt(j / perBlock);
					if (j - (float)num3 * perBlock < perGap)
					{
						if (hoveredSlot != null)
						{
							hoveredSlot.SetHighlighted(h: false);
							hoveredSlot = null;
						}
					}
					else
					{
						int index = num3;
						hoveredSlot = slots[index];
						ItemInstance itemInstance = hoveredSlot.assignedSlot.ItemInstance;
						if (!IsSlotConcealed(hoveredSlot) && itemInstance != null && itemInstance.Definition.legalStatus != 0)
						{
							ItemDetected(hoveredSlot);
							yield return new WaitForSeconds(1f);
							if (GameManager.IS_TUTORIAL)
							{
								ResetAnimation.Play();
								yield return new WaitForSeconds(0.55f);
								StartCoroutine(Search());
							}
							else
							{
								Close(clear: false);
							}
							yield break;
						}
					}
					yield return new WaitForEndOfFrame();
				}
				hoveredSlot?.SetHighlighted(h: false);
				hoveredSlot = null;
				IndicatorAnimation.Play("Police icon end");
				yield return new WaitForSeconds(0.3f);
				Close(clear: true);
			}
		}

		private bool IsSlotConcealed(ItemSlotUI slot)
		{
			return concealedSlot == slot;
		}

		private void ItemDetected(ItemSlotUI slot)
		{
			IndicatorAnimation.Play("Police icon discover");
			if (onSearchFail != null)
			{
				onSearchFail.Invoke();
			}
		}

		public void SlotHeld(ItemSlotUI ui)
		{
			concealedSlot = ui;
			Image[] componentsInChildren = ui.ItemContainer.GetComponentsInChildren<Image>();
			defaultItemIconColors = new Color[componentsInChildren.Length];
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				defaultItemIconColors[i] = componentsInChildren[i].color;
				componentsInChildren[i].color = Color.black;
			}
		}

		public void SlotReleased(ItemSlotUI ui)
		{
			concealedSlot = null;
			Image[] componentsInChildren = ui.ItemContainer.GetComponentsInChildren<Image>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].color = defaultItemIconColors[i];
			}
		}

		public void Close(bool clear)
		{
			IsOpen = false;
			Canvas.enabled = false;
			Container.gameObject.SetActive(value: false);
			PlayerSingleton<PlayerMovement>.Instance.canMove = true;
			PlayerSingleton<PlayerCamera>.Instance.SetCanLook(c: true);
			PlayerSingleton<PlayerCamera>.Instance.RemoveActiveUIElement(base.name);
			PlayerSingleton<PlayerCamera>.Instance.LockMouse();
			PlayerSingleton<PlayerCamera>.Instance.SetDoFActive(active: false, 0f);
			PlayerSingleton<PlayerInventory>.Instance.SetInventoryEnabled(enabled: true);
			Singleton<HUD>.Instance.SetCrosshairVisible(vis: true);
			if (clear && onSearchClear != null)
			{
				onSearchClear.Invoke();
			}
		}

		private void OpenTutorial()
		{
			TutorialOpen = true;
			TutorialContainer.gameObject.SetActive(value: true);
			TutorialAnimation.Play();
		}

		public void CloseTutorial()
		{
			TutorialOpen = false;
			TutorialContainer.gameObject.SetActive(value: false);
		}
	}
}
