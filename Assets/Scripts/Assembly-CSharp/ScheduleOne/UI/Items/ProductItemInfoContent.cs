using System.Collections.Generic;
using ScheduleOne.ItemFramework;
using ScheduleOne.Product;
using TMPro;

namespace ScheduleOne.UI.Items
{
	public class ProductItemInfoContent : QualityItemInfoContent
	{
		public List<TextMeshProUGUI> PropertyLabels = new List<TextMeshProUGUI>();

		public override void Initialize(ItemInstance instance)
		{
			base.Initialize(instance);
			if (!(instance is ProductItemInstance productItemInstance))
			{
				Console.LogError("ProductItemInfoContent can only be used with ProductItemInstance!");
			}
			else
			{
				Initialize(productItemInstance.Definition);
			}
		}

		public override void Initialize(ItemDefinition definition)
		{
			base.Initialize(definition);
			ProductDefinition productDefinition = definition as ProductDefinition;
			PropertyUtility.DrugTypeData drugTypeData = PropertyUtility.GetDrugTypeData(productDefinition.DrugTypes[0].DrugType);
			TextMeshProUGUI qualityLabel = QualityLabel;
			qualityLabel.text = qualityLabel.text + " " + drugTypeData.Name;
			for (int i = 0; i < PropertyLabels.Count; i++)
			{
				if (productDefinition.Properties.Count > i)
				{
					PropertyLabels[i].text = "• " + productDefinition.Properties[i].Name;
					PropertyLabels[i].color = productDefinition.Properties[i].LabelColor;
					PropertyLabels[i].enabled = true;
				}
				else
				{
					PropertyLabels[i].enabled = false;
				}
			}
		}
	}
}
