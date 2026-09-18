using System;

namespace ScheduleOne.Persistence.Datas
{
	[Serializable]
	public class MSGConversationData : SaveData
	{
		public int ConversationIndex;

		public bool Read;

		public TextMessageData[] MessageHistory;

		public TextResponseData[] ActiveResponses;

		public bool IsHidden;

		public MSGConversationData(int conversationIndex, bool read, TextMessageData[] messageHistory, TextResponseData[] activeResponses, bool isHidden)
		{
			ConversationIndex = conversationIndex;
			Read = read;
			MessageHistory = messageHistory;
			ActiveResponses = activeResponses;
			IsHidden = isHidden;
		}

		public MSGConversationData()
		{
			ConversationIndex = 0;
			Read = false;
			MessageHistory = new TextMessageData[0];
			ActiveResponses = new TextResponseData[0];
			IsHidden = false;
		}
	}
}
