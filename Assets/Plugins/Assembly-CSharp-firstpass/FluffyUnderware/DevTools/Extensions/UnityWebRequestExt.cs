using UnityEngine.Networking;

namespace FluffyUnderware.DevTools.Extensions
{
	public static class UnityWebRequestExt
	{
		public static bool IsError(this UnityWebRequest webRequest)
		{
			if (webRequest.result != UnityWebRequest.Result.ConnectionError)
			{
				return webRequest.result == UnityWebRequest.Result.ProtocolError;
			}
			return true;
		}
	}
}
