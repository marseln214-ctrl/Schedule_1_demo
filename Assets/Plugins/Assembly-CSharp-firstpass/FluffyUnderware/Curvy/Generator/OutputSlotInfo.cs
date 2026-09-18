using System;
using JetBrains.Annotations;

namespace FluffyUnderware.Curvy.Generator
{
	[AttributeUsage(AttributeTargets.Field)]
	public class OutputSlotInfo : SlotInfo
	{
		public Type DataType => DataTypes[0];

		public OutputSlotInfo([NotNull] Type type)
			: this(null, type)
		{
		}

		public OutputSlotInfo(string name, [NotNull] Type type)
			: base(name, type)
		{
		}
	}
}
