using System;
using System.Collections.Generic;
using System.Linq;
using ScheduleOne.DevUtilities;
using ScheduleOne.Interaction;
using ScheduleOne.Packaging;
using ScheduleOne.Properties;
using TMPro;
using UnityEngine;

namespace ScheduleOne.Product
{
	public class NewMixDiscoveryBox : MonoBehaviour
	{
		[Serializable]
		public class DrugTypeVisuals
		{
			public EDrugType DrugType;

			public FilledPackagingVisuals Visuals;
		}

		private bool isOpen;

		[Header("References")]
		public Transform CameraPosition;

		public TextMeshPro PropertiesText;

		public DrugTypeVisuals[] Visuals;

		public Animation Animation;

		public InteractableObject IntObj;

		public Transform Lid;

		private Pose closedLidPose;

		private NewMixOperation currentMix;

		public void Start()
		{
			closedLidPose = new Pose(Lid.localPosition, Lid.localRotation);
			CloseCase();
			IntObj.onInteractStart.AddListener(Interacted);
			IntObj.gameObject.SetActive(value: false);
			_ = NetworkSingleton<ProductManager>.Instance.IsMixComplete;
		}

		public void ShowProduct(ProductDefinition baseDefinition, List<ScheduleOne.Properties.Property> properties)
		{
			PropertiesText.text = string.Empty;
			foreach (ScheduleOne.Properties.Property property in properties)
			{
				if (PropertiesText.text.Length > 0)
				{
					PropertiesText.text += "\n";
				}
				TextMeshPro propertiesText = PropertiesText;
				propertiesText.text = propertiesText.text + "<color=#" + ColorUtility.ToHtmlStringRGBA(property.LabelColor) + ">" + property.Name + "</color>";
			}
			for (int i = 0; i < Visuals.Length; i++)
			{
				Visuals[i].Visuals.gameObject.SetActive(value: false);
			}
			ProductDefinition productDefinition = UnityEngine.Object.Instantiate(baseDefinition);
			switch (baseDefinition.DrugType)
			{
			case EDrugType.Marijuana:
			{
				WeedDefinition obj3 = productDefinition as WeedDefinition;
				obj3.Initialize(_appearance: WeedDefinition.GetAppearanceSettings(properties), properties: properties, drugTypes: new List<EDrugType> { EDrugType.Marijuana });
				(obj3.GetDefaultInstance() as WeedInstance).SetupPackagingVisuals(Visuals.First((DrugTypeVisuals x) => x.DrugType == EDrugType.Marijuana).Visuals);
				Visuals.First((DrugTypeVisuals x) => x.DrugType == EDrugType.Marijuana).Visuals.gameObject.SetActive(value: true);
				break;
			}
			case EDrugType.Methamphetamine:
			{
				MethDefinition obj2 = productDefinition as MethDefinition;
				obj2.Initialize(_appearance: MethDefinition.GetAppearanceSettings(properties), properties: properties, drugTypes: new List<EDrugType> { EDrugType.Methamphetamine });
				(obj2.GetDefaultInstance() as MethInstance).SetupPackagingVisuals(Visuals.First((DrugTypeVisuals x) => x.DrugType == EDrugType.Methamphetamine).Visuals);
				Visuals.First((DrugTypeVisuals x) => x.DrugType == EDrugType.Methamphetamine).Visuals.gameObject.SetActive(value: true);
				break;
			}
			case EDrugType.Cocaine:
			{
				CocaineDefinition obj = productDefinition as CocaineDefinition;
				obj.Initialize(_appearance: CocaineDefinition.GetAppearanceSettings(properties), properties: properties, drugTypes: new List<EDrugType> { EDrugType.Cocaine });
				(obj.GetDefaultInstance() as CocaineInstance).SetupPackagingVisuals(Visuals.First((DrugTypeVisuals x) => x.DrugType == EDrugType.Cocaine).Visuals);
				Visuals.First((DrugTypeVisuals x) => x.DrugType == EDrugType.Cocaine).Visuals.gameObject.SetActive(value: true);
				break;
			}
			default:
				Console.LogError("Drug type not supported");
				break;
			}
			base.gameObject.SetActive(value: true);
		}

		private void CloseCase()
		{
			isOpen = false;
			Lid.localPosition = closedLidPose.position;
			Lid.localRotation = closedLidPose.rotation;
		}

		private void OpenCase()
		{
			isOpen = true;
			Animation.Play("New mix box open");
		}

		private void Interacted()
		{
			if (!isOpen)
			{
				OpenCase();
			}
			Registry.GetItem(currentMix.ProductID);
		}
	}
}
