using System;
using System.Collections.Generic;
using UnityEngine;

namespace ScheduleOne.Messaging
{
	public class SendableMessage
	{
		public delegate bool BoolCheck(SendableMessage message);

		public delegate bool ValidityCheck(SendableMessage message, out string invalidReason);

		public string Text;

		public BoolCheck ShouldShowCheck;

		public ValidityCheck IsValidCheck;

		public Action onSelected;

		public Action onSent;

		private MSGConversation conversation;

		public bool disableDefaultSendBehaviour;

		private List<int> sentIDs = new List<int>();

		public SendableMessage(string text, MSGConversation conversation)
		{
			Text = text;
			this.conversation = conversation;
		}

		public virtual bool ShouldShow()
		{
			if (ShouldShowCheck != null)
			{
				return ShouldShowCheck(this);
			}
			return true;
		}

		public virtual bool IsValid(out string invalidReason)
		{
			if (IsValidCheck != null)
			{
				return IsValidCheck(this, out invalidReason);
			}
			invalidReason = "";
			return true;
		}

		public virtual void Send(bool network, int id = -1)
		{
			if (id != -1)
			{
				if (sentIDs.Contains(id))
				{
					return;
				}
			}
			else
			{
				id = UnityEngine.Random.Range(0, int.MaxValue);
			}
			if (onSelected != null)
			{
				onSelected();
			}
			if (disableDefaultSendBehaviour)
			{
				return;
			}
			if (network)
			{
				conversation.SendPlayerMessage(conversation.Sendables.IndexOf(this), id, network: true);
				return;
			}
			sentIDs.Add(id);
			conversation.RenderPlayerMessage(this);
			if (onSent != null)
			{
				onSent();
			}
		}
	}
}
