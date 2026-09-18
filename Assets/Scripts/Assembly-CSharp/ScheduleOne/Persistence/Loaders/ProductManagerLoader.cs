using System;
using System.IO;
using ScheduleOne.DevUtilities;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.Product;
using ScheduleOne.UI;
using UnityEngine;

namespace ScheduleOne.Persistence.Loaders
{
	public class ProductManagerLoader : Loader
	{
		public override void Load(string mainPath)
		{
			string path = Path.Combine(mainPath, "CreatedProducts");
			if (Directory.Exists(path))
			{
				WeedProductLoader weedProductLoader = new WeedProductLoader();
				CocaineProductLoader cocaineProductLoader = new CocaineProductLoader();
				string[] files = Directory.GetFiles(path);
				for (int i = 0; i < files.Length; i++)
				{
					Debug.Log("Loading product: " + files[i]);
					if (!TryLoadFile(files[i], out var contents, autoAddExtension: false))
					{
						continue;
					}
					ProductData productData = null;
					try
					{
						productData = JsonUtility.FromJson<ProductData>(contents);
					}
					catch (Exception ex)
					{
						Debug.LogError("Error loading product data: " + ex.Message);
					}
					if (productData == null)
					{
						continue;
					}
					bool flag = false;
					if (string.IsNullOrEmpty(productData.Name))
					{
						Console.LogWarning("Product name is empty; generating random name");
						if (Singleton<NewMixScreen>.InstanceExists)
						{
							productData.Name = Singleton<NewMixScreen>.Instance.GenerateUniqueName();
						}
						else
						{
							productData.Name = "Product " + UnityEngine.Random.Range(0, 1000);
						}
						flag = true;
					}
					if (string.IsNullOrEmpty(productData.ID))
					{
						Console.LogWarning("Product ID is empty; generating from name");
						productData.ID = ProductManager.MakeIDFileSafe(productData.Name);
						flag = true;
					}
					if (flag)
					{
						try
						{
							File.WriteAllText(files[i], productData.GetJson());
						}
						catch (Exception ex2)
						{
							Console.LogError("Error saving modified product data: " + ex2.Message);
						}
					}
					switch (productData.DrugType)
					{
					case EDrugType.Marijuana:
						weedProductLoader.Load(files[i]);
						break;
					case EDrugType.Cocaine:
						cocaineProductLoader.Load(files[i]);
						break;
					default:
						Console.LogError("Unknown drug type: " + productData.DrugType);
						break;
					}
				}
			}
			if (!TryLoadFile(Path.Combine(mainPath, "Products"), out var contents2))
			{
				return;
			}
			ProductManagerData productManagerData = JsonUtility.FromJson<ProductManagerData>(contents2);
			if (productManagerData == null)
			{
				return;
			}
			for (int j = 0; j < productManagerData.DiscoveredProducts.Length; j++)
			{
				NetworkSingleton<ProductManager>.Instance.SetProductDiscovered(null, productManagerData.DiscoveredProducts[j], autoList: false);
			}
			for (int k = 0; k < productManagerData.ListedProducts.Length; k++)
			{
				NetworkSingleton<ProductManager>.Instance.SetProductListed(null, productManagerData.ListedProducts[k], listed: true);
			}
			if (productManagerData.ActiveMixOperation != null && productManagerData.ActiveMixOperation.ProductID != string.Empty)
			{
				NetworkSingleton<ProductManager>.Instance.SendMixOperation(productManagerData.ActiveMixOperation, productManagerData.IsMixComplete);
			}
			for (int l = 0; l < productManagerData.MixRecipes.Length; l++)
			{
				MixRecipeData mixRecipeData = productManagerData.MixRecipes[l];
				NetworkSingleton<ProductManager>.Instance.CreateMixRecipe(null, mixRecipeData.Product, mixRecipeData.Mixer, mixRecipeData.Output);
			}
			if (productManagerData.ProductPrices == null)
			{
				return;
			}
			for (int m = 0; m < productManagerData.ProductPrices.Length; m++)
			{
				StringIntPair stringIntPair = productManagerData.ProductPrices[m];
				ProductDefinition item = Registry.GetItem<ProductDefinition>(stringIntPair.String);
				if (item != null)
				{
					NetworkSingleton<ProductManager>.Instance.SetPrice(null, item.ID, stringIntPair.Int);
				}
			}
		}
	}
}
