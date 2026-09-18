using System;
using ScheduleOne.ItemFramework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ScheduleOne.UI.Items
{
	public class ItemUI : MonoBehaviour
	{
		protected ItemInstance itemInstance;

		[Header("References")]
		public RectTransform Rect;

		public Image IconImg;

		public TextMeshProUGUI QuantityLabel;

		protected int DisplayedQuantity;

		protected bool Destroyed;

		public virtual void Setup(ItemInstance item)
		{
			if (item == null)
			{
				Console.LogError("ItemUI.Setup called and passed null item");
			}
			itemInstance = item;
			ItemInstance obj = itemInstance;
			obj.onDataChanged = (Action)Delegate.Remove(obj.onDataChanged, new Action(UpdateUI));
			ItemInstance obj2 = itemInstance;
			obj2.onDataChanged = (Action)Delegate.Combine(obj2.onDataChanged, new Action(UpdateUI));
			UpdateUI();
		}

		public virtual void Destroy()
		{
			Destroyed = true;
			ItemInstance obj = itemInstance;
			obj.onDataChanged = (Action)Delegate.Remove(obj.onDataChanged, new Action(UpdateUI));
			itemInstance = null;
			UnityEngine.Object.Destroy(Rect.gameObject);
		}

		public virtual RectTransform DuplicateIcon(Transform parent, int overriddenQuantity = -1)
		{
			int displayedQuantity = DisplayedQuantity;
			if (overriddenQuantity != -1)
			{
				SetDisplayedQuantity(overriddenQuantity);
			}
			RectTransform component = UnityEngine.Object.Instantiate(IconImg.gameObject, parent).GetComponent<RectTransform>();
			component.localScale = Vector3.one;
			SetDisplayedQuantity(displayedQuantity);
			return component;
		}

		public virtual void SetVisible(bool vis)
		{
			Rect.gameObject.SetActive(vis);
		}

		public virtual void UpdateUI()
		{
			if (!Destroyed)
			{
				IconImg.sprite = itemInstance.Icon;
				SetDisplayedQuantity(itemInstance.Quantity);
			}
		}

		public virtual void SetDisplayedQuantity(int quantity)
		{
			DisplayedQuantity = quantity;
			if (quantity > 1)
			{
				QuantityLabel.text = quantity + "x";
			}
			else
			{
				QuantityLabel.text = string.Empty;
			}
		}
	}
}
