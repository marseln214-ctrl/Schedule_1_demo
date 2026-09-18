using ScheduleOne.Construction.Features;
using ScheduleOne.DevUtilities;
using ScheduleOne.PlayerScripts;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ScheduleOne.UI.Construction
{
	public class FeatureIcon : MonoBehaviour
	{
		public static FeatureIcon selectedFeatureIcon;

		[Header("References")]
		public RectTransform rectTransform;

		public Image icon;

		public TextMeshProUGUI text;

		public Image background;

		private bool hovered;

		public Feature feature { get; protected set; }

		public bool isSelected { get; protected set; }

		public void AssignFeature(Feature _feature)
		{
			feature = _feature;
			icon.sprite = feature.featureIcon;
			text.text = feature.featureName;
			text.gameObject.SetActive(value: false);
		}

		public void UpdateTransform()
		{
			Vector3 position = feature.featureIconLocation.position;
			if (PlayerSingleton<PlayerCamera>.Instance.transform.InverseTransformPoint(position).z < 0f)
			{
				base.gameObject.SetActive(value: false);
				return;
			}
			rectTransform.position = PlayerSingleton<PlayerCamera>.Instance.Camera.WorldToScreenPoint(position);
			float num = 0.3f;
			float num2 = 1f;
			float num3 = 3f;
			float num4 = 30f;
			float num5 = Vector3.Distance(position, PlayerSingleton<PlayerCamera>.Instance.transform.position);
			float num6 = 1f - Mathf.Clamp((num5 - num3) / (num4 - num3), 0f, 1f);
			float num7 = num + (num2 - num) * num6;
			rectTransform.localScale = new Vector3(num7, num7, num7);
			base.gameObject.SetActive(value: true);
		}

		public void Clicked()
		{
			SetIsSelected(!isSelected);
			if (isSelected)
			{
				Singleton<FeaturesManager>.Instance.OpenFeatureMenu(feature);
			}
			else
			{
				Singleton<FeaturesManager>.Instance.CloseFeatureMenu();
			}
		}

		public void SetIsSelected(bool s)
		{
			isSelected = s;
			if (isSelected)
			{
				if (selectedFeatureIcon != null && selectedFeatureIcon != this)
				{
					selectedFeatureIcon.SetIsSelected(s: false);
				}
				selectedFeatureIcon = this;
			}
			else if (selectedFeatureIcon == this)
			{
				selectedFeatureIcon = null;
			}
			if (!hovered)
			{
				text.gameObject.SetActive(value: false);
			}
			UpdateColors();
		}

		private void UpdateColors()
		{
			if (isSelected)
			{
				background.color = new Color32(byte.MaxValue, 156, 37, byte.MaxValue);
				icon.color = Color.white;
			}
			else
			{
				background.color = Color.white;
				icon.color = new Color32(byte.MaxValue, 156, 37, byte.MaxValue);
			}
		}

		public void PointerEnter()
		{
			hovered = true;
			text.gameObject.SetActive(value: true);
		}

		public void PointerExit()
		{
			hovered = false;
			if (!isSelected)
			{
				text.gameObject.SetActive(value: false);
			}
		}
	}
}
