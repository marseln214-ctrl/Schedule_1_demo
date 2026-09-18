using ScheduleOne.AvatarFramework.Customization;
using ScheduleOne.Clothing;
using ScheduleOne.DevUtilities;

namespace ScheduleOne.UI.CharacterCreator
{
	public class CharacterCreatorField<T> : BaseCharacterCreatorField
	{
		protected ClothingDefinition selectedClothingDefinition;

		public T value { get; protected set; }

		public virtual T ReadValue()
		{
			return Singleton<ScheduleOne.AvatarFramework.Customization.CharacterCreator>.Instance.ActiveSettings.GetValue<T>(PropertyName);
		}

		public virtual void WriteValue(bool applyValue = true)
		{
			Singleton<ScheduleOne.AvatarFramework.Customization.CharacterCreator>.Instance.SetValue(PropertyName, value, selectedClothingDefinition);
			Singleton<ScheduleOne.AvatarFramework.Customization.CharacterCreator>.Instance.RefreshCategory(Category);
			if (applyValue)
			{
				ApplyValue();
			}
		}

		public override void ApplyValue()
		{
			base.ApplyValue();
			value = ReadValue();
		}
	}
}
