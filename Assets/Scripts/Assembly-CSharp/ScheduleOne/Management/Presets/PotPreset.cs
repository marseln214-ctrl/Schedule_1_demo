using ScheduleOne.Management.Presets.Options;
using UnityEngine;

namespace ScheduleOne.Management.Presets
{
	public class PotPreset : Preset
	{
		public ItemList Seeds;

		public ItemList Additives;

		protected static PotPreset DefaultPreset { get; set; }

		public override Preset GetCopy()
		{
			PotPreset potPreset = new PotPreset();
			CopyTo(potPreset);
			return potPreset;
		}

		public override void CopyTo(Preset other)
		{
			base.CopyTo(other);
			if (other is PotPreset)
			{
				PotPreset potPreset = other as PotPreset;
				Seeds.CopyTo(potPreset.Seeds);
				Additives.CopyTo(potPreset.Additives);
			}
		}

		public override void InitializeOptions()
		{
			Seeds = new ItemList("Seed Types", ManagementUtilities.WeedSeedAssetPaths, canBeAll: true, canBeNone: true);
			Seeds.All = true;
			Additives = new ItemList("Additives", ManagementUtilities.AdditiveAssetPaths, canBeAll: true, canBeNone: true);
			Seeds.None = true;
		}

		public static PotPreset GetDefaultPreset()
		{
			if (DefaultPreset == null)
			{
				DefaultPreset = new PotPreset
				{
					PresetName = "Default",
					ObjectType = ManageableObjectType.Pot,
					PresetColor = new Color32(180, 180, 180, byte.MaxValue)
				};
			}
			return DefaultPreset;
		}

		public static PotPreset GetNewBlankPreset()
		{
			PotPreset obj = GetDefaultPreset().GetCopy() as PotPreset;
			obj.PresetName = "New Preset";
			return obj;
		}
	}
}
