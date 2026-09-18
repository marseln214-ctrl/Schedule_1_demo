using System;
using ScheduleOne.DevUtilities;
using UnityEngine;

namespace ScheduleOne.Variables
{
	[Serializable]
	public class Condition
	{
		public enum EConditionType
		{
			GreaterThan = 0,
			LessThan = 1,
			EqualTo = 2,
			NotEqualTo = 3,
			GreaterThanOrEqualTo = 4,
			LessThanOrEqualTo = 5
		}

		public string VariableName = "Variable Name";

		public EConditionType Operator = EConditionType.EqualTo;

		public string Value = "true";

		public bool Evaluate()
		{
			if (!NetworkSingleton<VariableDatabase>.InstanceExists)
			{
				return false;
			}
			BaseVariable variable = NetworkSingleton<VariableDatabase>.Instance.GetVariable(VariableName);
			if (variable == null)
			{
				Debug.LogError("Variable " + VariableName + " not found");
				return false;
			}
			return variable.EvaluateCondition(Operator, Value);
		}
	}
}
