using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ScheduleOne.DevUtilities;
using ScheduleOne.NPCs;
using ScheduleOne.Persistence;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.Persistence.Loaders;
using ScheduleOne.UI;
using ScheduleOne.UI.Phone;
using ScheduleOne.UI.Phone.Messages;
using UnityEngine;
using UnityEngine.UI;

namespace ScheduleOne.Messaging
{
	[Serializable]
	public class MSGConversation : ISaveable
	{
		public const int MAX_MESSAGE_HISTORY = 10;

		public string contactName = string.Empty;

		public NPC sender;

		public List<Message> messageHistory = new List<Message>();

		public List<MessageChain> messageChainHistory = new List<MessageChain>();

		public List<MessageBubble> bubbles = new List<MessageBubble>();

		public List<SendableMessage> Sendables = new List<SendableMessage>();

		public bool read = true;

		public List<EConversationCategory> Categories = new List<EConversationCategory>();

		public RectTransform entry;

		protected RectTransform container;

		protected RectTransform bubbleContainer;

		protected RectTransform scrollRectContainer;

		protected ScrollRect scrollRect;

		protected Text entryPreviewText;

		protected RectTransform unreadDot;

		protected Slider slider;

		protected Image sliderFill;

		protected RectTransform responseContainer;

		protected MessageSenderInterface senderInterface;

		private bool uiCreated;

		public Action onMessageRendered;

		public Action onLoaded;

		public Action onResponsesShown;

		public List<Response> currentResponses = new List<Response>();

		private List<RectTransform> responseRects = new List<RectTransform>();

		public bool IsSenderKnown { get; protected set; } = true;


		public int index { get; protected set; }

		public bool isOpen { get; protected set; }

		public bool rollingOut { get; protected set; }

		public bool EntryVisible { get; protected set; } = true;


		public bool AreResponsesActive => currentResponses.Count > 0;

		public string SaveFolderName => "MessageConversation";

		public string SaveFileName => "MessageConversation";

		public Loader Loader => null;

		public bool ShouldSaveUnderFolder => false;

		public List<string> LocalExtraFiles { get; set; } = new List<string>();


		public List<string> LocalExtraFolders { get; set; } = new List<string>();


		public bool HasChanged { get; set; }

		public MSGConversation(NPC _npc, string _contactName)
		{
			contactName = _contactName;
			sender = _npc;
			MessagesApp.Conversations.Insert(0, this);
			index = 0;
			NetworkSingleton<MessagingManager>.Instance.Register(_npc, this);
			InitializeSaveable();
		}

		public virtual void InitializeSaveable()
		{
			Singleton<SaveManager>.Instance.RegisterSaveable(this);
		}

		public void SetCategories(List<EConversationCategory> cat)
		{
			Categories = cat;
		}

		public void MoveToTop()
		{
			MessagesApp.ActiveConversations.Remove(this);
			MessagesApp.ActiveConversations.Insert(0, this);
			index = 0;
			PlayerSingleton<MessagesApp>.Instance.RepositionEntries();
		}

		protected void CreateUI()
		{
			if (uiCreated)
			{
				return;
			}
			uiCreated = true;
			PlayerSingleton<MessagesApp>.Instance.CreateConversationUI(this, out entry, out container);
			MessagesApp.ActiveConversations.Add(this);
			entryPreviewText = entry.Find("Preview").GetComponent<Text>();
			unreadDot = entry.Find("UnreadDot").GetComponent<RectTransform>();
			slider = entry.Find("Slider").GetComponent<Slider>();
			sliderFill = slider.fillRect.GetComponent<Image>();
			entry.Find("Button").GetComponent<Button>().onClick.AddListener(EntryClicked);
			Button component = entry.Find("Hide").GetComponent<Button>();
			if (sender.ConversationCanBeHidden)
			{
				component.gameObject.SetActive(value: true);
				component.onClick.AddListener(delegate
				{
					SetEntryVisibility(v: false);
				});
			}
			else
			{
				component.gameObject.SetActive(value: false);
			}
			scrollRectContainer = container.Find("ScrollContainer").GetComponent<RectTransform>();
			scrollRect = scrollRectContainer.Find("ScrollRect").GetComponent<ScrollRect>();
			bubbleContainer = scrollRect.transform.Find("Viewport/Content").GetComponent<RectTransform>();
			entryPreviewText.text = string.Empty;
			unreadDot.gameObject.SetActive(!read && messageHistory.Count > 0);
			responseContainer = container.Find("Responses").GetComponent<RectTransform>();
			senderInterface = container.Find("SenderInterface").GetComponent<MessageSenderInterface>();
			for (int i = 0; i < Sendables.Count; i++)
			{
				senderInterface.AddSendable(Sendables[i]);
			}
			RepositionEntry();
			SetResponseContainerVisible(v: false);
			SetOpen(open: false);
		}

		private void EnsureUIExists()
		{
			if (!uiCreated)
			{
				CreateUI();
			}
		}

		protected void RefreshPreviewText()
		{
			if (bubbles.Count == 0)
			{
				entryPreviewText.text = string.Empty;
			}
			else
			{
				entryPreviewText.text = bubbles[bubbles.Count - 1].text;
			}
		}

		public void RepositionEntry()
		{
			if (!(entry == null))
			{
				entry.SetSiblingIndex(MessagesApp.ActiveConversations.IndexOf(this));
			}
		}

		public void SetIsKnown(bool known)
		{
			IsSenderKnown = known;
			if (entry != null)
			{
				entry.Find("Name").GetComponent<Text>().text = (IsSenderKnown ? contactName : "Unknown");
				entry.Find("IconMask/Icon").GetComponent<Image>().sprite = (IsSenderKnown ? sender.MugshotSprite : PlayerSingleton<MessagesApp>.Instance.BlankAvatarSprite);
			}
		}

		public void EntryClicked()
		{
			SetOpen(open: true);
		}

		public void SetOpen(bool open)
		{
			isOpen = open;
			PlayerSingleton<MessagesApp>.Instance.homePage.gameObject.SetActive(!open);
			PlayerSingleton<MessagesApp>.Instance.dialoguePage.gameObject.SetActive(open);
			if (open)
			{
				PlayerSingleton<MessagesApp>.Instance.SetCurrentConversation(this);
				float y = 0f;
				if (sender.ShowRelationshipInfo)
				{
					y = 15f;
					PlayerSingleton<MessagesApp>.Instance.relationshipScrollbar.value = sender.RelationData.NormalizedRelationDelta;
				}
				PlayerSingleton<MessagesApp>.Instance.relationshipContainer.gameObject.SetActive(sender.ShowRelationshipInfo);
				PlayerSingleton<MessagesApp>.Instance.dialoguePageNameText.text = (IsSenderKnown ? contactName : "Unknown");
				PlayerSingleton<MessagesApp>.Instance.dialoguePageNameText.rectTransform.anchoredPosition = new Vector2((0f - PlayerSingleton<MessagesApp>.Instance.dialoguePageNameText.preferredWidth) / 2f + 30f, y);
				PlayerSingleton<MessagesApp>.Instance.iconContainerRect.anchoredPosition = new Vector2((0f - PlayerSingleton<MessagesApp>.Instance.dialoguePageNameText.preferredWidth) / 2f - 30f, PlayerSingleton<MessagesApp>.Instance.iconContainerRect.anchoredPosition.y);
				PlayerSingleton<MessagesApp>.Instance.iconImage.sprite = (IsSenderKnown ? sender.MugshotSprite : PlayerSingleton<MessagesApp>.Instance.BlankAvatarSprite);
				SetRead(r: true);
				CheckSendLoop();
			}
			else
			{
				PlayerSingleton<MessagesApp>.Instance.SetCurrentConversation(null);
			}
			container.gameObject.SetActive(open);
			SetResponseContainerVisible(AreResponsesActive);
		}

		protected virtual void RenderMessage(Message m)
		{
			MessageBubble component = UnityEngine.Object.Instantiate(PlayerSingleton<MessagesApp>.Instance.messageBubblePrefab, bubbleContainer).GetComponent<MessageBubble>();
			component.SetupBubble(m.text, (m.sender == Message.ESenderType.Other) ? MessageBubble.Alignment.Left : MessageBubble.Alignment.Right);
			float num = 0f;
			for (int i = 0; i < bubbles.Count; i++)
			{
				num += bubbles[i].height;
				num += bubbles[i].spacingAbove;
			}
			bool flag = false;
			if (messageHistory.IndexOf(m) > 0 && messageHistory[messageHistory.IndexOf(m) - 1].sender == m.sender)
			{
				flag = true;
			}
			float num2 = MessageBubble.baseBubbleSpacing;
			if (!flag)
			{
				num2 *= 10f;
			}
			if (flag && messageHistory[messageHistory.IndexOf(m) - 1].endOfGroup)
			{
				num2 *= 20f;
			}
			component.container.anchoredPosition = new Vector2(component.container.anchoredPosition.x, 0f - num - num2 - component.height / 2f);
			component.spacingAbove = num2;
			component.showTriangle = true;
			if (flag && !messageHistory[messageHistory.IndexOf(m) - 1].endOfGroup)
			{
				bubbles[bubbles.Count - 1].showTriangle = false;
			}
			bubbleContainer.sizeDelta = new Vector2(bubbleContainer.sizeDelta.x, num + component.height + num2 + MessageBubble.baseBubbleSpacing * 10f);
			scrollRect.verticalNormalizedPosition = 0f;
			bubbles.Add(component);
			if (m.sender == Message.ESenderType.Player && PlayerSingleton<MessagesApp>.Instance.isOpen && PlayerSingleton<Phone>.Instance.IsOpen)
			{
				PlayerSingleton<MessagesApp>.Instance.MessageSentSound.Play();
			}
			else if (PlayerSingleton<Phone>.Instance.IsOpen && PlayerSingleton<MessagesApp>.Instance.isOpen && (isOpen || PlayerSingleton<MessagesApp>.Instance.currentConversation == null))
			{
				PlayerSingleton<MessagesApp>.Instance.MessageReceivedSound.Play();
			}
			if (onMessageRendered != null)
			{
				onMessageRendered();
			}
		}

		public void SetEntryVisibility(bool v)
		{
			if (v || sender.ConversationCanBeHidden)
			{
				EntryVisible = v;
				entry.gameObject.SetActive(v);
				if (!v)
				{
					SetRead(r: true);
				}
				HasChanged = true;
			}
		}

		public void SetRead(bool r)
		{
			read = r;
			if (read)
			{
				if (PlayerSingleton<MessagesApp>.Instance.unreadConversations.Contains(this))
				{
					PlayerSingleton<MessagesApp>.Instance.unreadConversations.Remove(this);
					PlayerSingleton<MessagesApp>.Instance.RefreshNotifications();
				}
			}
			else if (!PlayerSingleton<MessagesApp>.Instance.unreadConversations.Contains(this))
			{
				PlayerSingleton<MessagesApp>.Instance.unreadConversations.Add(this);
				PlayerSingleton<MessagesApp>.Instance.RefreshNotifications();
			}
			if (unreadDot != null)
			{
				unreadDot.gameObject.SetActive(!read);
			}
			HasChanged = true;
		}

		public void SendMessage(Message message, bool notify = true, bool network = true)
		{
			EnsureUIExists();
			if (message.messageId == -1)
			{
				message.messageId = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
			}
			if (messageHistory.Find((Message x) => x.messageId == message.messageId) != null)
			{
				return;
			}
			if (network)
			{
				NetworkSingleton<MessagingManager>.Instance.SendMessage(message, notify, sender.ID);
				return;
			}
			messageHistory.Add(message);
			if (messageHistory.Count > 10)
			{
				messageHistory.RemoveAt(0);
			}
			if (message.sender == Message.ESenderType.Other && notify)
			{
				SetEntryVisibility(v: true);
				if (!isOpen)
				{
					SetRead(r: false);
				}
				if (!isOpen || !PlayerSingleton<MessagesApp>.Instance.isOpen || !PlayerSingleton<Phone>.Instance.IsOpen)
				{
					Singleton<NotificationsManager>.Instance.SendNotification(IsSenderKnown ? contactName : "Unknown", message.text, PlayerSingleton<MessagesApp>.Instance.AppIcon);
				}
			}
			RenderMessage(message);
			RefreshPreviewText();
			MoveToTop();
			HasChanged = true;
		}

		public void SendMessageChain(MessageChain messages, float initialDelay = 0f, bool notify = true, bool network = true)
		{
			EnsureUIExists();
			if (messages.id == -1)
			{
				messages.id = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
			}
			if (messageChainHistory.Find((MessageChain x) => x.id == messages.id) == null)
			{
				if (network)
				{
					NetworkSingleton<MessagingManager>.Instance.SendMessageChain(messages, sender.ID, initialDelay, notify);
					return;
				}
				messageChainHistory.Add(messages);
				HasChanged = true;
				Singleton<CoroutineService>.Instance.StartCoroutine(Routine(messages, initialDelay));
			}
			IEnumerator Routine(MessageChain messageChain, float initialDelay)
			{
				rollingOut = true;
				List<Message> messageClasses = new List<Message>();
				for (int j = 0; j < messageChain.Messages.Count; j++)
				{
					Message item = new Message(messageChain.Messages[j], Message.ESenderType.Other, j == messageChain.Messages.Count - 1);
					messageHistory.Add(item);
					if (messageHistory.Count > 10)
					{
						messageHistory.RemoveAt(0);
					}
					messageClasses.Add(item);
				}
				yield return new WaitForSeconds(initialDelay);
				if (notify && (!isOpen || !PlayerSingleton<MessagesApp>.Instance.isOpen || !PlayerSingleton<Phone>.Instance.IsOpen))
				{
					Singleton<NotificationsManager>.Instance.SendNotification(IsSenderKnown ? contactName : "Unknown", messageChain.Messages[0], PlayerSingleton<MessagesApp>.Instance.AppIcon);
				}
				for (int i = 0; i < messageClasses.Count; i++)
				{
					RenderMessage(messageClasses[i]);
					RefreshPreviewText();
					MoveToTop();
					if (!isOpen && notify)
					{
						SetEntryVisibility(v: true);
						SetRead(r: false);
					}
					if (i + 1 < messageClasses.Count && !Application.isEditor)
					{
						yield return new WaitForSeconds(1f);
					}
				}
				rollingOut = false;
			}
		}

		public MSGConversationData GetSaveData()
		{
			List<TextMessageData> list = new List<TextMessageData>();
			for (int i = 0; i < messageHistory.Count; i++)
			{
				list.Add(messageHistory[i].GetSaveData());
			}
			List<TextResponseData> list2 = new List<TextResponseData>();
			for (int j = 0; j < currentResponses.Count; j++)
			{
				list2.Add(new TextResponseData(currentResponses[j].text, currentResponses[j].label));
			}
			return new MSGConversationData(MessagesApp.ActiveConversations.IndexOf(this), read, list.ToArray(), list2.ToArray(), !EntryVisible);
		}

		public virtual string GetSaveString()
		{
			return GetSaveData().GetJson();
		}

		public virtual void Load(MSGConversationData data)
		{
			EnsureUIExists();
			index = data.ConversationIndex;
			SetRead(data.Read);
			if (data.MessageHistory != null)
			{
				for (int i = 0; i < data.MessageHistory.Length; i++)
				{
					Message message = new Message(data.MessageHistory[i]);
					messageHistory.Add(message);
					if (messageHistory.Count > 10)
					{
						messageHistory.RemoveAt(0);
					}
					RenderMessage(message);
				}
			}
			else
			{
				Console.LogWarning("Message history null!");
			}
			if (data.ActiveResponses != null)
			{
				List<Response> list = new List<Response>();
				for (int j = 0; j < data.ActiveResponses.Length; j++)
				{
					list.Add(new Response(data.ActiveResponses[j].Text, data.ActiveResponses[j].Label));
				}
				if (list.Count > 0)
				{
					ShowResponses(list);
				}
			}
			else
			{
				Console.LogWarning("Message reponses null!");
			}
			RefreshPreviewText();
			HasChanged = false;
			_ = data.IsHidden;
			if (data.IsHidden)
			{
				SetEntryVisibility(v: false);
			}
			if (onLoaded != null)
			{
				onLoaded();
			}
		}

		public void SetSliderValue(float value, Color color)
		{
			if (!(slider == null))
			{
				slider.value = value;
				sliderFill.color = color;
				slider.gameObject.SetActive(value > 0f);
			}
		}

		public Response GetResponse(string label)
		{
			return currentResponses.Find((Response x) => x.label == label);
		}

		public void ShowResponses(List<Response> _responses, float showResponseDelay = 0f, bool network = true)
		{
			if (network)
			{
				NetworkSingleton<MessagingManager>.Instance.ShowResponses(sender.ID, _responses, showResponseDelay);
				return;
			}
			EnsureUIExists();
			currentResponses = _responses;
			ClearResponseUI();
			for (int i = 0; i < _responses.Count; i++)
			{
				CreateResponseUI(_responses[i]);
			}
			if (showResponseDelay == 0f)
			{
				SetResponseContainerVisible(v: true);
			}
			else
			{
				Singleton<CoroutineService>.Instance.StartCoroutine(Routine());
			}
			HasChanged = true;
			if (onResponsesShown != null)
			{
				onResponsesShown();
			}
			IEnumerator Routine()
			{
				yield return new WaitForSeconds(showResponseDelay);
				SetResponseContainerVisible(v: true);
			}
		}

		protected void CreateResponseUI(Response r)
		{
			EnsureUIExists();
			MessageBubble component = UnityEngine.Object.Instantiate(PlayerSingleton<MessagesApp>.Instance.messageBubblePrefab, responseContainer).GetComponent<MessageBubble>();
			float num = 5f;
			float num2 = 25f;
			component.bubble_MinWidth = responseContainer.rect.width - num2 * 2f;
			component.bubble_MaxWidth = responseContainer.rect.width - num2 * 2f;
			component.SetupBubble(r.text, MessageBubble.Alignment.Center, alignCenter: true);
			float num3 = num2;
			for (int i = 0; i < responseRects.Count; i++)
			{
				num3 += responseRects[i].gameObject.GetComponent<MessageBubble>().height;
				num3 += num;
			}
			component.container.anchoredPosition = new Vector2(0f, 0f - num3 - 35f);
			responseRects.Add(component.container);
			component.button.interactable = true;
			bool network = !r.disableDefaultResponseBehaviour;
			component.button.onClick.AddListener(delegate
			{
				ResponseChosen(r, network);
			});
			responseContainer.sizeDelta = new Vector2(responseContainer.sizeDelta.x, num3 + component.height + num2);
			responseContainer.anchoredPosition = new Vector2(0f, responseContainer.sizeDelta.y / 2f);
		}

		protected void ClearResponseUI()
		{
			for (int i = 0; i < responseRects.Count; i++)
			{
				UnityEngine.Object.Destroy(responseRects[i].gameObject);
			}
			responseRects.Clear();
		}

		public void SetResponseContainerVisible(bool v)
		{
			if (v)
			{
				scrollRectContainer.offsetMin = new Vector2(0f, responseContainer.sizeDelta.y);
			}
			else
			{
				scrollRectContainer.offsetMin = new Vector2(0f, 0f);
			}
			responseContainer.gameObject.SetActive(v);
			bubbleContainer.anchoredPosition = new Vector2(bubbleContainer.anchoredPosition.x, Mathf.Clamp(bubbleContainer.anchoredPosition.y, 1100f, float.MaxValue));
		}

		public void ResponseChosen(Response r, bool network)
		{
			if (!AreResponsesActive)
			{
				return;
			}
			if (r.disableDefaultResponseBehaviour)
			{
				if (r.callback != null)
				{
					r.callback();
				}
				return;
			}
			if (network)
			{
				NetworkSingleton<MessagingManager>.Instance.SendResponse(currentResponses.IndexOf(r), sender.ID);
				return;
			}
			ClearResponses();
			RenderMessage(new Message(r.text, Message.ESenderType.Player, _endOfGroup: true));
			HasChanged = true;
			MoveToTop();
			if (r.callback != null)
			{
				r.callback();
			}
		}

		public void ClearResponses(bool network = false)
		{
			ClearResponseUI();
			SetResponseContainerVisible(v: false);
			currentResponses.Clear();
			if (network)
			{
				NetworkSingleton<MessagingManager>.Instance.ClearResponses(sender.ID);
			}
		}

		public SendableMessage CreateSendableMessage(string text)
		{
			SendableMessage sendableMessage = new SendableMessage(text, this);
			Sendables.Add(sendableMessage);
			if (uiCreated)
			{
				senderInterface.AddSendable(sendableMessage);
			}
			return sendableMessage;
		}

		public void SendPlayerMessage(int sendableIndex, int sentIndex, bool network)
		{
			if (network)
			{
				NetworkSingleton<MessagingManager>.Instance.SendPlayerMessage(sendableIndex, sentIndex, sender.ID);
			}
			else
			{
				Sendables[sendableIndex].Send(network: false, sentIndex);
			}
		}

		public void RenderPlayerMessage(SendableMessage sendable)
		{
			Message m = new Message(sendable.Text, Message.ESenderType.Player, _endOfGroup: true);
			RenderMessage(m);
		}

		private void CheckSendLoop()
		{
			CanSendNewMessage();
			PlayerSingleton<MessagesApp>.Instance.StartCoroutine(Loop());
			IEnumerator Loop()
			{
				while (isOpen)
				{
					if (CanSendNewMessage())
					{
						if (senderInterface.Visibility == MessageSenderInterface.EVisibility.Hidden)
						{
							senderInterface.SetVisibility(MessageSenderInterface.EVisibility.Docked);
						}
					}
					else if (senderInterface.Visibility != 0)
					{
						senderInterface.SetVisibility(MessageSenderInterface.EVisibility.Hidden);
					}
					scrollRect.GetComponent<RectTransform>().offsetMin = new Vector2(0f, (senderInterface.Visibility == MessageSenderInterface.EVisibility.Docked) ? 200f : 0f);
					yield return new WaitForEndOfFrame();
				}
				senderInterface.SetVisibility(MessageSenderInterface.EVisibility.Hidden);
				scrollRect.GetComponent<RectTransform>().offsetMin = new Vector2(0f, 0f);
			}
		}

		private bool CanSendNewMessage()
		{
			if (rollingOut)
			{
				return false;
			}
			if (AreResponsesActive)
			{
				return false;
			}
			if (Sendables.FirstOrDefault((SendableMessage x) => x.ShouldShow()) == null)
			{
				return false;
			}
			return true;
		}
	}
}
