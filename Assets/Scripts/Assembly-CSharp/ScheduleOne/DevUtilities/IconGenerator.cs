using System;
using System.Collections.Generic;
using System.IO;
using EasyButtons;
using ScheduleOne.ItemFramework;
using ScheduleOne.Packaging;
using ScheduleOne.Product;
using UnityEngine;
using UnityEngine.Rendering;

namespace ScheduleOne.DevUtilities
{
	public class IconGenerator : Singleton<IconGenerator>
	{
		[Serializable]
		public class PackagingVisuals
		{
			public string PackagingID;

			public FilledPackagingVisuals Visuals;
		}

		public int IconSize = 512;

		public string OutputPath;

		public bool ModifyLighting = true;

		[Header("References")]
		public Registry Registry;

		public Camera CameraPosition;

		public Transform MainContainer;

		public Transform ItemContainer;

		public GameObject Canvas;

		public List<PackagingVisuals> Visuals;

		protected override void Awake()
		{
			base.Awake();
			Canvas.gameObject.SetActive(value: false);
			CameraPosition.gameObject.SetActive(value: false);
			CameraPosition.clearFlags = CameraClearFlags.Color;
			if (Registry == null)
			{
				Registry = Singleton<Registry>.Instance;
			}
		}

		[Button]
		public void GenerateIcon()
		{
			LayerUtility.SetLayerRecursively(ItemContainer.gameObject, LayerMask.NameToLayer("IconGeneration"));
			Transform transform = null;
			for (int i = 0; i < ItemContainer.transform.childCount; i++)
			{
				if (ItemContainer.transform.GetChild(i).gameObject.activeSelf)
				{
					transform = ItemContainer.transform.GetChild(i);
				}
			}
			string text = OutputPath + "/" + transform.name + "_Icon.png";
			Texture2D texture = GetTexture(transform.transform);
			Debug.Log("Writing to: " + text);
			byte[] bytes = texture.EncodeToPNG();
			File.WriteAllBytes(text, bytes);
		}

		public Texture2D GeneratePackagingIcon(string packagingID, string productID)
		{
			PackagingVisuals packagingVisuals = Visuals.Find((PackagingVisuals x) => packagingID == x.PackagingID);
			if (packagingVisuals == null)
			{
				Debug.LogError("Failed to find visuals for packaging (" + packagingID + ") containing product (" + productID + ")");
				return null;
			}
			ItemDefinition itemDefinition = Registry._GetItem(productID);
			if (Application.isPlaying)
			{
				itemDefinition = Singleton<Registry>.Instance._GetItem(productID);
			}
			ProductDefinition productDefinition = itemDefinition as ProductDefinition;
			if (productDefinition == null)
			{
				Debug.LogError("Failed to find product definition for product (" + productID + ")");
				return null;
			}
			(productDefinition.GetDefaultInstance() as ProductItemInstance).SetupPackagingVisuals(packagingVisuals.Visuals);
			packagingVisuals.Visuals.gameObject.SetActive(value: true);
			Texture2D texture = GetTexture(packagingVisuals.Visuals.transform.parent);
			packagingVisuals.Visuals.gameObject.SetActive(value: false);
			return texture;
		}

		public Texture2D GetTexture(Transform model)
		{
			MainContainer.gameObject.SetActive(value: true);
			bool activeSelf = ItemContainer.gameObject.activeSelf;
			ItemContainer.gameObject.SetActive(value: true);
			if (ModifyLighting)
			{
				RenderSettings.ambientMode = AmbientMode.Flat;
				RenderSettings.ambientLight = Color.white;
			}
			RuntimePreviewGenerator.CamPos = CameraPosition.transform.position;
			RuntimePreviewGenerator.CamRot = CameraPosition.transform.rotation;
			RuntimePreviewGenerator.Padding = 0f;
			RuntimePreviewGenerator.UseLocalBounds = true;
			RuntimePreviewGenerator.BackgroundColor = new Color32(0, 0, 0, 0);
			Texture2D result = RuntimePreviewGenerator.GenerateModelPreview(model, IconSize, IconSize);
			RenderSettings.ambientMode = AmbientMode.Trilight;
			MainContainer.gameObject.SetActive(value: false);
			ItemContainer.gameObject.SetActive(activeSelf);
			return result;
		}
	}
}
