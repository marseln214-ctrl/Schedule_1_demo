using System;
using FluffyUnderware.Curvy.Pools;
using FluffyUnderware.Curvy.Utils;
using JetBrains.Annotations;
using ToolBuddy.Pooling.Collections;
using UnityEngine;

namespace FluffyUnderware.Curvy.Generator
{
	[CGDataInfo(0.13f, 0.59f, 0.95f, 1f)]
	public class CGPath : CGShape
	{
		private SubArray<Vector3> directions;

		public SubArray<Vector3> Directions
		{
			get
			{
				return directions;
			}
			set
			{
				ArrayPools.Vector3.Free(directions);
				directions = value;
			}
		}

		[UsedImplicitly]
		[Obsolete("Use Directions instead")]
		public Vector3[] Direction
		{
			get
			{
				return Directions.CopyToArray(ArrayPools.Vector3);
			}
			set
			{
				Directions = new SubArray<Vector3>(value);
			}
		}

		public CGPath()
		{
			directions = ArrayPools.Vector3.Allocate(0);
		}

		public CGPath(CGPath source)
			: base(source)
		{
			directions = ArrayPools.Vector3.Clone(source.directions);
		}

		protected override bool Dispose(bool disposing)
		{
			bool num = base.Dispose(disposing);
			if (num)
			{
				ArrayPools.Vector3.Free(directions);
			}
			return num;
		}

		public override T Clone<T>()
		{
			return new CGPath(this) as T;
		}

		public static void Copy(CGPath dest, CGPath source)
		{
			CGShape.Copy(dest, source);
			ArrayPools.Vector3.Resize(ref dest.directions, source.directions.Count);
			Array.Copy(source.directions.Array, 0, dest.directions.Array, 0, source.directions.Count);
		}

		public void Interpolate(float f, out Vector3 position, out Vector3 direction, out Vector3 up)
		{
			float frag;
			int fIndex = GetFIndex(f, out frag);
			position = base.Positions.Array[fIndex].LerpUnclamped(base.Positions.Array[fIndex + 1], frag);
			direction = Vector3.SlerpUnclamped(directions.Array[fIndex], directions.Array[fIndex + 1], frag);
			up = Vector3.SlerpUnclamped(base.Normals.Array[fIndex], base.Normals.Array[fIndex + 1], frag);
		}

		[UsedImplicitly]
		[Obsolete("Method is no more used by Curvy and will get removed. Copy its content if you still need it")]
		public void Interpolate(float f, float angleF, out Vector3 pos, out Vector3 dir, out Vector3 up)
		{
			Interpolate(f, out pos, out dir, out up);
			if (angleF != 0f)
			{
				Quaternion quaternion = Quaternion.AngleAxis(angleF * -360f, dir);
				up = quaternion * up;
			}
		}

		public Vector3 InterpolateDirection(float f)
		{
			float frag;
			int fIndex = GetFIndex(f, out frag);
			return Vector3.SlerpUnclamped(directions.Array[fIndex], directions.Array[fIndex + 1], frag);
		}
	}
}
