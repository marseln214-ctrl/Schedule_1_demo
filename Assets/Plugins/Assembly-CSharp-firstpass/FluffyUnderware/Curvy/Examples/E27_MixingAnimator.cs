using FluffyUnderware.Curvy.Generator.Modules;
using JetBrains.Annotations;
using UnityEngine;

namespace FluffyUnderware.Curvy.Examples
{
	public class E27_MixingAnimator : MonoBehaviour
	{
		public ModifierVariableMixShapes VariableMixShapes;

		[UsedImplicitly]
		private void Update()
		{
			Keyframe[] keys = VariableMixShapes.MixCurve.keys;
			keys[1].value = Mathf.Sin(Time.time);
			VariableMixShapes.MixCurve.keys = keys;
			VariableMixShapes.Dirty = true;
		}
	}
}
