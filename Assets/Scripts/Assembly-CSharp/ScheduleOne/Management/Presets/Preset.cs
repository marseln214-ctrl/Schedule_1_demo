using UnityEngine;

namespace ScheduleOne.Management.Presets
{
	public abstract class Preset
	{
		public delegate void NameChange(string name);

		public delegate void PresetDeletion(Preset replacement);

		public string PresetName = "Default";

		public Color32 PresetColor = new Color32(180, 180, 180, byte.MaxValue);

		public ManageableObjectType ObjectType;

		public NameChange onNameChanged;

		public PresetDeletion onDeleted;

		public Preset()
		{
			InitializeOptions();
		}

		public abstract Preset GetCopy();

		public virtual void CopyTo(Preset other)
		{
			other.PresetName = PresetName;
			other.PresetColor = PresetColor;
		}

		public abstract void InitializeOptions();

		public void SetName(string newName)
		{
			if (!(PresetName == newName))
			{
				PresetName = newName;
				if (onNameChanged != null)
				{
					onNameChanged(newName);
				}
			}
		}

		public void DeletePreset(Preset replacement)
		{
			if (onDeleted != null)
			{
				onDeleted(replacement);
			}
		}

		public static Preset GetDefault(ManageableObjectType type)
		{
			if (type == ManageableObjectType.Pot)
			{
				return PotPreset.GetDefaultPreset();
			}
			Console.LogWarning("GetDefault: type not accounted for");
			return null;
		}
	}
}
