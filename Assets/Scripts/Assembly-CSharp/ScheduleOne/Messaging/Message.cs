using System;
using ScheduleOne.Persistence.Datas;
using UnityEngine;

namespace ScheduleOne.Messaging
{
	[Serializable]
	public class Message
	{
		public enum ESenderType
		{
			Player = 0,
			Other = 1
		}

		public int messageId = -1;

		public string text;

		public ESenderType sender;

		public bool endOfGroup;

		public Message()
		{
		}

		public Message(string _text, ESenderType _type, bool _endOfGroup = false, int _messageId = -1)
		{
			text = _text;
			sender = _type;
			endOfGroup = _endOfGroup;
			if (_messageId == -1)
			{
				messageId = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
			}
		}

		public Message(TextMessageData data)
		{
			text = data.Text;
			sender = (ESenderType)data.Sender;
			endOfGroup = data.EndOfChain;
			messageId = data.MessageID;
		}

		public TextMessageData GetSaveData()
		{
			return new TextMessageData((int)sender, messageId, text, endOfGroup);
		}
	}
}
