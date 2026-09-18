using System;
using System.Collections.Generic;
using ScheduleOne.Audio;
using ScheduleOne.DevUtilities;
using ScheduleOne.Money;
using ScheduleOne.PlayerScripts;
using ScheduleOne.PlayerTasks;
using ScheduleOne.Product;
using ScheduleOne.Properties;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ScheduleOne.UI
{
	public class NewMixScreen : Singleton<NewMixScreen>
	{
		public const int MAX_PROPERTIES_DISPLAYED = 5;

		[Header("References")]
		[SerializeField]
		protected Canvas canvas;

		public RectTransform Container;

		[SerializeField]
		protected TMP_InputField nameInputField;

		[SerializeField]
		protected GameObject mixAlreadyExistsText;

		[SerializeField]
		protected RectTransform editIcon;

		[SerializeField]
		protected Button randomizeNameButton;

		[SerializeField]
		protected Button confirmButton;

		[SerializeField]
		protected TextMeshProUGUI PropertiesLabel;

		[SerializeField]
		protected TextMeshProUGUI MarketValueLabel;

		public AudioSourceController Sound;

		[Header("Prefabs")]
		[SerializeField]
		protected GameObject attributeEntryPrefab;

		[Header("Name Library")]
		[SerializeField]
		protected List<string> name1Library = new List<string>();

		[SerializeField]
		protected List<string> name2Library = new List<string>();

		public Action<string> onMixNamed;

		public bool IsOpen => canvas.enabled;

		protected override void Awake()
		{
			base.Awake();
			nameInputField.onValueChanged.AddListener(OnNameValueChanged);
			GameInput.RegisterExitListener(Exit, 3);
			canvas.enabled = false;
			Container.gameObject.SetActive(value: false);
		}

		private void Exit(ExitAction action)
		{
		}

		protected virtual void Update()
		{
			if (IsOpen && confirmButton.interactable && GameInput.GetButtonDown(GameInput.ButtonCode.Submit))
			{
				ConfirmButtonClicked();
			}
		}

		public void Open(List<ScheduleOne.Properties.Property> properties, EDrugType drugType, float productMarketValue)
		{
			canvas.enabled = true;
			Container.gameObject.SetActive(value: true);
			PlayerSingleton<PlayerCamera>.Instance.AddActiveUIElement(base.name);
			nameInputField.text = GenerateUniqueName(properties.ToArray(), drugType);
			Singleton<TaskManager>.Instance.PlayTaskCompleteSound();
			PropertiesLabel.text = string.Empty;
			for (int i = 0; i < properties.Count; i++)
			{
				ScheduleOne.Properties.Property property = properties[i];
				if (PropertiesLabel.text.Length > 0)
				{
					PropertiesLabel.text += "\n";
				}
				if (i == 4 && properties.Count > 5)
				{
					int num = properties.Count - 5 + 1;
					TextMeshProUGUI propertiesLabel = PropertiesLabel;
					propertiesLabel.text = propertiesLabel.text + "+ " + num + " more...";
					break;
				}
				TextMeshProUGUI propertiesLabel2 = PropertiesLabel;
				propertiesLabel2.text = propertiesLabel2.text + "<color=#" + ColorUtility.ToHtmlStringRGBA(property.LabelColor) + ">• " + property.Name + "</color>";
			}
			MarketValueLabel.text = "Market Value: <color=#54E717>" + MoneyManager.FormatAmount(productMarketValue) + "</color>";
		}

		public void Close()
		{
			canvas.enabled = false;
			Container.gameObject.SetActive(value: false);
			PlayerSingleton<PlayerCamera>.Instance.RemoveActiveUIElement(base.name);
		}

		public void RandomizeButtonClicked()
		{
			nameInputField.text = GenerateUniqueName();
		}

		public void ConfirmButtonClicked()
		{
			if (onMixNamed != null)
			{
				onMixNamed(nameInputField.text);
			}
			Sound.Play();
			RandomizeButtonClicked();
			Close();
		}

		public string GenerateUniqueName(ScheduleOne.Properties.Property[] properties = null, EDrugType drugType = EDrugType.Marijuana)
		{
			UnityEngine.Random.InitState((int)(Time.timeSinceLevelLoad * 10f));
			string text = name1Library[UnityEngine.Random.Range(0, name1Library.Count)];
			string text2 = name2Library[UnityEngine.Random.Range(0, name2Library.Count)];
			if (properties != null)
			{
				int num = 0;
				foreach (ScheduleOne.Properties.Property property in properties)
				{
					num += property.Name.GetHashCode() / 2000;
				}
				num += drugType.GetHashCode() / 1000;
				int value = num % name1Library.Count;
				int value2 = num / 2 % name2Library.Count;
				text = name1Library[Mathf.Clamp(value, 0, name1Library.Count)];
				text2 = name2Library[Mathf.Clamp(value2, 0, name2Library.Count)];
			}
			while (NetworkSingleton<ProductManager>.Instance.ProductNames.Contains(text + " " + text2))
			{
				text = name1Library[UnityEngine.Random.Range(0, name1Library.Count)];
				text2 = name2Library[UnityEngine.Random.Range(0, name2Library.Count)];
			}
			return text + " " + text2;
		}

		protected void RefreshNameButtons()
		{
			float num = nameInputField.textComponent.preferredWidth / 2f;
			float num2 = 20f;
			editIcon.anchoredPosition = new Vector2(num + num2, editIcon.anchoredPosition.y);
			randomizeNameButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f - num - num2, randomizeNameButton.GetComponent<RectTransform>().anchoredPosition.y);
		}

		public void OnNameValueChanged(string newVal)
		{
			if (NetworkSingleton<ProductManager>.Instance.ProductNames.Contains(nameInputField.text) || !ProductManager.IsMixNameValid(nameInputField.text))
			{
				mixAlreadyExistsText.gameObject.SetActive(value: true);
				confirmButton.interactable = false;
			}
			else
			{
				mixAlreadyExistsText.gameObject.SetActive(value: false);
				confirmButton.interactable = true;
			}
			RefreshNameButtons();
			Invoke("RefreshNameButtons", 1f / 60f);
		}
	}
}
