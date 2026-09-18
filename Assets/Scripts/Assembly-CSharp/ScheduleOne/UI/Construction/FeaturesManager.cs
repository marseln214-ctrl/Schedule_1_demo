using System.Collections.Generic;
using ScheduleOne.ConstructableScripts;
using ScheduleOne.Construction.Features;
using ScheduleOne.DevUtilities;
using ScheduleOne.UI.Construction.Features;
using TMPro;
using UnityEngine;

namespace ScheduleOne.UI.Construction
{
	public class FeaturesManager : Singleton<FeaturesManager>
	{
		public Constructable activeConstructable;

		public Feature selectedFeature;

		[Header("References")]
		[SerializeField]
		protected RectTransform featureIconsContainer;

		[SerializeField]
		protected RectTransform featureMenuRect;

		[SerializeField]
		protected TextMeshProUGUI featureMenuTitleLabel;

		[SerializeField]
		protected RectTransform featureInterfaceContainer;

		[Header("Prefabs")]
		[SerializeField]
		protected GameObject featureIconPrefab;

		private FI_Base currentFeatureInterface;

		private bool roofSetInvisible;

		protected List<FeatureIcon> featureIcons = new List<FeatureIcon>();

		public bool isActive => activeConstructable != null;

		protected override void Awake()
		{
			base.Awake();
			CloseFeatureMenu();
		}

		private void LateUpdate()
		{
			if (isActive && featureIcons.Count > 0)
			{
				UpdateIconTransforms();
			}
		}

		public void OpenFeatureMenu(Feature feature)
		{
			if (selectedFeature != null)
			{
				CloseFeatureMenu();
			}
			selectedFeature = feature;
			featureMenuRect.gameObject.SetActive(value: true);
			featureMenuTitleLabel.text = Singleton<ConstructionMenu>.Instance.SelectedConstructable.ConstructableName + " > " + selectedFeature.featureName;
			if (feature.disableRoofDisibility && Singleton<ConstructionMenu>.Instance.SelectedConstructable is Constructable_GridBased)
			{
				(Singleton<ConstructionMenu>.Instance.SelectedConstructable as Constructable_GridBased).SetRoofVisible(vis: false);
				roofSetInvisible = true;
			}
			currentFeatureInterface = feature.CreateInterface(featureInterfaceContainer);
		}

		public void CloseFeatureMenu()
		{
			if (currentFeatureInterface != null)
			{
				currentFeatureInterface.Close();
			}
			if (roofSetInvisible)
			{
				if (Singleton<ConstructionMenu>.Instance.SelectedConstructable is Constructable_GridBased)
				{
					(Singleton<ConstructionMenu>.Instance.SelectedConstructable as Constructable_GridBased).SetRoofVisible(vis: true);
				}
				roofSetInvisible = false;
			}
			selectedFeature = null;
			featureMenuRect.gameObject.SetActive(value: false);
		}

		public void DeselectFeature()
		{
			if (selectedFeature == null)
			{
				return;
			}
			foreach (FeatureIcon featureIcon in featureIcons)
			{
				if (featureIcon.isSelected)
				{
					featureIcon.SetIsSelected(s: false);
				}
			}
			CloseFeatureMenu();
			selectedFeature = null;
		}

		public void Activate(Constructable constructable)
		{
			Deactivate();
			activeConstructable = constructable;
			CreateIcons();
		}

		public void Deactivate()
		{
			ClearIcons();
			if (selectedFeature != null)
			{
				CloseFeatureMenu();
			}
			activeConstructable = null;
		}

		private void ClearIcons()
		{
			for (int i = 0; i < featureIcons.Count; i++)
			{
				Object.Destroy(featureIcons[i].gameObject);
			}
			featureIcons.Clear();
		}

		private void CreateIcons()
		{
			foreach (Feature feature in activeConstructable.features)
			{
				FeatureIcon component = Object.Instantiate(featureIconPrefab, featureIconsContainer).GetComponent<FeatureIcon>();
				component.AssignFeature(feature);
				featureIcons.Add(component);
			}
			UpdateIconTransforms();
		}

		private void UpdateIconTransforms()
		{
			foreach (FeatureIcon featureIcon in featureIcons)
			{
				featureIcon.UpdateTransform();
			}
		}
	}
}
