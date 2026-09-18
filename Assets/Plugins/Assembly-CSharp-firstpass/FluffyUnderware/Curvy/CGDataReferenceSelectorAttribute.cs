using System;
using FluffyUnderware.DevTools;
using JetBrains.Annotations;

namespace FluffyUnderware.Curvy
{
	[AttributeUsage(AttributeTargets.Field)]
	public sealed class CGDataReferenceSelectorAttribute : DTPropertyAttribute
	{
		[NotNull]
		public readonly Type DataType;

		public CGDataReferenceSelectorAttribute([NotNull] Type dataType)
		{
			DataType = dataType;
		}
	}
}
