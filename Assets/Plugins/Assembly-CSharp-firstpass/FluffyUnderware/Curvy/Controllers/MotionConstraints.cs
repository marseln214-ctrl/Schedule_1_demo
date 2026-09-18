using System;

namespace FluffyUnderware.Curvy.Controllers
{
	[Flags]
	public enum MotionConstraints
	{
		None = 0,
		FreezePositionX = 1,
		FreezePositionY = 2,
		FreezePositionZ = 4,
		FreezeRotationX = 8,
		FreezeRotationY = 0x10,
		FreezeRotationZ = 0x20
	}
}
