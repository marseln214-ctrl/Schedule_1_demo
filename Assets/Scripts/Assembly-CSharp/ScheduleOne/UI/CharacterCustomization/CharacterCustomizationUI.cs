using System.Collections;
using ScheduleOne.AvatarFramework;
using ScheduleOne.AvatarFramework.Customization;
using ScheduleOne.DevUtilities;
using ScheduleOne.PlayerScripts;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ScheduleOne.UI.CharacterCustomization
{
	public class CharacterCustomizationUI : MonoBehaviour
	{
		[Header("Settings")]
		public string Title = "Customize";

		public CharacterCustomizationCategory[] Categories;

		public bool LoadAvatarSettingsNaked;

		[Header("References")]
		public Canvas Canvas;

		public RectTransform MainContainer;

		public RectTransform MenuContainer;

		public TextMeshProUGUI TitleText;

		public RectTransform ButtonContainer;

		public Button ExitButton;

		public Slider RigRotationSlider;

		public Transform CameraPosition;

		public Transform RigContainer;

		public ScheduleOne.AvatarFramework.Avatar AvatarRig;

		public RectTransform PreviewIndicator;

		[Header("Prefab")]
		public Button CategoryButtonPrefab;

		private float rigTargetY;

		private Coroutine openCloseRoutine;

		protected BasicAvatarSettings currentSettings;

		public bool IsOpen { get; private set; }

		public CharacterCustomizationCategory ActiveCategory { get; private set; }

		private void OnValidate()
		{
			Categories = GetComponentsInChildren<CharacterCustomizationCategory>(includeInactive: true);
			TitleText.text = Title;
		}

		private void Awake()
		{
			GameInput.RegisterExitListener(Exit, 1);
			RigRotationSlider.onValueChanged.AddListener(delegate(float value)
			{
				rigTargetY = value * 359f;
			});
			Categories = GetComponentsInChildren<CharacterCustomizationCategory>(includeInactive: true);
			TitleText.text = Title;
			ExitButton.onClick.AddListener(Close);
			for (int i = 0; i < Categories.Length; i++)
			{
				Button button = Object.Instantiate(CategoryButtonPrefab, ButtonContainer);
				button.GetComponentInChildren<TextMeshProUGUI>().text = Categories[i].CategoryName;
				CharacterCustomizationCategory category = Categories[i];
				button.onClick.AddListener(delegate
				{
					SetActiveCategory(category);
				});
			}
			IsOpen = false;
			Canvas.enabled = false;
			MainContainer.gameObject.SetActive(value: false);
			AvatarRig.gameObject.SetActive(value: false);
			SetActiveCategory(null);
		}

		protected virtual void Update()
		{
			if (IsOpen)
			{
				RigContainer.localEulerAngles = Vector3.Lerp(RigContainer.localEulerAngles, new Vector3(0f, rigTargetY, 0f), Time.deltaTime * 5f);
			}
		}

		public void SetActiveCategory(CharacterCustomizationCategory category)
		{
			ActiveCategory = category;
			for (int i = 0; i < Categories.Length; i++)
			{
				Categories[i].gameObject.SetActive(Categories[i] == category);
				if (Categories[i] == category)
				{
					Categories[i].Open();
				}
			}
			MenuContainer.gameObject.SetActive(category == null);
		}

		public virtual bool IsOptionCurrentlyApplied(CharacterCustomizationOption option)
		{
			return false;
		}

		public virtual void OptionSelected(CharacterCustomizationOption option)
		{
			PreviewIndicator.gameObject.SetActive(!option.purchased);
		}

		public virtual void OptionDeselected(CharacterCustomizationOption option)
		{
			Console.Log("Deselected option: " + option.Label);
		}

		public virtual void OptionPurchased(CharacterCustomizationOption option)
		{
			PreviewIndicator.gameObject.SetActive(value: false);
		}

		public virtual void Open()
		{
			_ = openCloseRoutine;
		}

		private void Exit(ExitAction action)
		{
			if (!action.used && IsOpen && action.exitType == ExitType.Escape)
			{
				action.used = true;
				if (ActiveCategory != null)
				{
					ActiveCategory.Back();
				}
				else
				{
					Close();
				}
			}
		}

		protected virtual void Close()
		{
			if (openCloseRoutine == null)
			{
				SetActiveCategory(null);
				IsOpen = false;
				Canvas.enabled = false;
				MainContainer.gameObject.SetActive(value: false);
				Player.Local.SendAppearance(currentSettings);
				openCloseRoutine = StartCoroutine(Close());
			}
			IEnumerator Close()
			{
				Singleton<BlackOverlay>.Instance.Open();
				yield return new WaitForSeconds(0.6f);
				AvatarRig.gameObject.SetActive(value: false);
				if (PlayerSingleton<PlayerCamera>.InstanceExists)
				{
					PlayerSingleton<PlayerCamera>.Instance.RemoveActiveUIElement(base.name);
					PlayerSingleton<PlayerCamera>.Instance.SetCanLook(c: true);
					PlayerSingleton<PlayerCamera>.Instance.LockMouse();
					PlayerSingleton<PlayerCamera>.Instance.StopTransformOverride(0f);
					PlayerSingleton<PlayerCamera>.Instance.StopFOVOverride(0f);
					PlayerSingleton<PlayerMovement>.Instance.canMove = true;
					PlayerSingleton<PlayerInventory>.Instance.SetInventoryEnabled(enabled: true);
					Singleton<InputPromptsCanvas>.Instance.UnloadModule();
				}
				Singleton<BlackOverlay>.Instance.Close();
				openCloseRoutine = null;
			}
		}
	}
}
