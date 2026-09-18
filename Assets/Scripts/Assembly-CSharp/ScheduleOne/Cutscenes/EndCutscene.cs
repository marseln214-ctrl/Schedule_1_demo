using UnityEngine.Events;

namespace ScheduleOne.Cutscenes
{
	public class EndCutscene : Cutscene
	{
		public UnityEvent onStandUp;

		public UnityEvent onRunStart;

		public UnityEvent onEngineStart;

		public void StandUp()
		{
			if (onStandUp != null)
			{
				Console.Log("StandUp");
				onStandUp.Invoke();
			}
		}

		public void RunStart()
		{
			if (onRunStart != null)
			{
				Console.Log("RunStart");
				onRunStart.Invoke();
			}
		}

		public void EngineStart()
		{
			if (onEngineStart != null)
			{
				Console.Log("EngineStart");
				onEngineStart.Invoke();
			}
		}
	}
}
