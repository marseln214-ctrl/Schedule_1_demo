namespace ScheduleOne.Management
{
	public abstract class ConfigField
	{
		public EntityConfiguration ParentConfig { get; protected set; }

		public ConfigField(EntityConfiguration parentConfig)
		{
			ParentConfig = parentConfig;
			ParentConfig.Fields.Add(this);
		}

		public abstract bool IsValueDefault();
	}
}
