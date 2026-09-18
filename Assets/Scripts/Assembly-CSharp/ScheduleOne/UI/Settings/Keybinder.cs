using System;
using System.Collections;
using ScheduleOne.DevUtilities;
using UnityEngine;

namespace ScheduleOne.UI.Settings
{
	public class Keybinder : MonoBehaviour
	{
		public RebindActionUI rebindActionUI;

		private void Awake()
		{
			RebindActionUI obj = rebindActionUI;
			obj.onRebind = (Action)Delegate.Combine(obj.onRebind, new Action(OnRebind));
		}

		private void Start()
		{
			ScheduleOne.DevUtilities.Settings instance = Singleton<ScheduleOne.DevUtilities.Settings>.Instance;
			instance.onInputsApplied = (Action)Delegate.Remove(instance.onInputsApplied, new Action(OnSettingsApplied));
			ScheduleOne.DevUtilities.Settings instance2 = Singleton<ScheduleOne.DevUtilities.Settings>.Instance;
			instance2.onInputsApplied = (Action)Delegate.Combine(instance2.onInputsApplied, new Action(OnSettingsApplied));
			rebindActionUI.UpdateBindingDisplay();
		}

		private void OnDestroy()
		{
			if (rebindActionUI != null)
			{
				RebindActionUI obj = rebindActionUI;
				obj.onRebind = (Action)Delegate.Remove(obj.onRebind, new Action(OnRebind));
			}
			if (Singleton<ScheduleOne.DevUtilities.Settings>.InstanceExists)
			{
				ScheduleOne.DevUtilities.Settings instance = Singleton<ScheduleOne.DevUtilities.Settings>.Instance;
				instance.onInputsApplied = (Action)Delegate.Remove(instance.onInputsApplied, new Action(OnSettingsApplied));
			}
		}

		private void OnRebind()
		{
			StartCoroutine(ApplySettings());
			static IEnumerator ApplySettings()
			{
				yield return new WaitForEndOfFrame();
				Singleton<ScheduleOne.DevUtilities.Settings>.Instance.WriteInputSettings(Singleton<ScheduleOne.DevUtilities.Settings>.Instance.InputSettings);
				Singleton<ScheduleOne.DevUtilities.Settings>.Instance.ApplyInputSettings(Singleton<ScheduleOne.DevUtilities.Settings>.Instance.ReadInputSettings());
			}
		}

		private void OnSettingsApplied()
		{
			rebindActionUI.UpdateBindingDisplay();
		}
	}
}
