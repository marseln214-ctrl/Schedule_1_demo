using System.Threading;

namespace FluffyUnderware.DevTools
{
	internal class QueuedCallback
	{
		public WaitCallback Callback;

		public object State;
	}
}
