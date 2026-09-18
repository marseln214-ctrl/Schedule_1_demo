using ScheduleOne.DevUtilities;
using ScheduleOne.ItemFramework;
using ScheduleOne.ObjectScripts;
using UnityEngine;

namespace ScheduleOne.PlayerTasks.Tasks
{
	public class PourOntoTargetTask : PourIntoPotTask
	{
		public Transform Target;

		public float SUCCESS_THRESHOLD = 0.12f;

		public float SUCCESS_TIME = 0.4f;

		private float timeOverTarget;

		public PourOntoTargetTask(Pot _pot, ItemInstance _itemInstance, Pourable _pourablePrefab)
			: base(_pot, _itemInstance, _pourablePrefab)
		{
			Target = _pot.Target;
			_pot.RandomizeTarget();
			_pot.SetTargetActive(active: true);
		}

		public override void Update()
		{
			base.Update();
			Vector3 vector = pourable.PourPoint.position - Target.position;
			vector.y = 0f;
			if (vector.magnitude < SUCCESS_THRESHOLD)
			{
				timeOverTarget += Time.deltaTime * pourable.NormalizedPourRate;
				if (timeOverTarget >= SUCCESS_TIME)
				{
					TargetReached();
				}
			}
			else
			{
				timeOverTarget = 0f;
			}
		}

		public override void StopTask()
		{
			pot.SetTargetActive(active: false);
			base.StopTask();
		}

		public virtual void TargetReached()
		{
			pot.RandomizeTarget();
			timeOverTarget = 0f;
			Singleton<TaskManager>.Instance.PlayTaskCompleteSound();
		}
	}
}
