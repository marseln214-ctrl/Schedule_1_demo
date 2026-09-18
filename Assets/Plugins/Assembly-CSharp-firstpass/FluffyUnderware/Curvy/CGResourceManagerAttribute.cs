using System;
using FluffyUnderware.DevTools;
using JetBrains.Annotations;

namespace FluffyUnderware.Curvy
{
	[AttributeUsage(AttributeTargets.Field)]
	public class CGResourceManagerAttribute : DTPropertyAttribute
	{
		[NotNull]
		public readonly string ResourceName;

		public bool ReadOnly;

		public CGResourceManagerAttribute([NotNull] string resourceName)
		{
			ResourceName = resourceName;
		}
	}
}
