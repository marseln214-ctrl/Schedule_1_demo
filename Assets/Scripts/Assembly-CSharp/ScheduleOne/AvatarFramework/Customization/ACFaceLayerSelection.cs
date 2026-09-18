namespace ScheduleOne.AvatarFramework.Customization
{
	public class ACFaceLayerSelection : ACSelection<FaceLayer>
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
			FaceLayer faceLayer = Options.Find((FaceLayer x) => x.AssetPath == path);
			if (!(faceLayer != null))
			{
				return -1;
			}
			return Options.IndexOf(faceLayer);
		}
	}
}
