using System;
using ScheduleOne.DevUtilities;
using ScheduleOne.UI.Phone.ProductManagerApp;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ScheduleOne.Product
{
	public class ProductEntry : MonoBehaviour
	{
		public Color SelectedColor;

		public Color DeselectedColor;

		[Header("References")]
		public Button Button;

		public Image Frame;

		public Image Icon;

		public RectTransform Tick;

		public RectTransform Cross;

		public EventTrigger Trigger;

		public UnityEvent onHovered;

		public ProductDefinition Definition { get; private set; }

		public void Initialize(ProductDefinition definition)
		{
			Definition = definition;
			Icon.sprite = definition.Icon;
			Button.onClick.AddListener(Clicked);
			EventTrigger.Entry entry = new EventTrigger.Entry();
			entry.eventID = EventTriggerType.PointerEnter;
			entry.callback.AddListener(delegate
			{
				onHovered.Invoke();
			});
			Trigger.triggers.Add(entry);
			UpdateListed();
			UpdateDiscovered(Definition);
			ProductManager instance = NetworkSingleton<ProductManager>.Instance;
			instance.onProductDiscovered = (Action<ProductDefinition>)Delegate.Combine(instance.onProductDiscovered, new Action<ProductDefinition>(UpdateDiscovered));
			ProductManager instance2 = NetworkSingleton<ProductManager>.Instance;
			instance2.onProductListed = (Action<ProductDefinition>)Delegate.Combine(instance2.onProductListed, (Action<ProductDefinition>)delegate
			{
				UpdateListed();
			});
			ProductManager instance3 = NetworkSingleton<ProductManager>.Instance;
			instance3.onProductDelisted = (Action<ProductDefinition>)Delegate.Combine(instance3.onProductDelisted, (Action<ProductDefinition>)delegate
			{
				UpdateListed();
			});
		}

		private void Clicked()
		{
			PlayerSingleton<ProductManagerApp>.Instance.SelectProduct(this);
			UpdateListed();
		}

		public void UpdateListed()
		{
			if (ProductManager.ListedProducts.Contains(Definition))
			{
				Frame.color = SelectedColor;
				Tick.gameObject.SetActive(value: true);
				Cross.gameObject.SetActive(value: false);
			}
			else
			{
				Frame.color = DeselectedColor;
				Tick.gameObject.SetActive(value: false);
				Cross.gameObject.SetActive(value: true);
			}
		}

		public void UpdateDiscovered(ProductDefinition def)
		{
			if (def == null)
			{
				Console.LogWarning(def?.ToString() + " productDefinition is null");
			}
			if (def.ID == Definition.ID)
			{
				if (ProductManager.DiscoveredProducts.Contains(Definition))
				{
					Icon.color = Color.white;
				}
				else
				{
					Icon.color = Color.black;
				}
				UpdateListed();
			}
		}
	}
}
