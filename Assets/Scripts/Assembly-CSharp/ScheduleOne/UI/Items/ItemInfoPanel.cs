using ScheduleOne.ItemFramework;
using UnityEngine;

namespace ScheduleOne.UI.Items
{
	public class ItemInfoPanel : MonoBehaviour
	{
		public const float VERTICAL_THRESHOLD = 200f;

		[Header("References")]
		public RectTransform Container;

		public RectTransform ContentContainer;

		public GameObject TopArrow;

		public GameObject BottomArrow;

		public Canvas Canvas;

		[Header("Settings")]
		public Vector2 Offset = new Vector2(0f, 125f);

		[Header("Prefabs")]
		public ItemInfoContent DefaultContentPrefab;

		private ItemInfoContent content;

		public bool IsOpen { get; protected set; }

		public ItemInstance CurrentItem { get; protected set; }

		private void Awake()
		{
			Close();
		}

		public void Open(ItemInstance item, RectTransform rect)
		{
			if (IsOpen)
			{
				Close();
			}
			if (item == null)
			{
				Console.LogWarning("Item is null!");
				return;
			}
			CurrentItem = item;
			if (item.Definition.CustomInfoContent != null)
			{
				content = Object.Instantiate(item.Definition.CustomInfoContent, ContentContainer);
				content.Initialize(item);
			}
			else
			{
				content = Object.Instantiate(DefaultContentPrefab, ContentContainer);
				content.Initialize(item);
			}
			Container.sizeDelta = new Vector2(Container.sizeDelta.x, content.Height);
			float num = (rect.sizeDelta.y + Container.sizeDelta.y) / 2f + Offset.y;
			num *= Canvas.scaleFactor;
			if (rect.position.y > 200f)
			{
				Container.position = rect.position - new Vector3(0f, num, 0f);
				TopArrow.SetActive(value: true);
				BottomArrow.SetActive(value: false);
			}
			else
			{
				Container.position = rect.position + new Vector3(0f, num, 0f);
				TopArrow.SetActive(value: false);
				BottomArrow.SetActive(value: true);
			}
			IsOpen = true;
			Container.gameObject.SetActive(value: true);
		}

		public void Open(ItemDefinition def, RectTransform rect)
		{
			if (IsOpen)
			{
				Close();
			}
			if (def == null)
			{
				Console.LogWarning("Item is null!");
				return;
			}
			CurrentItem = null;
			content = Object.Instantiate(DefaultContentPrefab, ContentContainer);
			content.Initialize(def);
			float num = (rect.sizeDelta.y + Container.sizeDelta.y) / 2f + Offset.y;
			num *= Canvas.scaleFactor;
			if (rect.position.y > 200f)
			{
				Container.position = rect.position - new Vector3(0f, num, 0f);
				TopArrow.SetActive(value: true);
				BottomArrow.SetActive(value: false);
			}
			else
			{
				Container.position = rect.position + new Vector3(0f, num, 0f);
				TopArrow.SetActive(value: false);
				BottomArrow.SetActive(value: true);
			}
			IsOpen = true;
			Container.gameObject.SetActive(value: true);
		}

		public void Close()
		{
			if (content != null)
			{
				Object.Destroy(content.gameObject);
			}
			IsOpen = false;
			CurrentItem = null;
			Container.gameObject.SetActive(value: false);
		}
	}
}
