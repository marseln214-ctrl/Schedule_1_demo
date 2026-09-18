using System;

namespace ScheduleOne.Variables
{
	[Serializable]
	public class VariableCreator
	{
		public string Name;

		public VariableDatabase.EVariableType Type;

		public string InitialValue = string.Empty;

		public bool Persistent = true;

		public EVariableMode Mode;
	}
}
