using FishNet;
using ScheduleOne.DevUtilities;
using ScheduleOne.Interaction;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.Persistence
{
	public class SavePoint : MonoBehaviour
	{
		public const float SAVE_COOLDOWN = 60f;

		public InteractableObject IntObj;

		public UnityEvent onSaveStart;

		public UnityEvent onSaveComplete;

		public void Awake()
		{
			IntObj.onHovered.AddListener(Hovered);
			IntObj.onInteractStart.AddListener(Interacted);
		}

		public void Hovered()
		{
			if (!InstanceFinder.IsServer)
			{
				IntObj.SetMessage("Only host can save");
				IntObj.SetInteractableState(InteractableObject.EInteractableState.Invalid);
				return;
			}
			if (Singleton<SaveManager>.Instance.IsSaving)
			{
				IntObj.SetInteractableState(InteractableObject.EInteractableState.Disabled);
			}
			if (CanSave(out var reason))
			{
				IntObj.SetInteractableState(InteractableObject.EInteractableState.Default);
				IntObj.SetMessage("Save game");
			}
			else if (Singleton<SaveManager>.Instance.SecondsSinceLastSave < 10f)
			{
				IntObj.SetMessage("Game saved!");
				IntObj.SetInteractableState(InteractableObject.EInteractableState.Label);
			}
			else
			{
				IntObj.SetMessage(reason);
				IntObj.SetInteractableState(InteractableObject.EInteractableState.Invalid);
			}
		}

		private bool CanSave(out string reason)
		{
			reason = string.Empty;
			if (Singleton<SaveManager>.Instance.SecondsSinceLastSave < 60f)
			{
				reason = "Wait " + Mathf.Ceil(60f - Singleton<SaveManager>.Instance.SecondsSinceLastSave) + "s";
				return false;
			}
			if (Singleton<SaveManager>.Instance.SecondsSinceLastSave < 60f)
			{
				reason = "Wait " + Mathf.Ceil(60f - Singleton<SaveManager>.Instance.SecondsSinceLastSave) + "s";
				return false;
			}
			return true;
		}

		public void Interacted()
		{
			if (CanSave(out var _))
			{
				Save();
			}
		}

		private void Save()
		{
			Singleton<SaveManager>.Instance.onSaveComplete.RemoveListener(OnSaveComplete);
			Singleton<SaveManager>.Instance.onSaveComplete.AddListener(OnSaveComplete);
			Singleton<SaveManager>.Instance.Save();
			if (onSaveStart != null)
			{
				onSaveStart.Invoke();
			}
		}

		public void OnSaveComplete()
		{
			if (onSaveComplete != null)
			{
				onSaveComplete.Invoke();
			}
		}
	}
}
