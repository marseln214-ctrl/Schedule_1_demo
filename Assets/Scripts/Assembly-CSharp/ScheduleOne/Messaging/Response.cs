using System;
using FishNet.Serializing.Helping;

namespace ScheduleOne.Messaging
{
	[Serializable]
	public class Response
	{
		public string text;

		public string label;

		[CodegenExclude]
		public Action callback;

		public bool disableDefaultResponseBehaviour;

		public Response(string _text, string _label, Action _callback = null, bool _disableDefaultResponseBehaviour = false)
		{
			text = _text;
			label = _label;
			callback = _callback;
			disableDefaultResponseBehaviour = _disableDefaultResponseBehaviour;
		}

		public Response()
		{
		}
	}
}
