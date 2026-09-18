using System;
using System.Linq;
using ScheduleOne.DevUtilities;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.Product;
using UnityEngine;

namespace ScheduleOne.Persistence.Loaders
{
	public class CocaineProductLoader : Loader
	{
		public override void Load(string mainPath)
		{
			if (TryLoadFile(mainPath, out var contents, autoAddExtension: false))
			{
				CocaineProductData cocaineProductData = null;
				try
				{
					cocaineProductData = JsonUtility.FromJson<CocaineProductData>(contents);
				}
				catch (Exception ex)
				{
					Debug.LogError("Error loading product data: " + ex.Message);
				}
				if (cocaineProductData != null)
				{
					NetworkSingleton<ProductManager>.Instance.CreateCocaine_Server(cocaineProductData.Name, cocaineProductData.ID, cocaineProductData.DrugType, cocaineProductData.Properties.ToList(), cocaineProductData.AppearanceSettings);
				}
			}
		}
	}
}
