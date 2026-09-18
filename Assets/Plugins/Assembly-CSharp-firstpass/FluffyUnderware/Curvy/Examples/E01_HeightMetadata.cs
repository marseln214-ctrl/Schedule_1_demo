using FluffyUnderware.DevTools;
using UnityEngine;

namespace FluffyUnderware.Curvy.Examples
{
	public class E01_HeightMetadata : CurvyInterpolatableMetadataBase<float>
	{
		[SerializeField]
		[RangeEx(0f, 1f, "", "", Slider = true)]
		private float m_Height;

		public override float MetaDataValue => m_Height;

		public override float Interpolate(CurvyInterpolatableMetadataBase<float> nextMetadata, float interpolationTime)
		{
			if (!(nextMetadata != null))
			{
				return MetaDataValue;
			}
			return Mathf.Lerp(MetaDataValue, nextMetadata.MetaDataValue, interpolationTime);
		}
	}
}
