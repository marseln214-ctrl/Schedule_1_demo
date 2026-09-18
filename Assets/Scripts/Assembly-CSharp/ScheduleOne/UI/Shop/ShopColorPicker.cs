using System;
using ScheduleOne.Clothing;
using ScheduleOne.ItemFramework;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ScheduleOne.UI.Shop
{
	public class ShopColorPicker : MonoBehaviour
	{
		public Image AssetIconImage;

		public TextMeshProUGUI ColorLabel;

		public RectTransform ColorButtonParent;

		public GameObject ColorButtonPrefab;

		public UnityEvent<EClothingColor> onColorPicked = new UnityEvent<EClothingColor>();

		public bool IsOpen => base.gameObject.activeSelf;

		public void Start()
		{
			foreach (EClothingColor color in Enum.GetValues(typeof(EClothingColor)))
			{
				GameObject obj = UnityEngine.Object.Instantiate(ColorButtonPrefab, ColorButtonParent);
				obj.transform.Find("Color").GetComponent<Image>().color = color.GetActualColor();
				obj.GetComponent<Button>().onClick.AddListener(delegate
				{
					ColorPicked(color);
				});
				EventTrigger eventTrigger = obj.AddComponent<EventTrigger>();
				EventTrigger.Entry entry = new EventTrigger.Entry();
				entry.eventID = EventTriggerType.PointerEnter;
				entry.callback.AddListener(delegate
				{
					ColorHovered(color);
				});
				eventTrigger.triggers.Add(entry);
			}
			base.gameObject.SetActive(value: false);
		}

		private void ColorPicked(EClothingColor color)
		{
			if (onColorPicked != null)
			{
				onColorPicked.Invoke(color);
			}
			Close();
		}

		public void Open(ItemDefinition item)
		{
			AssetIconImage.sprite = item.Icon;
			ColorHovered(EClothingColor.White);
			base.gameObject.SetActive(value: true);
		}

		public void Close()
		{
			base.gameObject.SetActive(value: false);
		}

		private void ColorHovered(EClothingColor color)
		{
			AssetIconImage.color = color.GetActualColor();
			ColorLabel.text = color.GetLabel();
		}
	}
}
