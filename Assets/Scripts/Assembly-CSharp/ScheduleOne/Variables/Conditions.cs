using System;

namespace ScheduleOne.Variables
{
	[Serializable]
	public class Conditions
	{
		public enum EEvaluationType
		{
			And = 0,
			Or = 1
		}

		public EEvaluationType EvaluationType;

		public Condition[] ConditionList;

		public QuestCondition[] QuestConditionList;

		public bool Evaluate()
		{
			bool flag = false;
			for (int i = 0; i < ConditionList.Length; i++)
			{
				if (ConditionList[i].Evaluate())
				{
					flag = true;
					if (EvaluationType != 0)
					{
						return true;
					}
				}
				else if (EvaluationType == EEvaluationType.And)
				{
					return false;
				}
			}
			for (int j = 0; j < QuestConditionList.Length; j++)
			{
				if (QuestConditionList[j].Evaluate())
				{
					flag = true;
					if (EvaluationType != 0)
					{
						return true;
					}
				}
				else if (EvaluationType == EEvaluationType.And)
				{
					return false;
				}
			}
			if (!flag)
			{
				return ConditionList.Length + QuestConditionList.Length == 0;
			}
			return true;
		}
	}
}
