using UnityEngine;
using UnityEngine.UI;

namespace ScheduleOne.UI.Shop
{
	public class CategoryButton : MonoBehaviour
	{
		public EShopCategory Category;

		private Button button;

		private ShopInterface shop;

		public bool isSelected { get; protected set; }

		private void Awake()
		{
			button = GetComponent<Button>();
			shop = GetComponentInParent<ShopInterface>();
			button.onClick.AddListener(Clicked);
			Deselect();
		}

		private void OnValidate()
		{
			base.gameObject.name = Category.ToString();
		}

		private void Clicked()
		{
			if (isSelected)
			{
				Deselect();
			}
			else
			{
				Select();
			}
		}

		public void Deselect()
		{
			isSelected = false;
			RefreshUI();
		}

		public void Select()
		{
			isSelected = true;
			RefreshUI();
			shop.CategorySelected(Category);
		}

		private void RefreshUI()
		{
			button.interactable = !isSelected;
		}
	}
}
