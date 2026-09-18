namespace ScheduleOne.Management.Presets.Options
{
	public abstract class Option
	{
		public string Name { get; protected set; } = "OptionName";


		public Option(string name)
		{
			Name = name;
		}

		public virtual void CopyTo(Option other)
		{
			other.Name = Name;
		}

		public abstract string GetDisplayString();
	}
}
