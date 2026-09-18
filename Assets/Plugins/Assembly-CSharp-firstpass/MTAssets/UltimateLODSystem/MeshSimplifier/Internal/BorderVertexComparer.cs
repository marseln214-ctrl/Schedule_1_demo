using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace MTAssets.UltimateLODSystem.MeshSimplifier.Internal
{
	internal class BorderVertexComparer : IComparer<BorderVertex>
	{
		public static readonly BorderVertexComparer instance = new BorderVertexComparer();

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int Compare(BorderVertex x, BorderVertex y)
		{
			return x.hash.CompareTo(y.hash);
		}
	}
}
