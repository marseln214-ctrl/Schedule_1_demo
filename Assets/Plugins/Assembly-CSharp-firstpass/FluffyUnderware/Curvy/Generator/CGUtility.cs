using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace FluffyUnderware.Curvy.Generator
{
	public static class CGUtility
	{
		[UsedImplicitly]
		[Obsolete("Method will get remove in next major update. Copy its content if you need it")]
		public static Vector2[] CalculateUV2(Vector2[] uv)
		{
			Vector2[] array = new Vector2[uv.Length];
			CalculateUV2(uv, array, uv.Length);
			return array;
		}

		[UsedImplicitly]
		[Obsolete("Method will get remove in next major update. Copy its content if you need it")]
		public static void CalculateUV2(Vector2[] uv, Vector2[] uv2, int elementsNumber)
		{
			float num = 0f;
			float num2 = 0f;
			for (int i = 0; i < elementsNumber; i++)
			{
				num = ((num < uv[i].x) ? uv[i].x : num);
				num2 = ((num2 < uv[i].y) ? uv[i].y : num2);
			}
			float num3 = 1f / num;
			float num4 = 1f / num2;
			for (int j = 0; j < elementsNumber; j++)
			{
				uv2[j].x = uv[j].x * num3;
				uv2[j].y = uv[j].y * num4;
			}
		}

		public static List<ControlPointOption> GetControlPointsWithOptions(CGDataRequestMetaCGOptions options, CurvySpline shape, float startDist, float endDist, bool optimize, out int initialMaterialID, out float initialMaxStep)
		{
			List<ControlPointOption> list = new List<ControlPointOption>();
			initialMaterialID = 0;
			initialMaxStep = float.MaxValue;
			CurvySplineSegment curvySplineSegment = shape.DistanceToSegment(startDist);
			float num = shape.ClampDistance(endDist, shape.Closed ? CurvyClamping.Loop : CurvyClamping.Clamp);
			if (num == 0f)
			{
				num = endDist;
			}
			CurvySplineSegment curvySplineSegment2 = ((num == shape.Length) ? shape.LastVisibleControlPoint : shape.DistanceToSegment(num));
			if (endDist != shape.Length && endDist > curvySplineSegment2.Distance)
			{
				curvySplineSegment2 = shape.GetNextControlPoint(curvySplineSegment2);
			}
			float num2 = 0f;
			if ((bool)curvySplineSegment)
			{
				MetaCGOptions metadata = curvySplineSegment.GetMetadata<MetaCGOptions>(autoCreate: true);
				initialMaxStep = ((metadata.MaxStepDistance == 0f) ? float.MaxValue : metadata.MaxStepDistance);
				initialMaterialID = metadata.MaterialID;
				int num3 = initialMaterialID;
				float num4 = metadata.MaxStepDistance;
				CurvySplineSegment curvySplineSegment3 = shape.GetNextSegment(curvySplineSegment) ?? shape.GetNextControlPoint(curvySplineSegment);
				do
				{
					metadata = curvySplineSegment3.GetMetadata<MetaCGOptions>(autoCreate: true);
					if (shape.GetControlPointIndex(curvySplineSegment3) < shape.GetControlPointIndex(curvySplineSegment))
					{
						num2 = shape.Length;
					}
					if (options.IncludeControlPoints || metadata.CorrectedHardEdge || metadata.MaterialID != num3 || (optimize && metadata.MaxStepDistance != num4) || metadata.CorrectedUVEdge || metadata.ExplicitU)
					{
						num4 = ((metadata.MaxStepDistance == 0f) ? float.MaxValue : metadata.MaxStepDistance);
						num3 = metadata.MaterialID;
						list.Add(new ControlPointOption(curvySplineSegment3.TF + (float)Mathf.FloorToInt(num2 / shape.Length), curvySplineSegment3.Distance + num2, options.IncludeControlPoints, num3, metadata.CorrectedHardEdge, metadata.MaxStepDistance, metadata.CorrectedUVEdge, metadata.ExplicitU, metadata.FirstU, metadata.SecondU));
					}
					curvySplineSegment3 = shape.GetNextSegment(curvySplineSegment3);
				}
				while ((bool)curvySplineSegment3 && curvySplineSegment3 != curvySplineSegment2);
				if (!curvySplineSegment3 && shape.LastVisibleControlPoint == curvySplineSegment2)
				{
					metadata = curvySplineSegment2.GetMetadata<MetaCGOptions>(autoCreate: true);
					if (metadata.ExplicitU)
					{
						list.Add(new ControlPointOption(1f, curvySplineSegment2.Distance + num2, options.IncludeControlPoints, num3, metadata.CorrectedHardEdge, metadata.MaxStepDistance, metadata.CorrectedUVEdge, metadata.ExplicitU, metadata.FirstU, metadata.SecondU));
					}
				}
			}
			return list;
		}
	}
}
