using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace FluffyUnderware.DevTools.Threading
{
	public static class Parallel
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void For(int fromInclusive, int toExclusive, Action<int> body)
		{
			if (Environment.IsThreadingSupported)
			{
				System.Threading.Tasks.Parallel.For(fromInclusive, toExclusive, body);
				return;
			}
			for (int i = fromInclusive; i < toExclusive; i++)
			{
				body(i);
			}
		}
	}
}
