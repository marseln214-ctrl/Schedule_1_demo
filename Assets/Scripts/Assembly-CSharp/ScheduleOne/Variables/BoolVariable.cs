using ScheduleOne.PlayerScripts;

namespace ScheduleOne.Variables
{
	public class BoolVariable : Variable<bool>
	{
		public BoolVariable(string name, EVariableReplicationMode replicationMode, bool persistent, EVariableMode mode, Player owner, bool value)
			: base(name, replicationMode, persistent, mode, owner, value)
		{
		}

		public override bool TryDeserialize(string valueString, out bool value)
		{
			if (valueString.ToLower() == "true")
			{
				value = true;
				return true;
			}
			if (valueString.ToLower() == "false")
			{
				value = false;
				return true;
			}
			value = false;
			return false;
		}

		public override bool EvaluateCondition(Condition.EConditionType operation, string value)
		{
			if (!TryDeserialize(value, out var value2))
			{
				return false;
			}
			switch (operation)
			{
			case Condition.EConditionType.EqualTo:
				return Value == value2;
			case Condition.EConditionType.NotEqualTo:
				return Value != value2;
			default:
				Console.LogError("Invalid operation " + operation.ToString() + " for bool variable");
				return false;
			}
		}
	}
}
