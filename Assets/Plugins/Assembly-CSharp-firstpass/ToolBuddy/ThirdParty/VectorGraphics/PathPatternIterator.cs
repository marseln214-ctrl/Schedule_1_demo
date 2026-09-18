using UnityEngine;

namespace ToolBuddy.ThirdParty.VectorGraphics
{
	internal class PathPatternIterator
	{
		private float[] pattern;

		private int currentSegment;

		private bool solid = true;

		private float segmentLength;

		private float patternLength;

		private float patternOffset;

		public float SegmentLength => segmentLength;

		public bool IsSolid => solid;

		public PathPatternIterator(float[] pattern, float patternOffset = 0f)
		{
			if (pattern != null)
			{
				foreach (float num in pattern)
				{
					patternLength += num;
				}
			}
			if (patternLength < VectorUtils.Epsilon)
			{
				segmentLength = float.MaxValue;
				return;
			}
			this.pattern = pattern;
			this.patternOffset = patternOffset;
			if (patternOffset == 0f)
			{
				segmentLength = pattern[0];
			}
			else
			{
				solid = IsSolidAt(0f, out currentSegment, out segmentLength);
			}
		}

		public void Advance()
		{
			if (pattern != null)
			{
				currentSegment++;
				if (currentSegment >= pattern.Length)
				{
					currentSegment = 0;
				}
				solid = !solid;
				segmentLength = pattern[currentSegment];
			}
		}

		public bool IsSolidAt(float unitsFromPathStart)
		{
			int patternSegmentIndex;
			float patternSegmentLength;
			return IsSolidAt(unitsFromPathStart, out patternSegmentIndex, out patternSegmentLength);
		}

		public bool IsSolidAt(float unitsFromPathStart, out int patternSegmentIndex, out float patternSegmentLength)
		{
			patternSegmentIndex = 0;
			patternSegmentLength = 0f;
			if (pattern == null)
			{
				return true;
			}
			bool flag = true;
			unitsFromPathStart += patternOffset;
			int num = (int)(Mathf.Abs(unitsFromPathStart) / patternLength);
			if (unitsFromPathStart < 0f)
			{
				unitsFromPathStart = patternLength - (0f - unitsFromPathStart) % patternLength;
				if ((pattern.Length & 1) == 1)
				{
					flag = (num & 1) == 0;
				}
			}
			else
			{
				unitsFromPathStart %= patternLength;
				if ((pattern.Length & 1) == 1)
				{
					flag = (num & 1) == 1;
				}
			}
			while (unitsFromPathStart > pattern[patternSegmentIndex])
			{
				unitsFromPathStart -= pattern[patternSegmentIndex++];
				flag = !flag;
			}
			patternSegmentLength = pattern[patternSegmentIndex] - unitsFromPathStart;
			return flag;
		}
	}
}
