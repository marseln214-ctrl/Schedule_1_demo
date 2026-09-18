using HSVPicker;
using UnityEngine;
using UnityEngine.UI;

namespace ScheduleOne.UI.CharacterCustomization
{
	public class BarbershopUI : CharacterCustomizationUI
	{
		public HSVPicker.ColorPicker ColorPicker;

		public Button ApplyColorButton;

		private Color appliedColor = Color.black;

		public override bool IsOptionCurrentlyApplied(CharacterCustomizationOption option)
		{
			return currentSettings.HairStyle == option.Label;
		}

		public override void OptionSelected(CharacterCustomizationOption option)
		{
			base.OptionSelected(option);
			currentSettings.HairStyle = option.Label;
			AvatarRig.ApplyHairSettings(currentSettings.GetAvatarSettings());
		}

		protected override void Update()
		{
			base.Update();
			if (base.IsOpen)
			{
				_ = currentSettings == null;
			}
		}

		public override void Open()
		{
			base.Open();
			ColorPicker.CurrentColor = currentSettings.HairColor;
			appliedColor = currentSettings.HairColor;
			ApplyColorButton.interactable = false;
		}

		public void ColorFieldChanged(Color color)
		{
			currentSettings.HairColor = color;
			AvatarRig.ApplyHairColorSettings(currentSettings.GetAvatarSettings());
			ApplyColorButton.interactable = true;
		}

		public void ApplyColorChange()
		{
			appliedColor = ColorPicker.CurrentColor;
			currentSettings.HairColor = appliedColor;
			AvatarRig.ApplyHairSettings(currentSettings.GetAvatarSettings());
			ApplyColorButton.interactable = false;
		}

		public void RevertColorChange()
		{
			ColorPicker.CurrentColor = currentSettings.HairColor;
			currentSettings.HairColor = appliedColor;
			AvatarRig.ApplyHairSettings(currentSettings.GetAvatarSettings());
			ApplyColorButton.interactable = false;
		}
	}
}
