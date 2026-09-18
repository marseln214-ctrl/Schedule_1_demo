using System.Collections;

namespace FluffyUnderware.Curvy.Generator
{
	public class CGSpotComparer : IComparer
	{
		public int Compare(object x, object y)
		{
			return ((CGSpot)x).Index.CompareTo(((CGSpot)y).Index);
		}
	}
}
