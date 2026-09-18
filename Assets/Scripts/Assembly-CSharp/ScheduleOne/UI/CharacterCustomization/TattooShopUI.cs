using ScheduleOne.AvatarFramework;

namespace ScheduleOne.UI.CharacterCustomization
{
	public class TattooShopUI : CharacterCustomizationUI
	{
		public override bool IsOptionCurrentlyApplied(CharacterCustomizationOption option)
		{
			Console.Log("Checking if tattoo is applied: " + option.Label);
			Console.Log((currentSettings.Tattoos != null) ? string.Join(", ", currentSettings.Tattoos.ToArray()) : "No tattoos applied");
			if (currentSettings.Tattoos != null)
			{
				return currentSettings.Tattoos.Contains(option.Label);
			}
			return false;
		}

		public override void OptionSelected(CharacterCustomizationOption option)
		{
			base.OptionSelected(option);
			if (!currentSettings.Tattoos.Contains(option.Label))
			{
				currentSettings.Tattoos.Add(option.Label);
			}
			AvatarSettings avatarSettings = currentSettings.GetAvatarSettings();
			AvatarRig.ApplyBodyLayerSettings(avatarSettings, 19);
			AvatarRig.ApplyFaceLayerSettings(avatarSettings);
		}

		public override void OptionDeselected(CharacterCustomizationOption option)
		{
			base.OptionDeselected(option);
			currentSettings.Tattoos.Remove(option.Label);
			AvatarSettings avatarSettings = currentSettings.GetAvatarSettings();
			AvatarRig.ApplyBodyLayerSettings(avatarSettings, 19);
			AvatarRig.ApplyFaceLayerSettings(avatarSettings);
		}
	}
}
