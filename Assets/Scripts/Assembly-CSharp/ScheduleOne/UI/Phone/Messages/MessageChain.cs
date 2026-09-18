using System;
using System.Collections.Generic;
using UnityEngine;

namespace ScheduleOne.UI.Phone.Messages
{
	[Serializable]
	public class MessageChain
	{
		[TextArea(2, 10)]
		public List<string> Messages = new List<string>();

		[HideInInspector]
		public int id = -1;

		public static MessageChain Combine(MessageChain a, MessageChain b)
		{
			MessageChain messageChain = new MessageChain();
			messageChain.Messages.AddRange(a.Messages);
			messageChain.Messages.AddRange(b.Messages);
			return messageChain;
		}
	}
}
