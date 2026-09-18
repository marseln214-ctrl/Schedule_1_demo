using ScheduleOne.Calling;
using ScheduleOne.DevUtilities;
using ScheduleOne.ScriptableObjects;
using UnityEngine;

namespace ScheduleOne.Tools
{
	public class PhoneCallUtility : MonoBehaviour
	{
		public void PromptCall(PhoneCallData callData)
		{
			Singleton<CallManager>.Instance.QueueCall(callData);
		}

		public void StartCall(PhoneCallData callData)
		{
			Singleton<CallManager>.Instance.QueueCall(callData);
		}

		public void SetQueuedCall(PhoneCallData callData)
		{
			Singleton<CallManager>.Instance.QueueCall(callData);
		}

		public void ClearCall()
		{
			Singleton<CallManager>.Instance.ClearQueuedCall();
		}

		public void SetPhoneOpenable(bool openable)
		{
		}
	}
}
