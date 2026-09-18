using System;
using ScheduleOne.Audio;
using ScheduleOne.DevUtilities;

namespace ScheduleOne.PlayerTasks
{
	public class TaskManager : Singleton<TaskManager>
	{
		public Task currentTask;

		public AudioSourceController TaskCompleteSound;

		public Action<Task> OnTaskStarted;

		protected override void Start()
		{
			base.Start();
			GameInput.RegisterExitListener(Exit, 5);
		}

		protected virtual void Update()
		{
			if (currentTask != null)
			{
				currentTask.Update();
			}
		}

		private void Exit(ExitAction action)
		{
			if (!action.used && action.exitType == ExitType.Escape && currentTask != null)
			{
				action.used = true;
				currentTask.Outcome = Task.EOutcome.Cancelled;
				currentTask.StopTask();
			}
		}

		protected virtual void LateUpdate()
		{
			if (currentTask != null)
			{
				currentTask.LateUpdate();
			}
		}

		protected virtual void FixedUpdate()
		{
			if (currentTask != null)
			{
				currentTask.FixedUpdate();
			}
		}

		public void PlayTaskCompleteSound()
		{
			TaskCompleteSound.Play();
		}

		public void StartTask(Task task)
		{
			currentTask = task;
			if (OnTaskStarted != null)
			{
				OnTaskStarted(task);
			}
		}
	}
}
