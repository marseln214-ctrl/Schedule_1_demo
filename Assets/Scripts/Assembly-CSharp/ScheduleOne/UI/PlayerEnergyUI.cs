using System;
using ScheduleOne.DevUtilities;
using ScheduleOne.PlayerScripts;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ScheduleOne.UI
{
	public class PlayerEnergyUI : Singleton<PlayerEnergyUI>
	{
		public Slider Slider;

		public RectTransform SliderRect;

		public Image FillImage;

		public TextMeshProUGUI Label;

		[Header("Settings")]
		public Color SliderColor_Green;

		public Color SliderColor_Red;

		private float displayedValue = 1f;

		protected override void Awake()
		{
			base.Awake();
			Player.onLocalPlayerSpawned = (Action)Delegate.Combine(Player.onLocalPlayerSpawned, (Action)delegate
			{
				UpdateDisplayedEnergy();
				Player.Local.Energy.onEnergyChanged.AddListener(UpdateDisplayedEnergy);
			});
		}

		private void UpdateDisplayedEnergy()
		{
			SetDisplayedEnergy(Player.Local.Energy.CurrentEnergy);
		}

		public void SetDisplayedEnergy(float energy)
		{
			displayedValue = energy;
			Slider.value = energy / 100f;
			FillImage.color = ((energy <= 20f) ? SliderColor_Red : SliderColor_Green);
		}

		protected virtual void Update()
		{
			if (displayedValue < 20f)
			{
				float num = Mathf.Clamp((20f - displayedValue) / 20f, 0.25f, 1f);
				float num2 = num * 3f;
				SliderRect.anchoredPosition = new Vector2(UnityEngine.Random.Range(0f - num2, num2), UnityEngine.Random.Range(0f - num2, num2));
				Color white = Color.white;
				Color b = Color.Lerp(Color.white, Color.red, num);
				white.a = Label.color.a;
				b.a = Label.color.a;
				Label.color = Color.Lerp(white, b, (Mathf.Sin(Time.timeSinceLevelLoad * num * 10f) + 1f) / 2f);
			}
			else
			{
				SliderRect.anchoredPosition = Vector2.zero;
				Label.color = new Color(1f, 1f, 1f, Label.color.a);
			}
		}
	}
}
