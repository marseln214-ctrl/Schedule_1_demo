using ScheduleOne.DevUtilities;

namespace ScheduleOne.Persistence
{
	public class SaveRequest
	{
		public ISaveable Saveable;

		public string ParentFolderPath;

		public string SaveString { get; private set; }

		public SaveRequest(ISaveable saveable, string parentFolderPath)
		{
			Saveable = saveable;
			ParentFolderPath = parentFolderPath;
			SaveString = saveable.GetSaveString();
			if (SaveString != string.Empty)
			{
				Singleton<SaveManager>.Instance.QueueSaveRequest(this);
			}
			else
			{
				saveable.CompleteSave(parentFolderPath, writeDataFile: false);
			}
		}

		public void Complete()
		{
			Singleton<SaveManager>.Instance.DequeueSaveRequest(this);
			Saveable.WriteBaseData(ParentFolderPath, SaveString);
		}
	}
}
