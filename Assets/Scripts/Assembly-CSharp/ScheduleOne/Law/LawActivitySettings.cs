using System;
using UnityEngine;

namespace ScheduleOne.Law
{
	[Serializable]
	public class LawActivitySettings
	{
		public PatrolInstance[] Patrols;

		public CheckpointInstance[] Checkpoints;

		public CurfewInstance[] Curfews;

		public VehiclePatrolInstance[] VehiclePatrols;

		public SentryInstance[] Sentries;

		public void Evaluate()
		{
			for (int i = 0; i < Patrols.Length; i++)
			{
				Patrols[i].Evaluate();
			}
			for (int j = 0; j < Checkpoints.Length; j++)
			{
				Checkpoints[j].Evaluate();
			}
			for (int k = 0; k < Curfews.Length; k++)
			{
				Curfews[k].Evaluate();
			}
			for (int l = 0; l < VehiclePatrols.Length; l++)
			{
				VehiclePatrols[l].Evaluate();
			}
			for (int m = 0; m < Sentries.Length; m++)
			{
				Sentries[m].Evaluate();
			}
		}

		public void End()
		{
			for (int i = 0; i < Curfews.Length; i++)
			{
				if (Curfews[i].Enabled)
				{
					Curfews[i].shouldDisable = true;
				}
			}
		}

		public void OnLoaded()
		{
			Debug.Log("Settings loaded");
			for (int i = 0; i < Curfews.Length; i++)
			{
				Curfews[i].Evaluate(ignoreSleepReq: true);
			}
		}
	}
}
