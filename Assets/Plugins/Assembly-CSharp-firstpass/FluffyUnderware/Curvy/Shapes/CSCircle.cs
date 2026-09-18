using System;
using FluffyUnderware.DevTools;
using UnityEngine;

namespace FluffyUnderware.Curvy.Shapes
{
	[CurvyShapeInfo("2D/Circle", true)]
	[RequireComponent(typeof(CurvySpline))]
	[AddComponentMenu("Curvy/Shapes/Circle")]
	public class CSCircle : CurvyShape2D
	{
		private const int MinCount = 2;

		[Positive(MinValue = 2f, Tooltip = "Number of Control Points")]
		[SerializeField]
		private int m_Count = 4;

		[Positive]
		[SerializeField]
		private float m_Radius = 1f;

		public int Count
		{
			get
			{
				return m_Count;
			}
			set
			{
				int num = Mathf.Max(2, value);
				if (m_Count != num)
				{
					m_Count = num;
					Dirty = true;
				}
			}
		}

		public float Radius
		{
			get
			{
				return m_Radius;
			}
			set
			{
				float num = Mathf.Max(0f, value);
				if (m_Radius != num)
				{
					m_Radius = num;
					Dirty = true;
				}
			}
		}

		protected override void ApplyShape()
		{
			base.ApplyShape();
			PrepareSpline(CurvyInterpolation.Bezier);
			PrepareControlPoints(Count);
			float num = MathF.PI * 2f / (float)Count;
			for (int i = 0; i < Count; i++)
			{
				base.Spline.ControlPointsList[i].transform.localPosition = new Vector3(Mathf.Sin(num * (float)i) * Radius, Mathf.Cos(num * (float)i) * Radius, 0f);
			}
		}
	}
}
