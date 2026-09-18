using System;
using System.Collections.Generic;
using ScheduleOne.DevUtilities;
using ScheduleOne.ItemFramework;
using ScheduleOne.ObjectScripts;
using ScheduleOne.PlayerScripts;
using ScheduleOne.PlayerTasks.Tasks;
using ScheduleOne.Product;
using ScheduleOne.Properties;
using ScheduleOne.StationFramework;
using ScheduleOne.UI.Compass;
using ScheduleOne.UI.Items;
using ScheduleOne.Variables;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ScheduleOne.UI.Stations
{
	public class MixingStationCanvas : Singleton<MixingStationCanvas>
	{
		[Header("Prefabs")]
		public StationRecipeEntry RecipeEntryPrefab;

		[Header("References")]
		public Canvas Canvas;

		public RectTransform Container;

		public ItemSlotUI ProductSlotUI;

		public TextMeshProUGUI ProductPropertiesLabel;

		public ItemSlotUI IngredientSlotUI;

		public TextMeshProUGUI IngredientProblemLabel;

		public ItemSlotUI PreviewSlotUI;

		public Image PreviewIcon;

		public TextMeshProUGUI PreviewLabel;

		public RectTransform UnknownOutputIcon;

		public TextMeshProUGUI PreviewPropertiesLabel;

		public ItemSlotUI OutputSlotUI;

		public TextMeshProUGUI InstructionLabel;

		public RectTransform TitleContainer;

		public RectTransform MainContainer;

		public Button BeginButton;

		public RectTransform ProductHint;

		public RectTransform MixerHint;

		private StationRecipe selectedRecipe;

		public bool isOpen { get; protected set; }

		public MixingStation MixingStation { get; protected set; }

		protected override void Awake()
		{
			base.Awake();
			BeginButton.onClick.AddListener(BeginButtonPressed);
		}

		protected override void Start()
		{
			base.Start();
			isOpen = false;
			Canvas.enabled = false;
			Container.gameObject.SetActive(value: true);
			GameInput.RegisterExitListener(Exit, 4);
		}

		private void Exit(ExitAction action)
		{
			if (!action.used && isOpen && action.exitType == ExitType.Escape)
			{
				action.used = true;
				if (Singleton<NewMixScreen>.Instance.IsOpen)
				{
					Singleton<NewMixScreen>.Instance.Close();
				}
				Close();
			}
		}

		protected virtual void Update()
		{
			if (isOpen)
			{
				if (BeginButton.interactable && GameInput.GetButtonDown(GameInput.ButtonCode.Submit))
				{
					BeginButtonPressed();
					return;
				}
				UpdateInput();
				UpdateUI();
			}
		}

		private void UpdateUI()
		{
		}

		private void UpdateInput()
		{
			UpdateDisplayMode();
			UpdateInstruction();
		}

		public void Open(MixingStation station)
		{
			isOpen = true;
			MixingStation = station;
			UpdateUI();
			Canvas.enabled = true;
			Container.gameObject.SetActive(value: true);
			if (!NetworkSingleton<VariableDatabase>.Instance.GetValue<bool>("MixingHintsShown"))
			{
				MixerHint.gameObject.SetActive(value: true);
				ProductHint.gameObject.SetActive(value: true);
				NetworkSingleton<VariableDatabase>.Instance.SetVariableValue("MixingHintsShown", true.ToString());
			}
			if (PlayerSingleton<PlayerCamera>.InstanceExists)
			{
				PlayerSingleton<PlayerCamera>.Instance.AddActiveUIElement(base.name);
			}
			ProductSlotUI.AssignSlot(station.ProductSlot);
			IngredientSlotUI.AssignSlot(station.MixerSlot);
			OutputSlotUI.AssignSlot(station.OutputSlot);
			ItemSlot productSlot = station.ProductSlot;
			productSlot.onItemDataChanged = (Action)Delegate.Combine(productSlot.onItemDataChanged, new Action(StationContentsChanged));
			ItemSlot mixerSlot = station.MixerSlot;
			mixerSlot.onItemDataChanged = (Action)Delegate.Combine(mixerSlot.onItemDataChanged, new Action(StationContentsChanged));
			ItemSlot outputSlot = station.OutputSlot;
			outputSlot.onItemDataChanged = (Action)Delegate.Combine(outputSlot.onItemDataChanged, new Action(StationContentsChanged));
			Singleton<InputPromptsCanvas>.Instance.LoadModule("exitonly");
			PlayerSingleton<PlayerInventory>.Instance.SetInventoryEnabled(enabled: true);
			PlayerSingleton<PlayerInventory>.Instance.SetEquippingEnabled(enabled: false);
			Singleton<ItemUIManager>.Instance.SetDraggingEnabled(enabled: true);
			List<ItemSlot> list = new List<ItemSlot>();
			list.Add(station.ProductSlot);
			list.Add(station.MixerSlot);
			list.Add(station.OutputSlot);
			Singleton<ItemUIManager>.Instance.EnableQuickMove(PlayerSingleton<PlayerInventory>.Instance.GetAllInventorySlots(), list);
			UpdateDisplayMode();
			UpdateInstruction();
			UpdatePreview();
			UpdateBeginButton();
			if (station.IsMixingDone && !station.CurrentMixOperation.IsOutputKnown(out var _))
			{
				station.CurrentMixOperation.GetOutput(out var properties);
				ProductDefinition item = Registry.GetItem<ProductDefinition>(MixingStation.CurrentMixOperation.ProductID);
				station.DiscoveryBox.ShowProduct(item, properties);
				station.DiscoveryBox.transform.SetParent(PlayerSingleton<PlayerCamera>.Instance.transform);
				station.DiscoveryBox.transform.localPosition = station.DiscoveryBoxOffset;
				station.DiscoveryBox.transform.localRotation = station.DiscoveryBoxRotation;
				float productMarketValue = ProductManager.CalculateProductValue(item.BasePrice, properties);
				Singleton<NewMixScreen>.Instance.Open(properties, item.DrugType, productMarketValue);
				NewMixScreen newMixScreen = Singleton<NewMixScreen>.Instance;
				newMixScreen.onMixNamed = (Action<string>)Delegate.Remove(newMixScreen.onMixNamed, new Action<string>(MixNamed));
				NewMixScreen newMixScreen2 = Singleton<NewMixScreen>.Instance;
				newMixScreen2.onMixNamed = (Action<string>)Delegate.Combine(newMixScreen2.onMixNamed, new Action<string>(MixNamed));
			}
			else
			{
				station.onMixDone.RemoveListener(MixingDone);
				station.onMixDone.AddListener(MixingDone);
			}
			Singleton<CompassManager>.Instance.SetVisible(visible: false);
		}

		public void Close(bool enablePlayerControl = true)
		{
			isOpen = false;
			Canvas.enabled = false;
			Container.gameObject.SetActive(value: false);
			if (PlayerSingleton<PlayerCamera>.InstanceExists)
			{
				PlayerSingleton<PlayerCamera>.Instance.RemoveActiveUIElement(base.name);
			}
			ProductSlotUI.ClearSlot();
			IngredientSlotUI.ClearSlot();
			OutputSlotUI.ClearSlot();
			ItemSlot productSlot = MixingStation.ProductSlot;
			productSlot.onItemDataChanged = (Action)Delegate.Remove(productSlot.onItemDataChanged, new Action(StationContentsChanged));
			ItemSlot mixerSlot = MixingStation.MixerSlot;
			mixerSlot.onItemDataChanged = (Action)Delegate.Remove(mixerSlot.onItemDataChanged, new Action(StationContentsChanged));
			ItemSlot outputSlot = MixingStation.OutputSlot;
			outputSlot.onItemDataChanged = (Action)Delegate.Remove(outputSlot.onItemDataChanged, new Action(StationContentsChanged));
			MixingStation.onMixDone.RemoveListener(MixingDone);
			Singleton<InputPromptsCanvas>.Instance.UnloadModule();
			Singleton<ItemUIManager>.Instance.SetDraggingEnabled(enabled: false);
			if (enablePlayerControl)
			{
				MixingStation.Close();
				MixingStation = null;
			}
		}

		private void MixingDone()
		{
			if (MixingStation.IsMixingDone && !MixingStation.CurrentMixOperation.IsOutputKnown(out var _))
			{
				MixingStation.CurrentMixOperation.GetOutput(out var properties);
				ProductDefinition item = Registry.GetItem<ProductDefinition>(MixingStation.CurrentMixOperation.ProductID);
				MixingStation.DiscoveryBox.ShowProduct(item, properties);
				MixingStation.DiscoveryBox.transform.SetParent(PlayerSingleton<PlayerCamera>.Instance.transform);
				MixingStation.DiscoveryBox.transform.localPosition = MixingStation.DiscoveryBoxOffset;
				MixingStation.DiscoveryBox.transform.localRotation = MixingStation.DiscoveryBoxRotation;
				float productMarketValue = ProductManager.CalculateProductValue(item.BasePrice, properties);
				Singleton<NewMixScreen>.Instance.Open(properties, item.DrugType, productMarketValue);
				NewMixScreen newMixScreen = Singleton<NewMixScreen>.Instance;
				newMixScreen.onMixNamed = (Action<string>)Delegate.Remove(newMixScreen.onMixNamed, new Action<string>(MixNamed));
				NewMixScreen newMixScreen2 = Singleton<NewMixScreen>.Instance;
				newMixScreen2.onMixNamed = (Action<string>)Delegate.Combine(newMixScreen2.onMixNamed, new Action<string>(MixNamed));
			}
			UpdateDisplayMode();
			UpdateInstruction();
			UpdatePreview();
			UpdateBeginButton();
		}

		private void StationContentsChanged()
		{
			UpdateDisplayMode();
			UpdatePreview();
			UpdateBeginButton();
			if (MixingStation.ProductSlot.Quantity > 0)
			{
				ProductHint.gameObject.SetActive(value: false);
			}
			if (MixingStation.MixerSlot.Quantity > 0)
			{
				MixerHint.gameObject.SetActive(value: false);
			}
		}

		private void UpdateDisplayMode()
		{
			TitleContainer.gameObject.SetActive(value: true);
			MainContainer.gameObject.SetActive(value: true);
			OutputSlotUI.gameObject.SetActive(value: false);
			ProductDefinition knownProduct;
			if (MixingStation.OutputSlot.Quantity > 0)
			{
				MainContainer.gameObject.SetActive(value: false);
				OutputSlotUI.gameObject.SetActive(value: true);
			}
			else if (MixingStation.CurrentMixOperation != null && MixingStation.IsMixingDone && !MixingStation.CurrentMixOperation.IsOutputKnown(out knownProduct))
			{
				TitleContainer.gameObject.SetActive(value: false);
				MainContainer.gameObject.SetActive(value: false);
				OutputSlotUI.gameObject.SetActive(value: false);
			}
		}

		private void UpdateInstruction()
		{
			InstructionLabel.enabled = true;
			if (MixingStation.OutputSlot.Quantity > 0)
			{
				InstructionLabel.text = "Collect output";
			}
			else if (MixingStation.CurrentMixOperation != null)
			{
				InstructionLabel.text = "Mixing in progress...";
			}
			else if (!MixingStation.CanStartMix())
			{
				InstructionLabel.text = "Insert unpackaged product and mixing ingredient";
			}
			else
			{
				InstructionLabel.enabled = false;
			}
		}

		private void UpdatePreview()
		{
			ProductDefinition product = MixingStation.GetProduct();
			PropertyItemDefinition mixer = MixingStation.GetMixer();
			if (product != null)
			{
				ProductPropertiesLabel.text = GetPropertyListString(product.Properties);
				ProductPropertiesLabel.enabled = true;
			}
			else
			{
				ProductPropertiesLabel.enabled = false;
			}
			if (mixer == null && MixingStation.MixerSlot.Quantity > 0)
			{
				IngredientProblemLabel.enabled = true;
			}
			else
			{
				IngredientProblemLabel.enabled = false;
			}
			UnknownOutputIcon.gameObject.SetActive(value: false);
			if (product != null && mixer != null)
			{
				List<ScheduleOne.Properties.Property> outputProperties = GetOutputProperties(product, mixer);
				ProductDefinition knownProduct = NetworkSingleton<ProductManager>.Instance.GetKnownProduct(product.DrugTypes[0].DrugType, outputProperties);
				if (knownProduct == null)
				{
					PreviewIcon.sprite = product.Icon;
					PreviewIcon.color = Color.black;
					PreviewIcon.enabled = true;
					PreviewLabel.text = "Unknown";
					PreviewLabel.enabled = true;
					UnknownOutputIcon.gameObject.SetActive(value: true);
					PreviewPropertiesLabel.text = string.Empty;
					for (int i = 0; i < outputProperties.Count; i++)
					{
						if (product.Properties.Contains(outputProperties[i]))
						{
							if (PreviewPropertiesLabel.text.Length > 0)
							{
								PreviewPropertiesLabel.text += "\n";
							}
							PreviewPropertiesLabel.text += GetPropertyString(outputProperties[i]);
							continue;
						}
						if (PreviewPropertiesLabel.text.Length > 0)
						{
							PreviewPropertiesLabel.text += "\n";
						}
						TextMeshProUGUI previewPropertiesLabel = PreviewPropertiesLabel;
						previewPropertiesLabel.text = previewPropertiesLabel.text + "<color=#" + ColorUtility.ToHtmlStringRGBA(outputProperties[i].LabelColor) + ">• ?</color>";
					}
					PreviewPropertiesLabel.enabled = true;
					LayoutRebuilder.ForceRebuildLayoutImmediate(PreviewPropertiesLabel.rectTransform);
				}
				else
				{
					PreviewIcon.sprite = knownProduct.Icon;
					PreviewIcon.color = Color.white;
					PreviewIcon.enabled = true;
					PreviewLabel.text = knownProduct.Name;
					PreviewLabel.enabled = true;
					UnknownOutputIcon.gameObject.SetActive(value: false);
					PreviewPropertiesLabel.text = GetPropertyListString(knownProduct.Properties);
					PreviewPropertiesLabel.enabled = true;
					LayoutRebuilder.ForceRebuildLayoutImmediate(PreviewPropertiesLabel.rectTransform);
				}
			}
			else
			{
				PreviewIcon.enabled = false;
				PreviewLabel.enabled = false;
				PreviewPropertiesLabel.enabled = false;
			}
		}

		private string GetPropertyListString(List<ScheduleOne.Properties.Property> properties)
		{
			string text = "";
			for (int i = 0; i < properties.Count; i++)
			{
				if (i > 0)
				{
					text += "\n";
				}
				text += GetPropertyString(properties[i]);
			}
			return text;
		}

		private string GetPropertyString(ScheduleOne.Properties.Property property)
		{
			return "<color=#" + ColorUtility.ToHtmlStringRGBA(property.LabelColor) + ">• " + property.Name + "</color>";
		}

		private List<ScheduleOne.Properties.Property> GetOutputProperties(ProductDefinition product, PropertyItemDefinition mixer)
		{
			List<ScheduleOne.Properties.Property> properties = product.Properties;
			List<ScheduleOne.Properties.Property> properties2 = mixer.Properties;
			return PropertyMixCalculator.MixProperties(properties, properties2[0], product.DrugType);
		}

		private bool IsOutputKnown(out ProductDefinition knownProduct)
		{
			knownProduct = null;
			ProductDefinition product = MixingStation.GetProduct();
			PropertyItemDefinition mixer = MixingStation.GetMixer();
			if (product != null && mixer != null)
			{
				List<ScheduleOne.Properties.Property> outputProperties = GetOutputProperties(product, mixer);
				knownProduct = NetworkSingleton<ProductManager>.Instance.GetKnownProduct(product.DrugTypes[0].DrugType, outputProperties);
			}
			return knownProduct != null;
		}

		private void UpdateBeginButton()
		{
			if (MixingStation.CurrentMixOperation != null || MixingStation.OutputSlot.Quantity > 0)
			{
				BeginButton.gameObject.SetActive(value: false);
				return;
			}
			BeginButton.gameObject.SetActive(value: true);
			BeginButton.interactable = MixingStation.CanStartMix();
		}

		public void BeginButtonPressed()
		{
			int mixQuantity = MixingStation.GetMixQuantity();
			if (mixQuantity > 0)
			{
				bool flag = false;
				if (Application.isEditor && UnityEngine.Input.GetKey(KeyCode.R))
				{
					flag = true;
				}
				if (MixingStation.RequiresIngredientInsertion && !flag)
				{
					MixingStation mixingStation = MixingStation;
					Close(enablePlayerControl: false);
					new UseMixingStationTask(mixingStation);
					return;
				}
				ProductItemInstance productItemInstance = MixingStation.ProductSlot.ItemInstance as ProductItemInstance;
				string iD = MixingStation.MixerSlot.ItemInstance.ID;
				MixingStation.ProductSlot.ChangeQuantity(-mixQuantity);
				MixingStation.MixerSlot.ChangeQuantity(-mixQuantity);
				StartMixOperation(new MixOperation(productItemInstance.ID, productItemInstance.Quality, iD, mixQuantity));
				Close();
			}
			else
			{
				Console.LogWarning("Failed to start mixing operation, not enough ingredients or output slot is full");
			}
		}

		public void StartMixOperation(MixOperation mixOperation)
		{
			MixingStation.SendMixingOperation(mixOperation, 0);
			NetworkSingleton<VariableDatabase>.Instance.SetVariableValue("Mixing_Operations_Started", (NetworkSingleton<VariableDatabase>.Instance.GetValue<float>("Mixing_Operations_Started") + 1f).ToString());
		}

		private void MixNamed(string mixName)
		{
			if (MixingStation == null)
			{
				Console.LogWarning("Mixing station is null, cannot finish mix operation");
				return;
			}
			if (MixingStation.CurrentMixOperation == null)
			{
				Console.LogWarning("Mixing station current mix operation is null, cannot finish mix operation");
				return;
			}
			NetworkSingleton<ProductManager>.Instance.FinishAndNameMix(MixingStation.CurrentMixOperation.ProductID, MixingStation.CurrentMixOperation.IngredientID, mixName);
			MixingStation.TryCreateOutputItems();
			MixingStation.DiscoveryBox.gameObject.SetActive(value: false);
			UpdateDisplayMode();
		}
	}
}
