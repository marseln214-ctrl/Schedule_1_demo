using System;
using ScheduleOne.DevUtilities;
using ScheduleOne.ScriptableObjects;
using ScheduleOne.UI.Phone;

namespace ScheduleOne.Calling
{
	public class CallManager : Singleton<CallManager>
	{
		public PhoneCallData QueuedCallData { get; private set; }

		protected override void Start()
		{
			base.Start();
			CallInterface callInterface = Singleton<CallInterface>.Instance;
			callInterface.CallCompleted = (Action<PhoneCallData>)Delegate.Combine(callInterface.CallCompleted, new Action<PhoneCallData>(CallCompleted));
		}

		public void QueueCall(PhoneCallData data)
		{
			QueuedCallData = data;
		}

		public void ClearQueuedCall()
		{
			QueuedCallData = null;
		}

		private void CallCompleted(PhoneCallData call)
		{
			if (call == QueuedCallData)
			{
				ClearQueuedCall();
			}
		}
	}
}
