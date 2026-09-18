using System;
using JetBrains.Annotations;

namespace FluffyUnderware.Curvy
{
	[AttributeUsage(AttributeTargets.Field)]
	public sealed class CGResourceCollectionManagerAttribute : CGResourceManagerAttribute
	{
		public bool ShowCount;

		public CGResourceCollectionManagerAttribute([NotNull] string resourceName)
			: base(resourceName)
		{
			ReadOnly = true;
		}
	}
}
