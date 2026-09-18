using UnityEngine;
using UnityEngine.UI;

namespace ScheduleOne.UI.CharacterCreator
{
	public class CharacterCreatorSlider : CharacterCreatorField<float>
	{
		[Header("References")]
		public Slider Slider;

		protected override void Awake()
		{
			base.Awake();
			Slider.onValueChanged.AddListener(OnSliderChanged);
		}

		public override void ApplyValue()
		{
			base.ApplyValue();
			Slider.SetValueWithoutNotify(base.value);
		}

		public void OnSliderChanged(float newValue)
		{
			base.value = newValue;
			WriteValue(applyValue: false);
		}
	}
}
