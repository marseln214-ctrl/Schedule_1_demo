using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace ScheduleOne.DevUtilities
{
	public class RebindActionUI : MonoBehaviour
	{
		[Serializable]
		public class UpdateBindingUIEvent : UnityEvent<RebindActionUI, string, string, string>
		{
		}

		[Serializable]
		public class InteractiveRebindEvent : UnityEvent<RebindActionUI, InputActionRebindingExtensions.RebindingOperation>
		{
		}

		public Action onRebind;

		[Tooltip("Reference to action that is to be rebound from the UI.")]
		[SerializeField]
		private InputActionReference m_Action;

		[SerializeField]
		private string m_BindingId;

		[SerializeField]
		private InputBinding.DisplayStringOptions m_DisplayStringOptions;

		[Tooltip("Text label that will receive the name of the action. Optional. Set to None to have the rebind UI not show a label for the action.")]
		[SerializeField]
		private TextMeshProUGUI m_ActionLabel;

		[Tooltip("Text label that will receive the current, formatted binding string.")]
		[SerializeField]
		private TextMeshProUGUI m_BindingText;

		[Tooltip("Optional UI that will be shown while a rebind is in progress.")]
		[SerializeField]
		private GameObject m_RebindOverlay;

		[Tooltip("Optional text label that will be updated with prompt for user input.")]
		[SerializeField]
		private TextMeshProUGUI m_RebindText;

		[Tooltip("Event that is triggered when the way the binding is display should be updated. This allows displaying bindings in custom ways, e.g. using images instead of text.")]
		[SerializeField]
		private UpdateBindingUIEvent m_UpdateBindingUIEvent;

		[Tooltip("Event that is triggered when an interactive rebind is being initiated. This can be used, for example, to implement custom UI behavior while a rebind is in progress. It can also be used to further customize the rebind.")]
		[SerializeField]
		private InteractiveRebindEvent m_RebindStartEvent;

		[Tooltip("Event that is triggered when an interactive rebind is complete or has been aborted.")]
		[SerializeField]
		private InteractiveRebindEvent m_RebindStopEvent;

		private InputActionRebindingExtensions.RebindingOperation m_RebindOperation;

		private static List<RebindActionUI> s_RebindActionUIs;

		public InputActionReference actionReference
		{
			get
			{
				return m_Action;
			}
			set
			{
				m_Action = value;
				UpdateActionLabel();
				UpdateBindingDisplay();
			}
		}

		public string bindingId
		{
			get
			{
				return m_BindingId;
			}
			set
			{
				m_BindingId = value;
				UpdateBindingDisplay();
			}
		}

		public InputBinding.DisplayStringOptions displayStringOptions
		{
			get
			{
				return m_DisplayStringOptions;
			}
			set
			{
				m_DisplayStringOptions = value;
				UpdateBindingDisplay();
			}
		}

		public TextMeshProUGUI actionLabel
		{
			get
			{
				return m_ActionLabel;
			}
			set
			{
				m_ActionLabel = value;
				UpdateActionLabel();
			}
		}

		public TextMeshProUGUI bindingText
		{
			get
			{
				return m_BindingText;
			}
			set
			{
				m_BindingText = value;
				UpdateBindingDisplay();
			}
		}

		public TextMeshProUGUI rebindPrompt
		{
			get
			{
				return m_RebindText;
			}
			set
			{
				m_RebindText = value;
			}
		}

		public GameObject rebindOverlay
		{
			get
			{
				return m_RebindOverlay;
			}
			set
			{
				m_RebindOverlay = value;
			}
		}

		public UpdateBindingUIEvent updateBindingUIEvent
		{
			get
			{
				if (m_UpdateBindingUIEvent == null)
				{
					m_UpdateBindingUIEvent = new UpdateBindingUIEvent();
				}
				return m_UpdateBindingUIEvent;
			}
		}

		public InteractiveRebindEvent startRebindEvent
		{
			get
			{
				if (m_RebindStartEvent == null)
				{
					m_RebindStartEvent = new InteractiveRebindEvent();
				}
				return m_RebindStartEvent;
			}
		}

		public InteractiveRebindEvent stopRebindEvent
		{
			get
			{
				if (m_RebindStopEvent == null)
				{
					m_RebindStopEvent = new InteractiveRebindEvent();
				}
				return m_RebindStopEvent;
			}
		}

		public InputActionRebindingExtensions.RebindingOperation ongoingRebind => m_RebindOperation;

		public bool ResolveActionAndBinding(out InputAction action, out int bindingIndex)
		{
			bindingIndex = -1;
			action = m_Action?.action;
			if (action == null)
			{
				return false;
			}
			if (string.IsNullOrEmpty(m_BindingId))
			{
				return false;
			}
			Guid bindingId = new Guid(m_BindingId);
			bindingIndex = action.bindings.IndexOf((InputBinding x) => x.id == bindingId);
			if (bindingIndex == -1)
			{
				Debug.LogError($"Cannot find binding with ID '{bindingId}' on '{action}'", this);
				return false;
			}
			return true;
		}

		public void UpdateBindingDisplay()
		{
			string text = string.Empty;
			string deviceLayoutName = null;
			string controlPath = null;
			InputAction inputAction = m_Action?.action;
			if (inputAction != null)
			{
				int num = inputAction.bindings.IndexOf((InputBinding x) => x.id.ToString() == m_BindingId);
				if (num != -1)
				{
					text = inputAction.GetBindingDisplayString(num, out deviceLayoutName, out controlPath, displayStringOptions);
				}
			}
			m_BindingText.gameObject.SetActive(value: true);
			if (m_BindingText != null)
			{
				m_BindingText.text = text;
			}
			m_UpdateBindingUIEvent?.Invoke(this, text, deviceLayoutName, controlPath);
		}

		public void ResetToDefault()
		{
			if (!ResolveActionAndBinding(out var action, out var bindingIndex))
			{
				return;
			}
			if (action.bindings[bindingIndex].isComposite)
			{
				for (int i = bindingIndex + 1; i < action.bindings.Count && action.bindings[i].isPartOfComposite; i++)
				{
					action.RemoveBindingOverride(i);
				}
			}
			else
			{
				action.RemoveBindingOverride(bindingIndex);
			}
			UpdateBindingDisplay();
		}

		public void StartInteractiveRebind()
		{
			m_Action.action.Disable();
			if (!ResolveActionAndBinding(out var action, out var bindingIndex))
			{
				return;
			}
			if (action.bindings[bindingIndex].isComposite)
			{
				int num = bindingIndex + 1;
				if (num < action.bindings.Count && action.bindings[num].isPartOfComposite)
				{
					PerformInteractiveRebind(action, num, allCompositeParts: true);
				}
			}
			else
			{
				PerformInteractiveRebind(action, bindingIndex);
			}
		}

		private void PerformInteractiveRebind(InputAction action, int bindingIndex, bool allCompositeParts = false)
		{
			m_RebindOperation?.Cancel();
			m_RebindOperation = action.PerformInteractiveRebinding(bindingIndex).OnCancel(delegate(InputActionRebindingExtensions.RebindingOperation operation)
			{
				m_RebindStopEvent?.Invoke(this, operation);
				if (m_RebindOverlay != null)
				{
					m_RebindOverlay?.SetActive(value: false);
				}
				UpdateBindingDisplay();
				CleanUp();
			}).OnComplete(delegate(InputActionRebindingExtensions.RebindingOperation operation)
			{
				m_RebindOverlay?.SetActive(value: false);
				m_RebindStopEvent?.Invoke(this, operation);
				UpdateBindingDisplay();
				CleanUp();
				if (allCompositeParts)
				{
					int num = bindingIndex + 1;
					if (num < action.bindings.Count && action.bindings[num].isPartOfComposite)
					{
						PerformInteractiveRebind(action, num, allCompositeParts: true);
					}
				}
				onRebind?.Invoke();
			})
				.WithControlsExcluding("Mouse");
			if (action.bindings[bindingIndex].isPartOfComposite)
			{
				_ = "Binding '" + action.bindings[bindingIndex].name + "'. ";
			}
			if (m_RebindOverlay != null)
			{
				m_RebindOverlay?.SetActive(value: true);
			}
			if (m_RebindText != null)
			{
				m_RebindText.text = "Press key...";
			}
			if (m_RebindOverlay == null && m_RebindText == null && m_RebindStartEvent == null && m_BindingText != null)
			{
				m_BindingText.text = "<Waiting...>";
			}
			m_BindingText.gameObject.SetActive(value: false);
			m_RebindStartEvent?.Invoke(this, m_RebindOperation);
			m_RebindOperation.Start();
			void CleanUp()
			{
				m_RebindOperation?.Dispose();
				m_RebindOperation = null;
				m_Action.action.Enable();
			}
		}

		protected void OnEnable()
		{
			if (s_RebindActionUIs == null)
			{
				s_RebindActionUIs = new List<RebindActionUI>();
			}
			s_RebindActionUIs.Add(this);
			if (s_RebindActionUIs.Count == 1)
			{
				InputSystem.onActionChange += OnActionChange;
			}
		}

		protected void OnDisable()
		{
			m_RebindOperation?.Dispose();
			m_RebindOperation = null;
			s_RebindActionUIs.Remove(this);
			if (s_RebindActionUIs.Count == 0)
			{
				s_RebindActionUIs = null;
				InputSystem.onActionChange -= OnActionChange;
			}
		}

		private static void OnActionChange(object obj, InputActionChange change)
		{
			if (change != InputActionChange.BoundControlsChanged)
			{
				return;
			}
			InputAction inputAction = obj as InputAction;
			InputActionMap inputActionMap = inputAction?.actionMap ?? (obj as InputActionMap);
			InputActionAsset inputActionAsset = inputActionMap?.asset ?? (obj as InputActionAsset);
			for (int i = 0; i < s_RebindActionUIs.Count; i++)
			{
				RebindActionUI rebindActionUI = s_RebindActionUIs[i];
				InputAction inputAction2 = rebindActionUI.actionReference?.action;
				if (inputAction2 != null && (inputAction2 == inputAction || inputAction2.actionMap == inputActionMap || inputAction2.actionMap?.asset == inputActionAsset))
				{
					rebindActionUI.UpdateBindingDisplay();
				}
			}
		}

		private void UpdateActionLabel()
		{
			if (m_ActionLabel != null)
			{
				InputAction inputAction = m_Action?.action;
				m_ActionLabel.text = ((inputAction != null) ? inputAction.name : string.Empty);
			}
		}
	}
}
