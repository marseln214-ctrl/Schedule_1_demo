using System;
using System.Collections.Generic;
using System.Linq;
using ScheduleOne.Audio;
using ScheduleOne.DevUtilities;
using ScheduleOne.Messaging;
using ScheduleOne.Persistence;
using UnityEngine;
using UnityEngine.UI;

namespace ScheduleOne.UI.Phone.Messages
{
	public class MessagesApp : App<MessagesApp>
	{
		[Serializable]
		public class CategoryInfo
		{
			public EConversationCategory Category;

			public string Name;

			public Color Color;
		}

		public static List<MSGConversation> Conversations = new List<MSGConversation>();

		public static List<MSGConversation> ActiveConversations = new List<MSGConversation>();

		public List<CategoryInfo> categoryInfos;

		[Header("References")]
		[SerializeField]
		protected RectTransform conversationEntryContainer;

		[SerializeField]
		protected RectTransform conversationContainer;

		public GameObject homePage;

		public GameObject dialoguePage;

		public Text dialoguePageNameText;

		public RectTransform relationshipContainer;

		public Scrollbar relationshipScrollbar;

		public RectTransform iconContainerRect;

		public Image iconImage;

		public Sprite BlankAvatarSprite;

		public DealWindowSelector DealWindowSelector;

		public PhoneShopInterface PhoneShopInterface;

		public CounterofferInterface CounterofferInterface;

		public RectTransform ClearFilterButton;

		public Button[] CategoryButtons;

		public AudioSourceController MessageReceivedSound;

		public AudioSourceController MessageSentSound;

		[Header("Prefabs")]
		[SerializeField]
		protected GameObject conversationEntryPrefab;

		[SerializeField]
		protected GameObject conversationContainerPrefab;

		public GameObject messageBubblePrefab;

		public List<MSGConversation> unreadConversations = new List<MSGConversation>();

		public MSGConversation currentConversation { get; private set; }

		protected override void Start()
		{
			base.Start();
			Singleton<LoadManager>.Instance.onLoadComplete.RemoveListener(Loaded);
			Singleton<LoadManager>.Instance.onLoadComplete.AddListener(Loaded);
			Singleton<LoadManager>.Instance.onPreSceneChange.RemoveListener(Clean);
			Singleton<LoadManager>.Instance.onPreSceneChange.AddListener(Clean);
			dialoguePage.gameObject.SetActive(value: false);
		}

		protected override void Update()
		{
			base.Update();
		}

		private void Loaded()
		{
			ActiveConversations = ActiveConversations.OrderBy((MSGConversation x) => x.index).ToList();
			RepositionEntries();
		}

		private void Clean()
		{
			Conversations.Clear();
			ActiveConversations.Clear();
		}

		public void CreateConversationUI(MSGConversation c, out RectTransform entry, out RectTransform container)
		{
			entry = UnityEngine.Object.Instantiate(conversationEntryPrefab, conversationEntryContainer).GetComponent<RectTransform>();
			entry.Find("Name").GetComponent<Text>().text = (c.IsSenderKnown ? c.contactName : "Unknown");
			entry.Find("IconMask/Icon").GetComponent<Image>().sprite = (c.IsSenderKnown ? c.sender.MugshotSprite : BlankAvatarSprite);
			entry.SetAsLastSibling();
			if (c.Categories != null && c.Categories.Count > 0)
			{
				CategoryInfo categoryInfo = GetCategoryInfo(c.Categories[0]);
				RectTransform component = entry.Find("Category").GetComponent<RectTransform>();
				Text component2 = component.Find("Label").GetComponent<Text>();
				component2.text = categoryInfo.Name[0].ToString();
				LayoutRebuilder.ForceRebuildLayoutImmediate(component2.rectTransform);
				component.GetComponent<Image>().color = categoryInfo.Color;
				component.anchoredPosition = new Vector2(225f + entry.Find("Name").GetComponent<Text>().preferredWidth, component.anchoredPosition.y);
				component.gameObject.SetActive(value: true);
			}
			else
			{
				entry.Find("Category").gameObject.SetActive(value: false);
			}
			container = UnityEngine.Object.Instantiate(conversationContainerPrefab, conversationContainer).GetComponent<RectTransform>();
			RepositionEntries();
		}

		public void RepositionEntries()
		{
			for (int i = 0; i < ActiveConversations.Count; i++)
			{
				ActiveConversations[i].RepositionEntry();
			}
			for (int j = 0; j < ActiveConversations.Count; j++)
			{
				ActiveConversations[j].RepositionEntry();
			}
		}

		public void ReturnButtonClicked()
		{
			if (currentConversation != null)
			{
				currentConversation.SetOpen(open: false);
			}
		}

		public void RefreshNotifications()
		{
			SetNotificationCount(unreadConversations.Count);
			Singleton<HUD>.Instance.UnreadMessagesPrompt.gameObject.SetActive(unreadConversations.Count > 0);
		}

		public override void Exit(ExitAction exit)
		{
			if (!base.isOpen || exit.used)
			{
				base.Exit(exit);
				return;
			}
			if (currentConversation != null)
			{
				currentConversation.SetOpen(open: false);
				exit.used = true;
			}
			base.Exit(exit);
		}

		public void SetCurrentConversation(MSGConversation conversation)
		{
			if (conversation != currentConversation)
			{
				MSGConversation mSGConversation = currentConversation;
				currentConversation = conversation;
				mSGConversation?.SetOpen(open: false);
			}
		}

		public CategoryInfo GetCategoryInfo(EConversationCategory category)
		{
			return categoryInfos.Find((CategoryInfo x) => x.Category == category);
		}

		public void FilterByCategory(int category)
		{
			for (int i = 0; i < CategoryButtons.Length; i++)
			{
				CategoryButtons[i].interactable = true;
			}
			for (int j = 0; j < ActiveConversations.Count; j++)
			{
				ActiveConversations[j].entry.gameObject.SetActive(ActiveConversations[j].Categories.Contains((EConversationCategory)category));
			}
			ClearFilterButton.gameObject.SetActive(value: true);
		}

		public void ClearFilter()
		{
			for (int i = 0; i < ActiveConversations.Count; i++)
			{
				ActiveConversations[i].entry.gameObject.SetActive(value: true);
			}
			for (int j = 0; j < CategoryButtons.Length; j++)
			{
				CategoryButtons[j].interactable = true;
			}
			ClearFilterButton.gameObject.SetActive(value: false);
		}
	}
}
