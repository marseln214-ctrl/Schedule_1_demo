using System;
using UnityEngine;

namespace AdvancedPeopleSystem
{
	[Serializable]
	public class BlendshapeEmotionValue
	{
		public CharacterBlendShapeType BlendType;

		[Range(-100f, 100f)]
		public float BlendValue;

		public AnimationCurve BlendAnimationCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 0f));
	}
}
