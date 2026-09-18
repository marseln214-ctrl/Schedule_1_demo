using UnityEngine;

namespace FluffyUnderware.Curvy.Generator.Modules
{
	public class ScaleParameters
	{
		public readonly ScaleMode ScaleMode;

		public readonly CGReferenceMode ScaleReference;

		public readonly bool ScaleUniform;

		public readonly float ScaleOffset;

		public readonly float ScaleX;

		public readonly float ScaleY;

		public readonly AnimationCurve ScaleMultiplierX;

		public readonly AnimationCurve ScaleMultiplierY;

		public ScaleParameters(ScaleMode scaleMode, CGReferenceMode scaleReference, bool scaleUniform, float scaleOffset, float scaleX, float scaleY, AnimationCurve scaleMultiplierX, AnimationCurve scaleMultiplierY)
		{
			ScaleMode = scaleMode;
			ScaleReference = scaleReference;
			ScaleUniform = scaleUniform;
			ScaleOffset = scaleOffset;
			ScaleX = scaleX;
			ScaleY = scaleY;
			ScaleMultiplierX = scaleMultiplierX;
			ScaleMultiplierY = scaleMultiplierY;
		}
	}
}
