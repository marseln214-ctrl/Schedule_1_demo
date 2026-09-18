using System;
using System.Collections;
using System.Collections.Generic;
using ScheduleOne.DevUtilities;
using ScheduleOne.Product;
using UnityEngine;
using UnityEngine.UI;

namespace ScheduleOne.UI.Phone.ProductManagerApp
{
	public class ProductManagerApp : App<ProductManagerApp>
	{
		[Serializable]
		public class ProductTypeContainer
		{
			public EDrugType DrugType;

			public RectTransform Container;
		}

		[Header("References")]
		public List<ProductTypeContainer> ProductTypeContainers;

		public ProductAppDetailPanel DetailPanel;

		public RectTransform SelectionIndicator;

		public GameObject EntryPrefab;

		private List<ProductEntry> entries = new List<ProductEntry>();

		private ProductEntry entry;

		protected override void Awake()
		{
			base.Awake();
			DetailPanel.SetActiveProduct(null);
		}

		protected override void Start()
		{
			base.Start();
			ProductManager productManager = NetworkSingleton<ProductManager>.Instance;
			productManager.onProductDiscovered = (Action<ProductDefinition>)Delegate.Combine(productManager.onProductDiscovered, new Action<ProductDefinition>(CreateEntry));
			foreach (ProductDefinition discoveredProduct in ProductManager.DiscoveredProducts)
			{
				CreateEntry(discoveredProduct);
			}
		}

		private void LateUpdate()
		{
			if (base.isOpen && entry != null)
			{
				SelectionIndicator.position = entry.transform.position;
			}
		}

		public virtual void CreateEntry(ProductDefinition definition)
		{
			ProductEntry component = UnityEngine.Object.Instantiate(EntryPrefab, ProductTypeContainers.Find((ProductTypeContainer x) => x.DrugType == definition.DrugTypes[0].DrugType).Container).GetComponent<ProductEntry>();
			component.Initialize(definition);
			entries.Add(component);
			LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
		}

		public void SelectProduct(ProductEntry entry)
		{
			this.entry = entry;
			DetailPanel.SetActiveProduct(entry.Definition);
			SelectionIndicator.position = entry.transform.position;
			SelectionIndicator.gameObject.SetActive(value: true);
		}

		public override void SetOpen(bool open)
		{
			base.SetOpen(open);
			VerticalLayoutGroup[] layoutGroups;
			if (open)
			{
				for (int i = 0; i < entries.Count; i++)
				{
					entries[i].UpdateDiscovered(entries[i].Definition);
					entries[i].UpdateListed();
				}
				LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
				base.gameObject.SetActive(value: false);
				base.gameObject.SetActive(value: true);
				layoutGroups = GetComponentsInChildren<VerticalLayoutGroup>();
				for (int j = 0; j < layoutGroups.Length; j++)
				{
					layoutGroups[j].enabled = false;
					layoutGroups[j].enabled = true;
				}
				if (entry != null)
				{
					DetailPanel.SetActiveProduct(entry.Definition);
				}
				StartCoroutine(Delay());
			}
			IEnumerator Delay()
			{
				LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
				for (int k = 0; k < layoutGroups.Length; k++)
				{
					layoutGroups[k].enabled = false;
					layoutGroups[k].enabled = true;
				}
				yield return new WaitForEndOfFrame();
				LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
				for (int l = 0; l < layoutGroups.Length; l++)
				{
					layoutGroups[l].enabled = false;
					layoutGroups[l].enabled = true;
				}
				yield return new WaitForEndOfFrame();
				LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
				for (int m = 0; m < layoutGroups.Length; m++)
				{
					layoutGroups[m].enabled = false;
					layoutGroups[m].enabled = true;
				}
			}
		}
	}
}
