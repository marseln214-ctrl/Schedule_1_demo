using System.Collections.Generic;
using ScheduleOne.DevUtilities;
using ScheduleOne.Growing;
using ScheduleOne.ItemFramework;

namespace ScheduleOne.Management
{
	public class ManagementUtilities : Singleton<ManagementUtilities>
	{
		public List<string> weedSeedAssetPaths = new List<string>();

		public List<string> additiveAssetPaths = new List<string>();

		public List<SeedDefinition> SeedDefintions = new List<SeedDefinition>();

		public List<AdditiveDefinition> AdditiveDefinitions = new List<AdditiveDefinition>();

		public static List<string> WeedSeedAssetPaths => Singleton<ManagementUtilities>.Instance.weedSeedAssetPaths;

		public static List<string> AdditiveAssetPaths => Singleton<ManagementUtilities>.Instance.additiveAssetPaths;
	}
}
