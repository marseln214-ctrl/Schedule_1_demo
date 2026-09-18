using System;

namespace FluffyUnderware.Curvy
{
	[Flags]
	public enum CurvyBezierModeEnum
	{
		None = 0,
		Direction = 1,
		Length = 2,
		Connections = 4,
		Combine = 8
	}
}
