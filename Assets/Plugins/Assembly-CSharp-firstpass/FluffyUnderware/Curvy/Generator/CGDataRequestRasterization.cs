using UnityEngine;

namespace FluffyUnderware.Curvy.Generator
{
	public class CGDataRequestRasterization : CGDataRequestParameter
	{
		public enum ModeEnum
		{
			Even = 0,
			Optimized = 1
		}

		public float Start;

		public float RasterizedRelativeLength;

		public int Resolution;

		public float AngleThreshold;

		public ModeEnum Mode;

		public CGDataRequestRasterization(float start, float rasterizedRelativeLength, int resolution, float angle, ModeEnum mode = ModeEnum.Even)
		{
			Start = Mathf.Repeat(start, 1f);
			RasterizedRelativeLength = Mathf.Clamp01(rasterizedRelativeLength);
			Resolution = resolution;
			AngleThreshold = angle;
			Mode = mode;
		}

		public CGDataRequestRasterization(CGDataRequestRasterization source)
			: this(source.Start, source.RasterizedRelativeLength, source.Resolution, source.AngleThreshold, source.Mode)
		{
		}

		public override bool Equals(object obj)
		{
			if (!(obj is CGDataRequestRasterization cGDataRequestRasterization))
			{
				return false;
			}
			if (Start == cGDataRequestRasterization.Start && RasterizedRelativeLength == cGDataRequestRasterization.RasterizedRelativeLength && Resolution == cGDataRequestRasterization.Resolution && AngleThreshold == cGDataRequestRasterization.AngleThreshold)
			{
				return Mode == cGDataRequestRasterization.Mode;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return new
			{
				A = Start,
				B = RasterizedRelativeLength,
				C = Resolution,
				D = AngleThreshold,
				E = Mode
			}.GetHashCode();
		}

		public override string ToString()
		{
			return string.Format("{0}: {1}, {2}: {3}, {4}: {5}, {6}: {7}, {8}: {9}", "Start", Start, "RasterizedRelativeLength", RasterizedRelativeLength, "Resolution", Resolution, "AngleThreshold", AngleThreshold, "Mode", Mode);
		}
	}
}
