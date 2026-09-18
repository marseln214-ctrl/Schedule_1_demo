using ScheduleOne.PlayerScripts;

namespace ScheduleOne.Variables
{
	public class NumberVariable : Variable<float>
	{
		public NumberVariable(string name, EVariableReplicationMode replicationMode, bool persistent, EVariableMode mode, Player owner, float value)
			: base(name, replicationMode, persistent, mode, owner, value)
		{
		}

		public override bool TryDeserialize(string valueString, out float value)
		{
			if (float.TryParse(valueString, out var result))
			{
				value = result;
				return true;
			}
			value = 0f;
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
			case Condition.EConditionType.GreaterThan:
				return Value > value2;
			case Condition.EConditionType.LessThan:
				return Value < value2;
			case Condition.EConditionType.GreaterThanOrEqualTo:
				return Value >= value2;
			case Condition.EConditionType.LessThanOrEqualTo:
				return Value <= value2;
			default:
				Console.LogError("Invalid operation " + operation.ToString() + " for number variable");
				return false;
			}
		}
	}
}
