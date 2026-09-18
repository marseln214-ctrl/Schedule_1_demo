using System;
using System.Collections.Generic;
using System.Linq;
using ScheduleOne.DevUtilities;
using ScheduleOne.Product.Packaging;
using UnityEngine;

namespace ScheduleOne.Product
{
	public class ProductIconManager : Singleton<ProductIconManager>
	{
		[Serializable]
		public class ProductIcon
		{
			public string ProductID;

			public string PackagingID;

			public Sprite Icon;
		}

		[SerializeField]
		private List<ProductIcon> icons = new List<ProductIcon>();

		[Header("Product and packaging")]
		public IconGenerator IconGenerator;

		public string IconContainerPath = "ProductIcons";

		public ProductDefinition[] Products;

		public PackagingDefinition[] Packaging;

		public Sprite GetIcon(string productID, string packagingID, bool ignoreError = false)
		{
			ProductIcon productIcon = icons.Find((ProductIcon x) => x.ProductID == productID && x.PackagingID == packagingID);
			if (productIcon == null)
			{
				if (!ignoreError)
				{
					Console.LogError("Failed to find icon for packaging (" + packagingID + ") containing product (" + productID + ")");
				}
				return null;
			}
			return productIcon.Icon;
		}

		public Sprite GenerateIcons(string productID)
		{
			if (Registry.GetItem(productID) == null)
			{
				Console.LogError("Failed to find product with ID: " + productID);
				return null;
			}
			Console.Log("Generating icons for " + productID);
			if (icons.Any((ProductIcon x) => x.ProductID == productID) && Registry.GetItem(productID) != null)
			{
				return Registry.GetItem(productID).Icon;
			}
			for (int i = 0; i < Packaging.Length; i++)
			{
				Texture2D texture2D = GenerateProductTexture(productID, Packaging[i].ID);
				if (texture2D == null)
				{
					Console.LogError("Failed to generate icon for packaging (" + Packaging[i].ID + ") containing product (" + productID + ")");
				}
				else
				{
					ProductIcon productIcon = new ProductIcon();
					productIcon.ProductID = productID;
					productIcon.PackagingID = Packaging[i].ID;
					texture2D.Apply();
					productIcon.Icon = Sprite.Create(texture2D, new Rect(0f, 0f, texture2D.width, texture2D.height), new Vector2(0.5f, 0.5f));
					icons.Add(productIcon);
				}
			}
			Texture2D texture2D2 = GenerateProductTexture(productID, "none");
			texture2D2.Apply();
			return Sprite.Create(texture2D2, new Rect(0f, 0f, texture2D2.width, texture2D2.height), new Vector2(0.5f, 0.5f));
		}

		private Texture2D GenerateProductTexture(string productID, string packagingID)
		{
			return IconGenerator.GeneratePackagingIcon(packagingID, productID);
		}
	}
}
