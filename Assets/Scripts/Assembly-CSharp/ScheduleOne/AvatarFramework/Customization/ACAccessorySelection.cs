namespace ScheduleOne.AvatarFramework.Customization
{
	public class ACAccessorySelection : ACSelection<Accessory>
	{
		public override string GetOptionLabel(int index)
		{
			return Options[index].Name;
		}

		public override void CallValueChange()
		{
			if (onValueChange != null)
			{
				onValueChange.Invoke((SelectedOptionIndex == -1) ? null : Options[SelectedOptionIndex]);
			}
			if (onValueChangeWithIndex != null)
			{
				onValueChangeWithIndex.Invoke((SelectedOptionIndex == -1) ? null : Options[SelectedOptionIndex], PropertyIndex);
			}
		}

		public override int GetAssetPathIndex(string path)
		{
			Accessory accessory = Options.Find((Accessory x) => x.AssetPath == path);
			if (!(accessory != null))
			{
				return -1;
			}
			return Options.IndexOf(accessory);
		}
	}
}
