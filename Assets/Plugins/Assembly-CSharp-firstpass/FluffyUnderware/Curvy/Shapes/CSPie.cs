using System;
using FluffyUnderware.DevTools;
using UnityEngine;

namespace FluffyUnderware.Curvy.Shapes
{
	[CurvyShapeInfo("2D/Pie", true)]
	[RequireComponent(typeof(CurvySpline))]
	[AddComponentMenu("Curvy/Shapes/Pie")]
	public class CSPie : CSCircle
	{
		public enum EatModeEnum
		{
			Left = 0,
			Right = 1,
			Center = 2
		}

		[Range(0f, 1f)]
		[SerializeField]
		private float m_Roundness = 1f;

		[SerializeField]
		[RangeEx(0f, "maxEmpty", "Empty", "Number of empty slices")]
		private int m_Empty = 1;

		[Label(Tooltip = "Eat Mode")]
		[SerializeField]
		private EatModeEnum m_Eat = EatModeEnum.Right;

		public float Roundness
		{
			get
			{
				return m_Roundness;
			}
			set
			{
				float num = Mathf.Clamp01(value);
				if (m_Roundness != num)
				{
					m_Roundness = num;
					Dirty = true;
				}
			}
		}

		public int Empty
		{
			get
			{
				return m_Empty;
			}
			set
			{
				int num = Mathf.Clamp(value, 0, maxEmpty);
				if (m_Empty != num)
				{
					m_Empty = num;
					Dirty = true;
				}
			}
		}

		private int maxEmpty => base.Count;

		public EatModeEnum Eat
		{
			get
			{
				return m_Eat;
			}
			set
			{
				if (m_Eat != value)
				{
					m_Eat = value;
					Dirty = true;
				}
			}
		}

		private Vector3 cpPosition(int i, int empty, float d)
		{
			return Eat switch
			{
				EatModeEnum.Left => new Vector3(Mathf.Sin(d * (float)i) * base.Radius, Mathf.Cos(d * (float)i) * base.Radius, 0f), 
				EatModeEnum.Right => new Vector3(Mathf.Sin(d * (float)(i + empty)) * base.Radius, Mathf.Cos(d * (float)(i + empty)) * base.Radius, 0f), 
				_ => new Vector3(Mathf.Sin(d * ((float)i + (float)empty * 0.5f)) * base.Radius, Mathf.Cos(d * ((float)i + (float)empty * 0.5f)) * base.Radius, 0f), 
			};
		}

		protected override void ApplyShape()
		{
			base.ApplyShape();
			PrepareSpline(CurvyInterpolation.Bezier);
			PrepareControlPoints(base.Count - Empty + 2);
			float d = MathF.PI * 2f / (float)base.Count;
			float num = Roundness * 0.39f;
			for (int i = 0; i < base.Spline.ControlPointCount - 1; i++)
			{
				base.Spline.ControlPointsList[i].AutoHandles = true;
				base.Spline.ControlPointsList[i].AutoHandleDistance = num;
				SetPosition(i, cpPosition(i, Empty, d));
			}
			SetPosition(base.Spline.ControlPointCount - 1, Vector3.zero);
			SetBezierHandles(base.Spline.ControlPointCount - 1, 0f);
			base.Spline.ControlPointsList[0].AutoHandles = false;
			base.Spline.ControlPointsList[0].HandleIn = Vector3.zero;
			base.Spline.ControlPointsList[0].SetBezierHandles(num, cpPosition(base.Count - 1, Empty, d) - base.Spline.ControlPointsList[0].transform.localPosition, cpPosition(1, Empty, d) - base.Spline.ControlPointsList[0].transform.localPosition, setIn: false);
			base.Spline.ControlPointsList[base.Spline.ControlPointCount - 2].AutoHandles = false;
			base.Spline.ControlPointsList[base.Spline.ControlPointCount - 2].HandleOut = Vector3.zero;
			base.Spline.ControlPointsList[base.Spline.ControlPointCount - 2].SetBezierHandles(num, cpPosition(base.Count - 1 - Empty, Empty, d) - base.Spline.ControlPointsList[base.Spline.ControlPointCount - 2].transform.localPosition, cpPosition(base.Count + 1 - Empty, Empty, d) - base.Spline.ControlPointsList[base.Spline.ControlPointCount - 2].transform.localPosition, setIn: true, setOut: false);
		}
	}
}
