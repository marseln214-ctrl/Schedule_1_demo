using ScheduleOne.EntityFramework;
using ScheduleOne.ObjectScripts;
using ScheduleOne.Persistence.Datas;

namespace ScheduleOne.Persistence.Loaders
{
	public class SoilPourerLoader : GridItemLoader
	{
		public override string ItemType => typeof(SoilPourerData).Name;

		public override void Load(string mainPath)
		{
			GridItem gridItem = LoadAndCreate(mainPath);
			if (gridItem == null)
			{
				Console.LogWarning("Failed to load grid item");
				return;
			}
			SoilPourerData data = GetData<SoilPourerData>(mainPath);
			if (data == null)
			{
				Console.LogWarning("Failed to load toggleableitem data");
				return;
			}
			SoilPourer soilPourer = gridItem as SoilPourer;
			if (soilPourer != null)
			{
				soilPourer.SendSoil(data.SoilID);
			}
		}
	}
}
