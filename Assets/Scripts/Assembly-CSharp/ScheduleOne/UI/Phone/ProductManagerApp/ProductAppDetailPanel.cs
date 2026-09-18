using ScheduleOne.DevUtilities;
using ScheduleOne.Money;
using ScheduleOne.PlayerTasks;
using ScheduleOne.Product;
using ScheduleOne.UI.Tooltips;
using UnityEngine;
using UnityEngine.UI;

namespace ScheduleOne.UI.Phone.ProductManagerApp
{
	public class ProductAppDetailPanel : MonoBehaviour
	{
		public Color AddictionColor_Min;

		public Color AddictionColor_Max;

		[Header("References")]
		public GameObject NothingSelected;

		public GameObject Container;

		public Text NameLabel;

		public InputField ValueLabel;

		public Text SuggestedPriceLabel;

		public Toggle ListedForSale;

		public Text DescLabel;

		public Text[] PropertyLabels;

		public RectTransform Listed;

		public RectTransform Delisted;

		public RectTransform NotDiscovered;

		public RectTransform RecipesLabel;

		public RectTransform[] RecipeEntries;

		public VerticalLayoutGroup LayoutGroup;

		public Scrollbar AddictionSlider;

		public Text AddictionLabel;

		public ScrollRect ScrollRect;

		public ProductDefinition ActiveProduct { get; protected set; }

		public void Awake()
		{
			ListedForSale.onValueChanged.AddListener(delegate
			{
				ListingToggled();
			});
			ValueLabel.onEndEdit.AddListener(delegate(string value)
			{
				PriceSubmitted(value);
			});
		}

		public void SetActiveProduct(ProductDefinition productDefinition)
		{
			ActiveProduct = productDefinition;
			bool flag = ProductManager.DiscoveredProducts.Contains(productDefinition);
			if (ActiveProduct != null)
			{
				NameLabel.text = productDefinition.Name;
				SuggestedPriceLabel.text = "Suggested: " + MoneyManager.FormatAmount(productDefinition.MarketValue);
				UpdatePrice();
				if (flag)
				{
					DescLabel.text = productDefinition.Description;
				}
				else
				{
					DescLabel.text = "???";
				}
				for (int i = 0; i < PropertyLabels.Length; i++)
				{
					if (productDefinition.Properties.Count > i)
					{
						PropertyLabels[i].text = "•  " + productDefinition.Properties[i].Name;
						PropertyLabels[i].color = productDefinition.Properties[i].LabelColor;
						PropertyLabels[i].gameObject.SetActive(value: true);
					}
					else
					{
						PropertyLabels[i].gameObject.SetActive(value: false);
					}
				}
				for (int j = 0; j < RecipeEntries.Length; j++)
				{
					if (productDefinition.Recipes.Count > j)
					{
						RecipeEntries[j].gameObject.SetActive(value: true);
						if (productDefinition.Recipes[j].Ingredients[0].Item is ProductDefinition)
						{
							RecipeEntries[j].Find("Product").GetComponent<Image>().sprite = productDefinition.Recipes[j].Ingredients[0].Item.Icon;
							RecipeEntries[j].Find("Product").GetComponent<Tooltip>().text = productDefinition.Recipes[j].Ingredients[0].Item.Name;
							RecipeEntries[j].Find("Mixer").GetComponent<Image>().sprite = productDefinition.Recipes[j].Ingredients[1].Item.Icon;
							RecipeEntries[j].Find("Mixer").GetComponent<Tooltip>().text = productDefinition.Recipes[j].Ingredients[1].Item.Name;
						}
						else
						{
							RecipeEntries[j].Find("Product").GetComponent<Image>().sprite = productDefinition.Recipes[j].Ingredients[1].Item.Icon;
							RecipeEntries[j].Find("Product").GetComponent<Tooltip>().text = productDefinition.Recipes[j].Ingredients[1].Item.Name;
							RecipeEntries[j].Find("Mixer").GetComponent<Image>().sprite = productDefinition.Recipes[j].Ingredients[0].Item.Icon;
							RecipeEntries[j].Find("Mixer").GetComponent<Tooltip>().text = productDefinition.Recipes[j].Ingredients[0].Item.Name;
						}
						RecipeEntries[j].Find("Output").GetComponent<Image>().sprite = productDefinition.Icon;
						RecipeEntries[j].Find("Output").GetComponent<Tooltip>().text = productDefinition.Name;
					}
					else
					{
						RecipeEntries[j].gameObject.SetActive(value: false);
					}
				}
				RecipesLabel.gameObject.SetActive(productDefinition.Recipes.Count > 0);
				NothingSelected.gameObject.SetActive(value: false);
				Container.gameObject.SetActive(value: true);
				AddictionSlider.value = productDefinition.GetAddictiveness();
				AddictionLabel.text = Mathf.FloorToInt(productDefinition.GetAddictiveness() * 100f) + "%";
				AddictionLabel.color = Color.Lerp(AddictionColor_Min, AddictionColor_Max, productDefinition.GetAddictiveness());
				ContentSizeFitter[] componentsInChildren = GetComponentsInChildren<ContentSizeFitter>();
				for (int k = 0; k < componentsInChildren.Length; k++)
				{
					componentsInChildren[k].enabled = false;
					componentsInChildren[k].enabled = true;
				}
				LayoutGroup.enabled = false;
				LayoutGroup.enabled = true;
				LayoutRebuilder.ForceRebuildLayoutImmediate(LayoutGroup.GetComponent<RectTransform>());
				ScrollRect.enabled = false;
				ScrollRect.enabled = true;
				ScrollRect.verticalNormalizedPosition = 1f;
				LayoutRebuilder.ForceRebuildLayoutImmediate(ScrollRect.GetComponent<RectTransform>());
			}
			else
			{
				NothingSelected.gameObject.SetActive(value: true);
				Container.gameObject.SetActive(value: false);
			}
			UpdateListed();
		}

		private void Update()
		{
			if (PlayerSingleton<ProductManagerApp>.Instance.isOpen)
			{
				UpdateListed();
			}
		}

		private void UpdateListed()
		{
			ListedForSale.SetIsOnWithoutNotify(ProductManager.ListedProducts.Contains(ActiveProduct));
		}

		private void UpdatePrice()
		{
			ValueLabel.SetTextWithoutNotify(NetworkSingleton<ProductManager>.Instance.GetPrice(ActiveProduct).ToString());
		}

		private void ListingToggled()
		{
			if (NetworkSingleton<ProductManager>.InstanceExists && !(ActiveProduct == null))
			{
				if (ProductManager.ListedProducts.Contains(ActiveProduct))
				{
					NetworkSingleton<ProductManager>.Instance.SetProductListed(ActiveProduct.ID, listed: false);
				}
				else
				{
					NetworkSingleton<ProductManager>.Instance.SetProductListed(ActiveProduct.ID, listed: true);
				}
				Singleton<TaskManager>.Instance.PlayTaskCompleteSound();
				UpdateListed();
			}
		}

		private void PriceSubmitted(string value)
		{
			if (NetworkSingleton<ProductManager>.InstanceExists && PlayerSingleton<ProductManagerApp>.Instance.isOpen && PlayerSingleton<Phone>.Instance.IsOpen && !(ActiveProduct == null))
			{
				if (float.TryParse(value, out var result))
				{
					NetworkSingleton<ProductManager>.Instance.SendPrice(ActiveProduct.ID, result);
					Singleton<TaskManager>.Instance.PlayTaskCompleteSound();
				}
				UpdatePrice();
			}
		}
	}
}
