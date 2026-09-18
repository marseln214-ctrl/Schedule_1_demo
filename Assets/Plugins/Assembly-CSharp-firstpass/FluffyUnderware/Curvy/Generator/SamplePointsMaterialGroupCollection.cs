using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace FluffyUnderware.Curvy.Generator
{
	public class SamplePointsMaterialGroupCollection : List<SamplePointsMaterialGroup>
	{
		public int MaterialID;

		public float AspectCorrectionU = 1f;

		public float AspectCorrectionV = 1f;

		public int TriangleCount
		{
			get
			{
				int num = 0;
				for (int i = 0; i < base.Count; i++)
				{
					num += base[i].TriangleCount;
				}
				return num;
			}
		}

		[UsedImplicitly]
		[Obsolete("Use AspectCorrectionV instead")]
		public float AspectCorrection
		{
			get
			{
				return AspectCorrectionV;
			}
			set
			{
				AspectCorrectionV = value;
			}
		}

		public SamplePointsMaterialGroupCollection()
		{
		}

		public SamplePointsMaterialGroupCollection(int capacity)
			: base(capacity)
		{
		}

		public SamplePointsMaterialGroupCollection(IEnumerable<SamplePointsMaterialGroup> collection)
			: base(collection)
		{
		}

		public void CalculateAspectCorrection(CGVolume volume, CGMaterialSettingsEx matSettings)
		{
			switch (matSettings.KeepAspect)
			{
			case CGKeepAspectMode.Off:
				AspectCorrectionV = 1f;
				AspectCorrectionU = 1f;
				break;
			case CGKeepAspectMode.ScaleU:
			case CGKeepAspectMode.ScaleV:
			{
				float num = 0f;
				float num2 = 0f;
				for (int i = 0; i < base.Count; i++)
				{
					base[i].GetLengths(volume, out var worldLength, out var uLength);
					num += worldLength;
					num2 += uLength;
				}
				if (matSettings.KeepAspect == CGKeepAspectMode.ScaleU)
				{
					AspectCorrectionV = 1f;
					AspectCorrectionU = num / volume.Length;
				}
				else
				{
					AspectCorrectionV = volume.Length * num2 / num;
					AspectCorrectionU = 1f;
				}
				break;
			}
			default:
				throw new ArgumentOutOfRangeException();
			}
		}
	}
}
