using System.Runtime.CompilerServices;

namespace FluffyUnderware.DevTools
{
	public static class Environment
	{
		public static bool IsThreadingSupported
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				return true;
			}
		}
	}
}
