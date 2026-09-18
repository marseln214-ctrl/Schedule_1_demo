using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FluffyUnderware.DevTools.Extensions;
using GameKit.Utilities;
using ScheduleOne.DevUtilities;
using ScheduleOne.ItemFramework;
using ScheduleOne.Levelling;
using ScheduleOne.NPCs;
using ScheduleOne.PlayerScripts;
using ScheduleOne.UI.Input;
using ScheduleOne.UI.Items;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ScheduleOne.UI
{
	public class PickpocketScreen : Singleton<PickpocketScreen>
	{
		public const int PICKPOCKET_XP = 2;

		[Header("Settings")]
		public float GreenAreaMaxWidth = 70f;

		public float GreenAreaMinWidth = 5f;

		public float SlideTime = 1f;

		public float SlideTimeMaxMultiplier = 2f;

		public float ValueDivisor = 300f;

		public float Tolerance = 0.01f;

		[Header("References")]
		public Canvas Canvas;

		public RectTransform Container;

		public ItemSlotUI[] Slots;

		public RectTransform[] GreenAreas;

		public Animation TutorialAnimation;

		public RectTransform TutorialContainer;

		public RectTransform SliderContainer;

		public Slider Slider;

		public InputPrompt InputPrompt;

		public UnityEvent onFail;

		public UnityEvent onStop;

		public UnityEvent onHitGreen;

		private NPC npc;

		private bool isSliding;

		private int slideDirection = 1;

		private float sliderPosition;

		private float slideTimeMultiplier = 1f;

		private bool isFail;

		public bool IsOpen { get; private set; }

		public bool TutorialOpen { get; private set; }

		protected override void Awake()
		{
			base.Awake();
			GameInput.RegisterExitListener(Exit, 3);
			Canvas.enabled = false;
			Container.gameObject.SetActive(value: false);
		}

		public void Open(NPC _npc)
		{
			IsOpen = true;
			npc = _npc;
			npc.SetIsBeingPickPocketed(pickpocketed: true);
			Singleton<GameInput>.Instance.ExitAll();
			PlayerSingleton<PlayerInventory>.Instance.SetEquippingEnabled(enabled: false);
			PlayerSingleton<PlayerMovement>.Instance.canMove = false;
			PlayerSingleton<PlayerCamera>.Instance.SetCanLook(c: false);
			PlayerSingleton<PlayerCamera>.Instance.AddActiveUIElement(base.name);
			PlayerSingleton<PlayerCamera>.Instance.FreeMouse();
			PlayerSingleton<PlayerCamera>.Instance.SetDoFActive(active: true, 0f);
			Singleton<ItemUIManager>.Instance.SetDraggingEnabled(enabled: true);
			Singleton<HUD>.Instance.SetCrosshairVisible(vis: false);
			Player.Local.VisualState.ApplyState("pickpocketing", PlayerVisualState.EVisualState.Pickpocketing);
			ItemSlot[] array = _npc.Inventory.ItemSlots.ToArray();
			array.Shuffle();
			for (int i = 0; i < Slots.Length; i++)
			{
				if (i < array.Length)
				{
					Slots[i].AssignSlot(array[i]);
				}
				else
				{
					Slots[i].ClearSlot();
				}
			}
			Singleton<ItemUIManager>.Instance.EnableQuickMove(new List<ItemSlot>(PlayerSingleton<PlayerInventory>.Instance.GetAllInventorySlots()), array.ToList());
			for (int j = 0; j < Slots.Length; j++)
			{
				ItemSlotUI itemSlotUI = Slots[j];
				SetSlotLocked(j, locked: true);
				if (itemSlotUI.assignedSlot == null || itemSlotUI.assignedSlot.Quantity == 0)
				{
					GreenAreas[j].gameObject.SetActive(value: false);
					continue;
				}
				float monetaryValue = itemSlotUI.assignedSlot.ItemInstance.GetMonetaryValue();
				float num = Mathf.Lerp(GreenAreaMaxWidth, GreenAreaMinWidth, Mathf.Pow(Mathf.Clamp01(monetaryValue / ValueDivisor), 0.3f));
				if (Player.Local.Sneaky)
				{
					num *= 1.5f;
				}
				RectTransform rectTransform = GreenAreas[j];
				rectTransform.sizeDelta = new Vector2(num, rectTransform.sizeDelta.y);
				rectTransform.gameObject.SetActive(value: true);
				rectTransform.anchoredPosition = new Vector2(37.5f + 90f * (float)j, rectTransform.anchoredPosition.y);
			}
			InputPrompt.SetLabel("Stop Arrow");
			isFail = false;
			isSliding = true;
			sliderPosition = 0f;
			slideDirection = 1;
			slideTimeMultiplier = 1f;
			Canvas.enabled = true;
			Container.gameObject.SetActive(value: true);
		}

		private void Exit(ExitAction action)
		{
			if (!action.used && IsOpen && action.exitType == ExitType.Escape)
			{
				action.used = true;
				Close();
			}
		}

		private void Update()
		{
			if (!IsOpen || isFail)
			{
				return;
			}
			if (Player.Local.CrimeData.CurrentPursuitLevel != 0)
			{
				Close();
				return;
			}
			if (GameInput.GetButtonDown(GameInput.ButtonCode.Jump))
			{
				if (isSliding)
				{
					StopArrow();
				}
				else
				{
					InputPrompt.SetLabel("Stop Arrow");
					isSliding = true;
					if (GetHoveredSlot()?.assignedSlot != null)
					{
						GreenAreas[Slots.IndexOf(GetHoveredSlot())].gameObject.SetActive(value: false);
					}
				}
			}
			if (isSliding)
			{
				slideTimeMultiplier = Mathf.Clamp(slideTimeMultiplier + Time.deltaTime / 20f, 0f, SlideTimeMaxMultiplier);
				if (slideDirection == 1)
				{
					sliderPosition = Mathf.Clamp01(sliderPosition + Time.deltaTime / SlideTime * slideTimeMultiplier);
					if (sliderPosition >= 1f)
					{
						slideDirection = -1;
					}
				}
				else
				{
					sliderPosition = Mathf.Clamp01(sliderPosition - Time.deltaTime / SlideTime * slideTimeMultiplier);
					if (sliderPosition <= 0f)
					{
						slideDirection = 1;
					}
				}
			}
			Slider.value = sliderPosition;
		}

		private void StopArrow()
		{
			if (onStop != null)
			{
				onStop.Invoke();
			}
			isSliding = false;
			ItemSlotUI hoveredSlot = GetHoveredSlot();
			InputPrompt.SetLabel("Continue");
			if (hoveredSlot != null)
			{
				NetworkSingleton<LevelManager>.Instance.AddXP(2);
				SetSlotLocked(Slots.IndexOf(hoveredSlot), locked: false);
				if (onHitGreen != null)
				{
					onHitGreen.Invoke();
				}
			}
			else
			{
				Fail();
			}
		}

		public void SetSlotLocked(int index, bool locked)
		{
			Slots[index].Rect.Find("Locked").gameObject.SetActive(locked);
			Slots[index].assignedSlot.SetIsAddLocked(locked);
			Slots[index].assignedSlot.SetIsRemovalLocked(locked);
		}

		private ItemSlotUI GetHoveredSlot()
		{
			for (int i = 0; i < GreenAreas.Length; i++)
			{
				if (GreenAreas[i].gameObject.activeSelf)
				{
					float num = GetGreenAreaNormalizedPosition(i) - GetGreenAreaNormalizedWidth(i) / 2f;
					float num2 = GetGreenAreaNormalizedPosition(i) + GetGreenAreaNormalizedWidth(i) / 2f;
					if (Slider.value >= num - Tolerance && Slider.value <= num2 + Tolerance)
					{
						return Slots[i];
					}
				}
			}
			return null;
		}

		private void Fail()
		{
			isFail = true;
			if (onFail != null)
			{
				onFail.Invoke();
			}
			StartCoroutine(FailCoroutine());
			IEnumerator FailCoroutine()
			{
				yield return new WaitForSeconds(0.9f);
				if (IsOpen)
				{
					Close();
				}
			}
		}

		public void Close()
		{
			IsOpen = false;
			Canvas.enabled = false;
			Container.gameObject.SetActive(value: false);
			for (int i = 0; i < Slots.Length; i++)
			{
				if (Slots[i].assignedSlot != null)
				{
					Slots[i].assignedSlot.SetIsRemovalLocked(locked: false);
				}
			}
			PlayerSingleton<PlayerMovement>.Instance.canMove = true;
			PlayerSingleton<PlayerCamera>.Instance.SetCanLook(c: true);
			PlayerSingleton<PlayerCamera>.Instance.RemoveActiveUIElement(base.name);
			PlayerSingleton<PlayerCamera>.Instance.LockMouse();
			PlayerSingleton<PlayerCamera>.Instance.SetDoFActive(active: false, 0f);
			Player.Local.VisualState.RemoveState("pickpocketing");
			PlayerSingleton<PlayerInventory>.Instance.SetEquippingEnabled(enabled: true);
			Singleton<ItemUIManager>.Instance.SetDraggingEnabled(enabled: false);
			Singleton<HUD>.Instance.SetCrosshairVisible(vis: true);
			npc.SetIsBeingPickPocketed(pickpocketed: false);
			if (isFail)
			{
				npc.responses.PlayerFailedPickpocket(Player.Local);
				npc.Inventory.ExpirePickpocket();
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

		private float GetGreenAreaNormalizedPosition(int index)
		{
			return GreenAreas[index].anchoredPosition.x / SliderContainer.sizeDelta.x;
		}

		private float GetGreenAreaNormalizedWidth(int index)
		{
			return GreenAreas[index].sizeDelta.x / SliderContainer.sizeDelta.x;
		}
	}
}
