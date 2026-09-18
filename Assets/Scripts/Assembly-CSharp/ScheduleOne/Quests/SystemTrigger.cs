using System;
using ScheduleOne.Variables;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.Quests
{
	[Serializable]
	public class SystemTrigger
	{
		public Conditions Conditions;

		[Header("True")]
		public VariableSetter[] onEvaluateTrueVariableSetters;

		public QuestStateSetter[] onEvaluateTrueQuestSetters;

		public UnityEvent onEvaluateTrue;

		[Header("False")]
		public VariableSetter[] onEvaluateFalseVariableSetters;

		public QuestStateSetter[] onEvaluateFalseQuestSetters;

		public UnityEvent onEvaluateFalse;

		public bool Trigger()
		{
			if (Conditions.Evaluate())
			{
				for (int i = 0; i < onEvaluateTrueQuestSetters.Length; i++)
				{
					onEvaluateTrueQuestSetters[i].Execute();
				}
				for (int j = 0; j < onEvaluateTrueVariableSetters.Length; j++)
				{
					onEvaluateTrueVariableSetters[j].Execute();
				}
				if (onEvaluateTrue != null)
				{
					onEvaluateTrue.Invoke();
				}
				return true;
			}
			for (int k = 0; k < onEvaluateFalseQuestSetters.Length; k++)
			{
				onEvaluateFalseQuestSetters[k].Execute();
			}
			for (int l = 0; l < onEvaluateFalseVariableSetters.Length; l++)
			{
				onEvaluateFalseVariableSetters[l].Execute();
			}
			if (onEvaluateFalse != null)
			{
				onEvaluateFalse.Invoke();
			}
			return false;
		}
	}
}
