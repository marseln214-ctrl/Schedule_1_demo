using System;
using System.Linq;
using ScheduleOne.DevUtilities;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.Product;
using UnityEngine;

namespace ScheduleOne.Persistence.Loaders
{
	public class WeedProductLoader : Loader
	{
		public override void Load(string mainPath)
		{
			if (TryLoadFile(mainPath, out var contents, autoAddExtension: false))
			{
				WeedProductData weedProductData = null;
				try
				{
					weedProductData = JsonUtility.FromJson<WeedProductData>(contents);
				}
				catch (Exception ex)
				{
					Debug.LogError("Error loading product data: " + ex.Message);
				}
				if (weedProductData != null)
				{
					NetworkSingleton<ProductManager>.Instance.CreateWeed_Server(weedProductData.Name, weedProductData.ID, weedProductData.DrugType, weedProductData.Properties.ToList(), weedProductData.AppearanceSettings);
				}
			}
		}
	}
}
