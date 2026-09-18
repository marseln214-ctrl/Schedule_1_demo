namespace ScheduleOne.Persistence.Datas
{
	public class GameData : SaveData
	{
		public string OrganisationName;

		public int Seed;

		public GameData(string organisationName, int seed)
		{
			OrganisationName = organisationName;
			Seed = seed;
		}

		public GameData()
		{
			OrganisationName = "Organisation";
			Seed = 0;
		}
	}
}
