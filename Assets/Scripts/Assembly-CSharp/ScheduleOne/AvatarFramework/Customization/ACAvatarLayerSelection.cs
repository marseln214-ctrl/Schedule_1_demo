namespace ScheduleOne.AvatarFramework.Customization
{
	public class ACAvatarLayerSelection : ACSelection<AvatarLayer>
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
			AvatarLayer avatarLayer = Options.Find((AvatarLayer x) => x.AssetPath == path);
			if (!(avatarLayer != null))
			{
				return -1;
			}
			return Options.IndexOf(avatarLayer);
		}
	}
}
