using System;
using ScheduleOne.DevUtilities;
using ScheduleOne.PlayerTasks;
using ScheduleOne.Variables;
using UnityEngine;

namespace ScheduleOne.UI
{
	public class TaskManagerUI : Singleton<TaskManagerUI>
	{
		private bool textShown;

		public GenericUIScreen inputPromptUI;

		public Canvas canvas;

		public RectTransform multiGrabIndicator;

		public GenericUIScreen PackagingStationMK2TutorialDone;

		protected virtual void Update()
		{
			UpdateInstructionLabel();
			canvas.enabled = Singleton<TaskManager>.Instance.currentTask != null;
		}

		protected override void Start()
		{
			base.Start();
			TaskManager taskManager = Singleton<TaskManager>.Instance;
			taskManager.OnTaskStarted = (Action<Task>)Delegate.Combine(taskManager.OnTaskStarted, new Action<Task>(TaskStarted));
			multiGrabIndicator.gameObject.SetActive(value: false);
		}

		protected virtual void UpdateInstructionLabel()
		{
			if (Singleton<TaskManager>.Instance.currentTask != null && Singleton<TaskManager>.Instance.currentTask.CurrentInstruction != string.Empty)
			{
				textShown = true;
				Singleton<HUD>.Instance.ShowTopScreenText(Singleton<TaskManager>.Instance.currentTask.CurrentInstruction);
			}
			else if (textShown)
			{
				textShown = false;
				Singleton<HUD>.Instance.HideTopScreenText();
			}
		}

		private void TaskStarted(Task task)
		{
			bool value = NetworkSingleton<VariableDatabase>.Instance.GetValue<bool>("InputHintsTutorialDone");
			multiGrabIndicator.gameObject.SetActive(value: false);
			if (!value && !Application.isEditor)
			{
				NetworkSingleton<VariableDatabase>.Instance.SetVariableValue("InputHintsTutorialDone", true.ToString());
				inputPromptUI.Open();
			}
		}
	}
}
