using System;

namespace FluffyUnderware.Curvy.Generator
{
	[AttributeUsage(AttributeTargets.Field)]
	public class ShapeOutputSlotInfo : OutputSlotInfo
	{
		public bool OutputsVariableShape;

		public ShapeOutputSlotInfo()
			: this(null)
		{
		}

		public ShapeOutputSlotInfo(string name)
			: base(name, typeof(CGShape))
		{
		}
	}
}
