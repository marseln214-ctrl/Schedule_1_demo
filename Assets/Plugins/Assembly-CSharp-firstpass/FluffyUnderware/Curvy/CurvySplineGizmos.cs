using System;

namespace FluffyUnderware.Curvy
{
	[Flags]
	public enum CurvySplineGizmos
	{
		None = 0,
		Connections = 1,
		Curve = 2,
		Approximation = 4,
		Tangents = 8,
		Orientation = 0x10,
		Labels = 0x20,
		Metadata = 0x40,
		Bounds = 0x80,
		TFs = 0x100,
		RelativeDistances = 0x200,
		OrientationAnchors = 0x400,
		All = 0xFFFF
	}
}
