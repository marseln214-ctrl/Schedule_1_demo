using FluffyUnderware.DevTools.Extensions;
using ScheduleOne.DevUtilities;
using ScheduleOne.ItemFramework;
using ScheduleOne.ObjectScripts;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Product;
using UnityEngine;

namespace ScheduleOne.Growing
{
	public class PlantHarvestable : MonoBehaviour
	{
		public StorableItemDefinition Product;

		public int ProductQuantity = 1;

		public virtual void Harvest(bool giveProduct = true)
		{
			Plant componentInParent = GetComponentInParent<Plant>();
			if (giveProduct)
			{
				int b = Mathf.RoundToInt(componentInParent.Pot.YieldMultiplier * (float)ProductQuantity);
				b = Mathf.Max(1, b);
				ItemInstance harvestedProduct = componentInParent.GetHarvestedProduct(b);
				if (Product is ProductDefinition productDefinition && !ProductManager.DiscoveredProducts.Contains(productDefinition))
				{
					NetworkSingleton<ProductManager>.Instance.DiscoverProduct(productDefinition.ID);
				}
				PlayerSingleton<PlayerInventory>.Instance.AddItemToInventory(harvestedProduct);
			}
			GetComponentInParent<Pot>().SendHarvestableActive(componentInParent.FinalGrowthStage.GrowthSites.IndexOf(base.transform.parent), active: false);
			GameObject obj = Object.Instantiate(base.gameObject, GameObject.Find("_Temp").transform);
			obj.transform.position = base.transform.position;
			obj.transform.rotation = base.transform.rotation;
			obj.transform.localScale = base.transform.lossyScale;
			Object.Destroy(obj.GetComponent<PlantHarvestable>());
			Object.Destroy(obj.GetComponentInChildren<Collider>());
			obj.AddComponent(typeof(Rigidbody));
			Rigidbody component = obj.GetComponent<Rigidbody>();
			component.AddForce(Vector3.up * 1.5f, ForceMode.VelocityChange);
			component.AddTorque(new Vector3(Random.Range(-1f, 1f), Random.Range(1f, 1f), Random.Range(-1f, 1f)) * 4f, ForceMode.VelocityChange);
			Object.Destroy(obj, 2f);
		}
	}
}
