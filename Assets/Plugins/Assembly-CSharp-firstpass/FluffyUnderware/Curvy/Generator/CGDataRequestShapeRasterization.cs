using System;
using FluffyUnderware.Curvy.Pools;
using JetBrains.Annotations;
using ToolBuddy.Pooling.Collections;

namespace FluffyUnderware.Curvy.Generator
{
	public class CGDataRequestShapeRasterization : CGDataRequestRasterization
	{
		private SubArray<float> relativeDistances;

		public SubArray<float> RelativeDistances
		{
			get
			{
				return relativeDistances;
			}
			set
			{
				relativeDistances = value;
			}
		}

		[UsedImplicitly]
		[Obsolete("Use RelativeDistances instead")]
		public float[] PathF
		{
			get
			{
				return RelativeDistances.CopyToArray(ArrayPools.Single);
			}
			set
			{
				RelativeDistances = new SubArray<float>(value);
			}
		}

		public CGDataRequestShapeRasterization(SubArray<float> relativeDistance, float start, float rasterizedRelativeLength, int resolution, float angle, ModeEnum mode = ModeEnum.Even)
			: base(start, rasterizedRelativeLength, resolution, angle, mode)
		{
			relativeDistances = ArrayPools.Single.Clone(relativeDistance);
		}

		[UsedImplicitly]
		[Obsolete("Use another constructor instead")]
		public CGDataRequestShapeRasterization(float[] pathF, float start, float rasterizedRelativeLength, int resolution, float angle, ModeEnum mode = ModeEnum.Even)
			: base(start, rasterizedRelativeLength, resolution, angle, mode)
		{
			relativeDistances = ArrayPools.Single.Clone(pathF);
		}

		public override bool Equals(object obj)
		{
			if (!(obj is CGDataRequestShapeRasterization cGDataRequestShapeRasterization))
			{
				return false;
			}
			if (!base.Equals(obj) || cGDataRequestShapeRasterization.relativeDistances.Count != relativeDistances.Count)
			{
				return false;
			}
			for (int i = 0; i < relativeDistances.Count; i++)
			{
				if (!cGDataRequestShapeRasterization.relativeDistances.Array[i].Equals(relativeDistances.Array[i]))
				{
					return false;
				}
			}
			return true;
		}

		public override int GetHashCode()
		{
			int num = base.GetHashCode() * 397;
			_ = relativeDistances;
			return num ^ relativeDistances.GetHashCode();
		}

		public override string ToString()
		{
			return string.Format("{0}, {1}: {2}", base.ToString(), "RelativeDistances", relativeDistances);
		}
	}
}
