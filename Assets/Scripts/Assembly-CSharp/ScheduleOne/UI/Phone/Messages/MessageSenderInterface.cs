using System.Collections.Generic;
using ScheduleOne.DevUtilities;
using ScheduleOne.Messaging;
using UnityEngine;
using UnityEngine.UI;

namespace ScheduleOne.UI.Phone.Messages
{
	public class MessageSenderInterface : MonoBehaviour
	{
		public enum EVisibility
		{
			Hidden = 0,
			Docked = 1,
			Expanded = 2
		}

		public EVisibility Visibility;

		[Header("Settings")]
		public float DockedMenuYPos;

		public float ExpandedMenuYPos;

		[Header("References")]
		public RectTransform Menu;

		public RectTransform SendablesContainer;

		public RectTransform[] DockedUIElements;

		public RectTransform[] ExpandedUIElements;

		public Button ComposeButton;

		public Button[] CancelButtons;

		private List<MessageBubble> sendableBubbles = new List<MessageBubble>();

		private Dictionary<MessageBubble, SendableMessage> sendableMap = new Dictionary<MessageBubble, SendableMessage>();

		public void Awake()
		{
			SetVisibility(EVisibility.Hidden);
			ComposeButton.onClick.AddListener(delegate
			{
				SetVisibility(EVisibility.Expanded);
			});
			Button[] cancelButtons = CancelButtons;
			for (int i = 0; i < cancelButtons.Length; i++)
			{
				cancelButtons[i].onClick.AddListener(delegate
				{
					SetVisibility(EVisibility.Docked);
				});
			}
		}

		public void Start()
		{
			GameInput.RegisterExitListener(Exit, 15);
		}

		private void Exit(ExitAction exit)
		{
			if (!exit.used && Visibility == EVisibility.Expanded)
			{
				SetVisibility(EVisibility.Docked);
				exit.used = true;
			}
		}

		public void SetVisibility(EVisibility visibility)
		{
			Visibility = visibility;
			RectTransform[] dockedUIElements = DockedUIElements;
			for (int i = 0; i < dockedUIElements.Length; i++)
			{
				dockedUIElements[i].gameObject.SetActive(visibility == EVisibility.Docked);
			}
			dockedUIElements = ExpandedUIElements;
			for (int i = 0; i < dockedUIElements.Length; i++)
			{
				dockedUIElements[i].gameObject.SetActive(visibility == EVisibility.Expanded);
			}
			if (visibility == EVisibility.Expanded)
			{
				UpdateSendables();
			}
			SendablesContainer.gameObject.SetActive(visibility == EVisibility.Expanded);
			Menu.anchoredPosition = new Vector2(0f, (Visibility == EVisibility.Expanded) ? ExpandedMenuYPos : DockedMenuYPos);
			base.gameObject.SetActive(visibility != EVisibility.Hidden);
		}

		public void UpdateSendables()
		{
			for (int i = 0; i < sendableBubbles.Count; i++)
			{
				SendableMessage sendableMessage = sendableMap[sendableBubbles[i]];
				string invalidReason;
				if (!sendableMessage.ShouldShow())
				{
					sendableBubbles[i].gameObject.SetActive(value: false);
				}
				else if (sendableMessage.IsValid(out invalidReason))
				{
					sendableBubbles[i].button.interactable = true;
					sendableBubbles[i].gameObject.SetActive(value: true);
				}
				else
				{
					sendableBubbles[i].button.interactable = false;
					sendableBubbles[i].gameObject.SetActive(value: false);
				}
			}
		}

		public void AddSendable(SendableMessage sendable)
		{
			MessageBubble component = Object.Instantiate(PlayerSingleton<MessagesApp>.Instance.messageBubblePrefab, SendablesContainer).GetComponent<MessageBubble>();
			component.SetupBubble(sendable.Text, MessageBubble.Alignment.Center, alignCenter: true);
			component.button.onClick.AddListener(delegate
			{
				SendableSelected(sendable);
			});
			sendableBubbles.Add(component);
			sendableMap.Add(component, sendable);
			UpdateSendables();
		}

		protected virtual void SendableSelected(SendableMessage sendable)
		{
			sendable.Send(network: true);
			SetVisibility(EVisibility.Hidden);
		}
	}
}
