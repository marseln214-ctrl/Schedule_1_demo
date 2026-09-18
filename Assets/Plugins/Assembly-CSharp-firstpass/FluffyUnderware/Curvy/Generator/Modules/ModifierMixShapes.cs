using System;
using System.Collections.Generic;
using System.Linq;
using FluffyUnderware.Curvy.Pools;
using FluffyUnderware.DevTools;
using JetBrains.Annotations;
using ToolBuddy.Pooling.Collections;
using UnityEngine;

namespace FluffyUnderware.Curvy.Generator.Modules
{
	[ModuleInfo("Modifier/Mix Shapes", ModuleName = "Mix Shapes", Description = "Interpolates between two shapes")]
	[HelpURL("https://curvyeditor.com/doclink/cgmixshapes")]
	public class ModifierMixShapes : CGModule, IOnRequestProcessing, IPathProvider
	{
		private const int MixMinValue = -1;

		private const int MixMaxValue = 1;

		[HideInInspector]
		[InputSlotInfo(new Type[] { typeof(CGShape) }, Name = "Shape A")]
		public CGModuleInputSlot InShapeA = new CGModuleInputSlot();

		[HideInInspector]
		[InputSlotInfo(new Type[] { typeof(CGShape) }, Name = "Shape B")]
		public CGModuleInputSlot InShapeB = new CGModuleInputSlot();

		[HideInInspector]
		[OutputSlotInfo(typeof(CGShape))]
		public CGModuleOutputSlot OutShape = new CGModuleOutputSlot();

		[SerializeField]
		[RangeEx(-1f, 1f, "", "", Tooltip = "Mix between the shapes. Values between -1 for Shape A and 1 for Shape B")]
		private float m_Mix;

		public float Mix
		{
			get
			{
				return m_Mix;
			}
			set
			{
				float num = Mathf.Clamp(value, -1f, 1f);
				if (m_Mix != num)
				{
					m_Mix = num;
					base.Dirty = true;
				}
			}
		}

		public bool PathIsClosed
		{
			get
			{
				if (IsConfigured && InShapeA.SourceSlot().PathProvider.PathIsClosed)
				{
					return InShapeB.SourceSlot().PathProvider.PathIsClosed;
				}
				return false;
			}
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			Properties.MinWidth = 250f;
			Properties.LabelWidth = 50f;
		}

		public override void Reset()
		{
			base.Reset();
			Mix = 0f;
		}

		public CGData[] OnSlotDataRequest(CGModuleInputSlot requestedBy, CGModuleOutputSlot requestedSlot, params CGDataRequestParameter[] requests)
		{
			if (!CGModule.GetRequestParameter<CGDataRequestRasterization>(ref requests))
			{
				return Array.Empty<CGData>();
			}
			bool isDataDisposable;
			CGShape data = InShapeA.GetData<CGShape>(out isDataDisposable, requests);
			bool isDataDisposable2;
			CGShape data2 = InShapeB.GetData<CGShape>(out isDataDisposable2, requests);
			CGShape cGShape = MixShapes(data, data2, Mix, UIMessages);
			if (isDataDisposable)
			{
				data.Dispose();
			}
			if (isDataDisposable2)
			{
				data2.Dispose();
			}
			if (cGShape != null)
			{
				return new CGData[1] { cGShape };
			}
			return Array.Empty<CGData>();
		}

		[CanBeNull]
		public static CGShape MixShapes([CanBeNull] CGShape shapeA, [CanBeNull] CGShape shapeB, float mix, [NotNull] List<string> warningsContainer, bool ignoreWarnings = false)
		{
			if (shapeA == null)
			{
				return shapeB;
			}
			if (shapeB == null)
			{
				return shapeA;
			}
			CGShape cGShape = new CGShape();
			InterpolateShape(cGShape, shapeA, shapeB, mix, warningsContainer, ignoreWarnings);
			return cGShape;
		}

		public static void InterpolateShape([NotNull] CGShape resultShape, CGShape shapeA, CGShape shapeB, float mix, [NotNull] List<string> warningsContainer, bool ignoreWarnings = false)
		{
			float num = (mix + 1f) * 0.5f;
			int num2 = Mathf.Max(shapeA.Count, shapeB.Count);
			CGShape cGShape = ((shapeA.Count == num2) ? shapeA : shapeB);
			SubArray<Vector3> positions = ArrayPools.Vector3.Allocate(num2);
			SubArray<Vector3> normals = ArrayPools.Vector3.Allocate(num2);
			Vector3[] array = shapeB.Positions.Array;
			Vector3[] array2 = shapeA.Positions.Array;
			Vector3[] array3 = positions.Array;
			Vector3[] array4 = shapeA.Normals.Array;
			Vector3[] array5 = shapeB.Normals.Array;
			if (cGShape == shapeA)
			{
				Vector3 vector = default(Vector3);
				for (int i = 0; i < num2; i++)
				{
					float frag;
					int fIndex = shapeB.GetFIndex(shapeA.RelativeDistances.Array[i], out frag);
					vector.x = array[fIndex].x + (array[fIndex + 1].x - array[fIndex].x) * frag;
					vector.y = array[fIndex].y + (array[fIndex + 1].y - array[fIndex].y) * frag;
					vector.z = array[fIndex].z + (array[fIndex + 1].z - array[fIndex].z) * frag;
					array3[i].x = array2[i].x + (vector.x - array2[i].x) * num;
					array3[i].y = array2[i].y + (vector.y - array2[i].y) * num;
					array3[i].z = array2[i].z + (vector.z - array2[i].z) * num;
					Vector3 b = Vector3.SlerpUnclamped(array5[fIndex], array5[fIndex + 1], frag);
					normals.Array[i] = Vector3.SlerpUnclamped(array4[i], b, num);
				}
			}
			else
			{
				Vector3 vector2 = default(Vector3);
				for (int j = 0; j < num2; j++)
				{
					float frag2;
					int fIndex2 = shapeA.GetFIndex(shapeB.RelativeDistances.Array[j], out frag2);
					vector2.x = array2[fIndex2].x + (array2[fIndex2 + 1].x - array2[fIndex2].x) * frag2;
					vector2.y = array2[fIndex2].y + (array2[fIndex2 + 1].y - array2[fIndex2].y) * frag2;
					vector2.z = array2[fIndex2].z + (array2[fIndex2 + 1].z - array2[fIndex2].z) * frag2;
					array3[j].x = vector2.x + (array[j].x - vector2.x) * num;
					array3[j].y = vector2.y + (array[j].y - vector2.y) * num;
					array3[j].z = vector2.z + (array[j].z - vector2.z) * num;
					Vector3 a = Vector3.SlerpUnclamped(array4[fIndex2], array4[fIndex2 + 1], frag2);
					normals.Array[j] = Vector3.SlerpUnclamped(a, array5[j], num);
				}
			}
			resultShape.Positions = positions;
			resultShape.RelativeDistances = ArrayPools.Single.Allocate(num2);
			resultShape.Recalculate();
			resultShape.Normals = normals;
			resultShape.CustomValues = ArrayPools.Single.Clone(cGShape.CustomValues);
			resultShape.SourceRelativeDistances = ArrayPools.Single.Clone(cGShape.SourceRelativeDistances);
			resultShape.MaterialGroups = cGShape.MaterialGroups.Select((SamplePointsMaterialGroup g) => g.Clone()).ToList();
			if (!ignoreWarnings)
			{
				if (shapeA.Closed != shapeB.Closed)
				{
					warningsContainer.Add("Mixing inputs with different Closed values is not supported");
				}
				if (shapeA.Seamless != shapeB.Seamless)
				{
					warningsContainer.Add("Mixing inputs with different Seamless values is not supported");
				}
				if (shapeA.SourceIsManaged != shapeB.SourceIsManaged)
				{
					warningsContainer.Add("Mixing inputs with different SourceIsManaged values is not supported");
				}
			}
			resultShape.Closed = shapeA.Closed;
			resultShape.Seamless = shapeA.Seamless;
			resultShape.SourceIsManaged = shapeA.SourceIsManaged;
		}
	}
}
