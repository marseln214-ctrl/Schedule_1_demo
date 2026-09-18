using System;
using UnityEngine;

namespace FluffyUnderware.Curvy.ThirdParty.LibTessDotNet
{
	[Obsolete("No more used in Curvy. Will get removed. Copy it if you still need it")]
	public static class LibTessV3Extension
	{
		public static Vector3 Vector3(this Vec3 v)
		{
			return new Vector3(v.X, v.Y, v.Z);
		}
	}
}
