using System.Collections.Generic;
using ScheduleOne.DevUtilities;
using ScheduleOne.ItemFramework;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Product;
using ScheduleOne.Properties;
using ScheduleOne.Storage;
using ScheduleOne.UI.Compass;
using ScheduleOne.UI.Items;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ScheduleOne.UI
{
	public class CreateMixInterface : Singleton<CreateMixInterface>
	{
		public const int BEAN_REQUIREMENT = 5;

		[Header("References")]
		public Canvas Canvas;

		public ItemSlotUI BeansSlot;

		public ItemSlotUI ProductSlot;

		public ItemSlotUI MixerSlot;

		public ItemSlotUI OutputSlot;

		public Image OutputIcon;

		public Button BeginButton;

		public WorldStorageEntity Storage;

		public TextMeshProUGUI ProductPropertiesLabel;

		public TextMeshProUGUI OutputPropertiesLabel;

		public TextMeshProUGUI BeanProblemLabel;

		public TextMeshProUGUI ProductProblemLabel;

		public TextMeshProUGUI MixerProblemLabel;

		public TextMeshProUGUI OutputProblemLabel;

		public Transform CameraPosition;

		public RectTransform UnknownOutputIcon;

		public UnityEvent onOpen;

		public UnityEvent onClose;

		public bool IsOpen { get; private set; }

		private ItemSlot beanSlot => Storage.ItemSlots[0];

		private ItemSlot mixerSlot => Storage.ItemSlots[1];

		private ItemSlot outputSlot => Storage.ItemSlots[2];

		private ItemSlot productSlot => Storage.ItemSlots[3];

		protected override void Awake()
		{
			base.Awake();
			Canvas.enabled = false;
			BeansSlot.AssignSlot(beanSlot);
			MixerSlot.AssignSlot(mixerSlot);
			OutputSlot.AssignSlot(outputSlot);
			ProductSlot.AssignSlot(productSlot);
			beanSlot.AddFilter(new ItemFilter_ID(new List<string> { "megabean" }));
			productSlot.AddFilter(new ItemFilter_Category(new List<EItemCategory> { EItemCategory.Product }));
			outputSlot.SetIsAddLocked(locked: true);
			Storage.onContentsChanged.AddListener(ContentsChanged);
			BeginButton.onClick.AddListener(BeginPressed);
			GameInput.RegisterExitListener(Exit, 3);
		}

		private void Exit(ExitAction action)
		{
			if (!action.used && IsOpen && action.exitType == ExitType.Escape)
			{
				action.used = true;
				Close();
			}
		}

		public void Open()
		{
			IsOpen = true;
			Canvas.enabled = true;
			Singleton<InputPromptsCanvas>.Instance.LoadModule("exitonly");
			PlayerSingleton<PlayerInventory>.Instance.SetInventoryEnabled(enabled: true);
			PlayerSingleton<PlayerInventory>.Instance.SetEquippingEnabled(enabled: false);
			PlayerSingleton<PlayerMovement>.Instance.canMove = false;
			Singleton<HUD>.Instance.SetCrosshairVisible(vis: false);
			PlayerSingleton<PlayerCamera>.Instance.FreeMouse();
			PlayerSingleton<PlayerCamera>.Instance.SetCanLook(c: false);
			PlayerSingleton<PlayerCamera>.Instance.OverrideFOV(60f, 0.2f);
			PlayerSingleton<PlayerCamera>.Instance.AddActiveUIElement(base.name);
			PlayerSingleton<PlayerCamera>.Instance.SetDoFActive(active: true, 0f);
			Singleton<ItemUIManager>.Instance.SetDraggingEnabled(enabled: true);
			List<ItemSlot> secondarySlots = new List<ItemSlot> { beanSlot, productSlot, mixerSlot };
			Singleton<ItemUIManager>.Instance.EnableQuickMove(PlayerSingleton<PlayerInventory>.Instance.GetAllInventorySlots(), secondarySlots);
			Singleton<CompassManager>.Instance.SetVisible(visible: false);
			ContentsChanged();
		}

		private void ContentsChanged()
		{
			UpdateCanBegin();
			UpdateOutput();
		}

		private void UpdateCanBegin()
		{
			BeanProblemLabel.enabled = !HasBeans();
			ProductProblemLabel.enabled = !HasProduct();
			if (HasProduct())
			{
				ProductDefinition productDefinition = productSlot.ItemInstance.Definition as ProductDefinition;
				ProductPropertiesLabel.text = GetPropertyListString(productDefinition.Properties);
				ProductPropertiesLabel.enabled = true;
			}
			else
			{
				ProductPropertiesLabel.enabled = false;
			}
			if (mixerSlot.Quantity == 0)
			{
				MixerProblemLabel.text = "Required";
				MixerProblemLabel.enabled = true;
			}
			else if (!HasMixer())
			{
				MixerProblemLabel.text = "Invalid mixer";
				MixerProblemLabel.enabled = true;
			}
			else
			{
				MixerProblemLabel.enabled = false;
			}
			BeginButton.interactable = CanBegin();
		}

		private void UpdateOutput()
		{
			ProductDefinition product = GetProduct();
			PropertyItemDefinition mixer = GetMixer();
			if (product != null && mixer != null)
			{
				List<ScheduleOne.Properties.Property> outputProperties = GetOutputProperties(product, mixer);
				ProductDefinition knownProduct = NetworkSingleton<ProductManager>.Instance.GetKnownProduct(product.DrugTypes[0].DrugType, outputProperties);
				if (knownProduct == null)
				{
					OutputIcon.sprite = product.Icon;
					OutputIcon.color = Color.black;
					OutputIcon.enabled = true;
					UnknownOutputIcon.gameObject.SetActive(value: true);
					List<Color32> list = new List<Color32>();
					OutputPropertiesLabel.text = string.Empty;
					for (int i = 0; i < outputProperties.Count; i++)
					{
						if (OutputPropertiesLabel.text.Length > 0)
						{
							OutputPropertiesLabel.text += "\n";
						}
						if (product.Properties.Contains(outputProperties[i]))
						{
							OutputPropertiesLabel.text += GetPropertyString(outputProperties[i]);
						}
						else
						{
							list.Add(outputProperties[i].LabelColor);
						}
					}
					for (int j = 0; j < list.Count; j++)
					{
						if (OutputPropertiesLabel.text.Length > 0)
						{
							OutputPropertiesLabel.text += "\n";
						}
						TextMeshProUGUI outputPropertiesLabel = OutputPropertiesLabel;
						outputPropertiesLabel.text = outputPropertiesLabel.text + "<color=#" + ColorUtility.ToHtmlStringRGBA(list[j]) + ">• ?</color>";
					}
					OutputPropertiesLabel.enabled = true;
					OutputProblemLabel.enabled = false;
					LayoutRebuilder.ForceRebuildLayoutImmediate(OutputPropertiesLabel.rectTransform);
				}
				else
				{
					OutputIcon.sprite = knownProduct.Icon;
					OutputIcon.color = Color.white;
					OutputIcon.enabled = true;
					UnknownOutputIcon.gameObject.SetActive(value: false);
					OutputPropertiesLabel.text = GetPropertyListString(knownProduct.Properties);
					OutputPropertiesLabel.enabled = true;
					OutputProblemLabel.text = "Mix already known. ";
					OutputProblemLabel.enabled = true;
					LayoutRebuilder.ForceRebuildLayoutImmediate(OutputPropertiesLabel.rectTransform);
				}
			}
			else
			{
				OutputIcon.enabled = false;
				OutputPropertiesLabel.enabled = false;
				OutputProblemLabel.enabled = false;
			}
		}

		private void BeginPressed()
		{
			if (CanBegin())
			{
				ProductDefinition product = GetProduct();
				NewMixOperation operation = new NewMixOperation(ingredientID: GetMixer().ID, productID: product.ID);
				NetworkSingleton<ProductManager>.Instance.SendMixOperation(operation, complete: false);
				beanSlot.ChangeQuantity(-5);
				productSlot.ChangeQuantity(-1);
				mixerSlot.ChangeQuantity(-1);
				Close();
			}
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
			ProductDefinition product = GetProduct();
			PropertyItemDefinition mixer = GetMixer();
			if (product != null && mixer != null)
			{
				List<ScheduleOne.Properties.Property> outputProperties = GetOutputProperties(product, mixer);
				knownProduct = NetworkSingleton<ProductManager>.Instance.GetKnownProduct(product.DrugTypes[0].DrugType, outputProperties);
			}
			return knownProduct != null;
		}

		private string GetPropertyListString(List<ScheduleOne.Properties.Property> properties)
		{
			ProductPropertiesLabel.text = "";
			for (int i = 0; i < properties.Count; i++)
			{
				if (i > 0)
				{
					ProductPropertiesLabel.text += "\n";
				}
				ProductPropertiesLabel.text += GetPropertyString(properties[i]);
			}
			return ProductPropertiesLabel.text;
		}

		private string GetPropertyString(ScheduleOne.Properties.Property property)
		{
			return "<color=#" + ColorUtility.ToHtmlStringRGBA(property.LabelColor) + ">• " + property.Name + "</color>";
		}

		private bool CanBegin()
		{
			ProductDefinition knownProduct;
			if (HasBeans() && HasProduct() && HasMixer())
			{
				return !IsOutputKnown(out knownProduct);
			}
			return false;
		}

		public void Close()
		{
			IsOpen = false;
			Canvas.enabled = false;
			if (beanSlot.ItemInstance != null)
			{
				PlayerSingleton<PlayerInventory>.Instance.AddItemToInventory(beanSlot.ItemInstance.GetCopy());
				beanSlot.ClearStoredInstance();
			}
			if (productSlot.ItemInstance != null)
			{
				PlayerSingleton<PlayerInventory>.Instance.AddItemToInventory(productSlot.ItemInstance.GetCopy());
				productSlot.ClearStoredInstance();
			}
			if (mixerSlot.ItemInstance != null)
			{
				PlayerSingleton<PlayerInventory>.Instance.AddItemToInventory(mixerSlot.ItemInstance.GetCopy());
				mixerSlot.ClearStoredInstance();
			}
			Singleton<InputPromptsCanvas>.Instance.UnloadModule();
			PlayerSingleton<PlayerInventory>.Instance.SetInventoryEnabled(enabled: true);
			PlayerSingleton<PlayerInventory>.Instance.SetEquippingEnabled(enabled: true);
			PlayerSingleton<PlayerMovement>.Instance.canMove = true;
			Singleton<HUD>.Instance.SetCrosshairVisible(vis: true);
			PlayerSingleton<PlayerCamera>.Instance.LockMouse();
			PlayerSingleton<PlayerCamera>.Instance.StopTransformOverride(0.2f, reenableCameraLook: true, returnToOriginalRotation: false);
			PlayerSingleton<PlayerCamera>.Instance.StopFOVOverride(0.2f);
			PlayerSingleton<PlayerCamera>.Instance.RemoveActiveUIElement(base.name);
			PlayerSingleton<PlayerCamera>.Instance.SetDoFActive(active: false, 0f);
			Singleton<ItemUIManager>.Instance.SetDraggingEnabled(enabled: false);
			Singleton<CompassManager>.Instance.SetVisible(visible: true);
		}

		private bool HasProduct()
		{
			return GetProduct() != null;
		}

		private bool HasBeans()
		{
			return beanSlot.Quantity >= 5;
		}

		private bool HasMixer()
		{
			return GetMixer() != null;
		}

		private ProductDefinition GetProduct()
		{
			if (productSlot.ItemInstance != null)
			{
				return productSlot.ItemInstance.Definition as ProductDefinition;
			}
			return null;
		}

		private PropertyItemDefinition GetMixer()
		{
			if (mixerSlot.ItemInstance != null)
			{
				PropertyItemDefinition propertyItemDefinition = mixerSlot.ItemInstance.Definition as PropertyItemDefinition;
				if (propertyItemDefinition != null && NetworkSingleton<ProductManager>.Instance.ValidMixIngredients.Contains(propertyItemDefinition))
				{
					return propertyItemDefinition;
				}
			}
			return null;
		}
	}
}
