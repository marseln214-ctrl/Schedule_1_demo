using System;
using System.Collections.Generic;
using ScheduleOne.Product;
using UnityEngine;

namespace ScheduleOne.Economy
{
	[Serializable]
	public class CustomerAffinityData
	{
		[Header("Product Affinities - How much the customer likes each product type. -1 = hates, 0 = neutral, 1 = loves.")]
		public List<ProductTypeAffinity> ProductAffinities = new List<ProductTypeAffinity>();

		public void CopyTo(CustomerAffinityData data)
		{
			foreach (ProductTypeAffinity affinity in ProductAffinities)
			{
				if (data.ProductAffinities.Exists((ProductTypeAffinity x) => x.DrugType == affinity.DrugType))
				{
					data.ProductAffinities.Find((ProductTypeAffinity x) => x.DrugType == affinity.DrugType).Affinity = affinity.Affinity;
				}
				else
				{
					data.ProductAffinities.Add(new ProductTypeAffinity
					{
						DrugType = affinity.DrugType,
						Affinity = affinity.Affinity
					});
				}
			}
		}

		public float GetAffinity(EDrugType type)
		{
			return ProductAffinities.Find((ProductTypeAffinity x) => x.DrugType == type).Affinity;
		}
	}
}
