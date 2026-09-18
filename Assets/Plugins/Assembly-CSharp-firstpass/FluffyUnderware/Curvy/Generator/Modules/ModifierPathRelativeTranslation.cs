using System;
using FluffyUnderware.DevTools;
using FluffyUnderware.DevTools.Extensions;
using FluffyUnderware.DevTools.Threading;
using UnityEngine;

namespace FluffyUnderware.Curvy.Generator.Modules
{
	[ModuleInfo("Modifier/Path Relative Translation", ModuleName = "Path Relative Translation", Description = "Translates a path relatively to it's direction, instead of relatively to the world as does the TRS Path module.")]
	[HelpURL("https://curvyeditor.com/doclink/cgpathrelativetranslation")]
	public class ModifierPathRelativeTranslation : CGModule, IOnRequestProcessing, IPathProvider
	{
		[HideInInspector]
		[InputSlotInfo(new Type[] { typeof(CGPath) }, Name = "Path A", ModifiesData = true)]
		public CGModuleInputSlot InPath = new CGModuleInputSlot();

		[HideInInspector]
		[OutputSlotInfo(typeof(CGPath))]
		public CGModuleOutputSlot OutPath = new CGModuleOutputSlot();

		[SerializeField]
		[Label("Translation", "")]
		[Tooltip("The (base) translation distance")]
		private float lateralTranslation;

		[SerializeField]
		[Tooltip("Defines translation multiplier, depending on the Relative Distance (between 0 and 1) of a point on the path")]
		[AnimationCurveEx("    Multiplier", "")]
		private AnimationCurve multiplier = AnimationCurve.Linear(0f, 1f, 1f, 1f);

		[SerializeField]
		[Tooltip("The translation angle, in degrees")]
		private float angle;

		public float LateralTranslation
		{
			get
			{
				return lateralTranslation;
			}
			set
			{
				if (lateralTranslation != value)
				{
					lateralTranslation = value;
					base.Dirty = true;
				}
			}
		}

		public float Angle
		{
			get
			{
				return angle;
			}
			set
			{
				if (float.IsNaN(value))
				{
					value = 0f;
				}
				if (angle != value)
				{
					angle = value;
					base.Dirty = true;
				}
			}
		}

		public AnimationCurve Multiplier
		{
			get
			{
				return multiplier;
			}
			set
			{
				if (multiplier != value)
				{
					multiplier = value;
					base.Dirty = true;
				}
			}
		}

		public bool PathIsClosed
		{
			get
			{
				if (IsConfigured)
				{
					return InPath.SourceSlot().PathProvider.PathIsClosed;
				}
				return false;
			}
		}

		public CGData[] OnSlotDataRequest(CGModuleInputSlot requestedBy, CGModuleOutputSlot requestedSlot, params CGDataRequestParameter[] requests)
		{
			if (requestedSlot != OutPath)
			{
				return Array.Empty<CGData>();
			}
			bool isDataDisposable;
			CGPath data = InPath.GetData<CGPath>(out isDataDisposable, requests);
			if (data == null)
			{
				return Array.Empty<CGData>();
			}
			if (!Multiplier.ValueIsOne())
			{
				for (int j = 0; j < data.Count; j++)
				{
					TranslatePoint(j, data, evaluateTranslationMultiplier: true, lateralTranslation, multiplier, angle);
				}
			}
			else
			{
				Parallel.For(0, data.Count, delegate(int i)
				{
					TranslatePoint(i, data, evaluateTranslationMultiplier: false, lateralTranslation, multiplier, angle);
				});
			}
			data.Recalculate();
			return new CGData[1] { data };
		}

		private static void TranslatePoint(int index, CGPath data, bool evaluateTranslationMultiplier, float translation, AnimationCurve translationMultiplier, float angle)
		{
			float num = ((!evaluateTranslationMultiplier) ? translation : (translation * translationMultiplier.Evaluate(data.RelativeDistances.Array[index])));
			Vector3 vector = data.Directions.Array[index];
			Vector3 lhs = data.Normals.Array[index];
			Vector3 vector2 = ((angle == 0f) ? (Vector3.Cross(lhs, vector) * num) : (Quaternion.AngleAxis(angle, vector) * Vector3.Cross(lhs, vector) * num));
			Vector3[] array = data.Positions.Array;
			array[index].x = array[index].x + vector2.x;
			array[index].y = array[index].y + vector2.y;
			array[index].z = array[index].z + vector2.z;
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			Properties.MinWidth = 250f;
			Properties.LabelWidth = 165f;
		}

		public override void Reset()
		{
			base.Reset();
			LateralTranslation = 0f;
			Angle = 0f;
			Multiplier = AnimationCurve.Linear(0f, 1f, 1f, 1f);
		}

		protected override void OnValidate()
		{
			base.OnValidate();
			Angle = angle;
		}
	}
}
