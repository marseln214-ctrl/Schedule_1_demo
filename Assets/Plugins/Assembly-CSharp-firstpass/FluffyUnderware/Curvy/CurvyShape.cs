using System;
using System.Collections.Generic;
using FluffyUnderware.DevTools;
using FluffyUnderware.DevTools.Extensions;
using JetBrains.Annotations;
using UnityEngine;

namespace FluffyUnderware.Curvy
{
	[RequireComponent(typeof(CurvySpline))]
	[ExecuteAlways]
	[HelpURL("https://curvyeditor.com/doclink/curvyshape")]
	public class CurvyShape : DTVersionedMonoBehaviour
	{
		[SerializeField]
		[Label("Plane", "")]
		private CurvyPlane m_Plane;

		private static Dictionary<CurvyShapeInfo, Type> mShapeDefs = new Dictionary<CurvyShapeInfo, Type>();

		private CurvySpline mSpline;

		[NonSerialized]
		public bool Dirty;

		public CurvyPlane Plane
		{
			get
			{
				return m_Plane;
			}
			set
			{
				if (m_Plane != value)
				{
					m_Plane = value;
					Dirty = true;
				}
			}
		}

		public CurvySpline Spline
		{
			get
			{
				if ((object)mSpline == null)
				{
					mSpline = GetComponent<CurvySpline>();
				}
				return mSpline;
			}
		}

		[UsedImplicitly]
		[Obsolete("Method will be removed in the next major release. If you need it, please copy its implementation")]
		public static Dictionary<CurvyShapeInfo, Type> ShapeDefinitions
		{
			get
			{
				if (mShapeDefs.Count == 0)
				{
					mShapeDefs = typeof(CurvyShape).GetAllTypesWithAttribute<CurvyShapeInfo>();
				}
				return mShapeDefs;
			}
		}

		[UsedImplicitly]
		private void Update()
		{
			Refresh();
		}

		protected override void OnValidate()
		{
			base.OnValidate();
			if (base.IsActiveAndEnabled)
			{
				Dirty = true;
			}
		}

		protected override void Reset()
		{
			Dirty = true;
			base.Reset();
		}

		public void Delete()
		{
			this.Destroy(isUndoable: true, doPrefabCheck: false);
		}

		public void Refresh()
		{
			if (Dirty)
			{
				ApplyShape();
				ApplyPlane();
				Spline.SetDirtyAll();
				Spline.Refresh();
			}
			Dirty = false;
		}

		[UsedImplicitly]
		[Obsolete("This method will become an Editor only method")]
		public CurvyShape Replace(string menuName)
		{
			Type shapeType = GetShapeType(menuName);
			if (shapeType != null)
			{
				GameObject obj = base.gameObject;
				Delete();
				CurvyShape obj2 = (CurvyShape)obj.AddComponent(shapeType);
				obj2.Dirty = true;
				return obj2;
			}
			return null;
		}

		protected void PrepareSpline(CurvyInterpolation interpolation, CurvyOrientation orientation = CurvyOrientation.Dynamic, int cachedensity = 50, bool closed = true)
		{
			Spline.Interpolation = interpolation;
			Spline.Orientation = orientation;
			Spline.CacheDensity = cachedensity;
			Spline.Closed = closed;
			Spline.RestrictTo2D = this is CurvyShape2D;
		}

		protected void SetPosition(int no, Vector3 position)
		{
			Spline.ControlPointsList[no].SetLocalPosition(position);
		}

		protected void SetRotation(int no, Quaternion rotation)
		{
			Spline.ControlPointsList[no].SetLocalRotation(rotation);
		}

		protected void SetBezierHandles(int no, float distanceFrag)
		{
			SetBezierHandles(no, distanceFrag, distanceFrag);
		}

		protected void SetBezierHandles(int no, float inDistanceFrag, float outDistanceFrag)
		{
			CurvySplineSegment curvySplineSegment = Spline.ControlPointsList[no];
			if (no >= 0 && no < Spline.ControlPointCount)
			{
				if (inDistanceFrag == outDistanceFrag)
				{
					curvySplineSegment.AutoHandles = true;
					curvySplineSegment.AutoHandleDistance = inDistanceFrag;
					return;
				}
				curvySplineSegment.AutoHandles = false;
				curvySplineSegment.AutoHandleDistance = (inDistanceFrag + outDistanceFrag) / 2f;
				SetBezierHandles(inDistanceFrag, true, false, curvySplineSegment);
				SetBezierHandles(outDistanceFrag, false, true, curvySplineSegment);
			}
		}

		protected void SetBezierHandles(int no, Vector3 i, Vector3 o, Space space = Space.World)
		{
			if (no >= 0 && no < Spline.ControlPointCount)
			{
				CurvySplineSegment curvySplineSegment = Spline.ControlPointsList[no];
				curvySplineSegment.AutoHandles = false;
				if (space == Space.World)
				{
					curvySplineSegment.HandleInPosition = i;
					curvySplineSegment.HandleOutPosition = o;
				}
				else
				{
					curvySplineSegment.HandleIn = i;
					curvySplineSegment.HandleOut = o;
				}
			}
		}

		public static void SetBezierHandles(float distanceFrag, bool setIn, bool setOut, params CurvySplineSegment[] controlPoints)
		{
			if (controlPoints.Length != 0)
			{
				for (int i = 0; i < controlPoints.Length; i++)
				{
					controlPoints[i].SetBezierHandles(distanceFrag, setIn, setOut);
				}
			}
		}

		protected void SetCGHardEdges(params int[] controlPoints)
		{
			if (controlPoints.Length == 0)
			{
				for (int i = 0; i < Spline.ControlPointCount; i++)
				{
					Spline.ControlPointsList[i].GetMetadata<MetaCGOptions>(autoCreate: true).HardEdge = true;
				}
				return;
			}
			for (int j = 0; j < controlPoints.Length; j++)
			{
				if (j >= 0 && j < Spline.ControlPointCount)
				{
					Spline.ControlPointsList[j].GetMetadata<MetaCGOptions>(autoCreate: true).HardEdge = true;
				}
			}
		}

		protected virtual void ApplyShape()
		{
		}

		protected void PrepareControlPoints(int count)
		{
			int i = count - Spline.ControlPointCount;
			bool flag = i != 0;
			while (i > 0)
			{
				Spline.InsertAfter(null, skipRefreshingAndEvents: true);
				i--;
			}
			for (; i < 0; i++)
			{
				Spline.Delete(Spline.LastVisibleControlPoint, skipRefreshingAndEvents: true, isUndoableDeletion: false);
			}
			for (int j = 0; j < Spline.ControlPointsList.Count; j++)
			{
				CurvySplineSegment curvySplineSegment = Spline.ControlPointsList[j];
				curvySplineSegment.transform.localPosition = Vector3.zero;
				curvySplineSegment.transform.localRotation = Quaternion.identity;
				curvySplineSegment.transform.localScale = Vector3.one;
				curvySplineSegment.ResetConnectionUnrelatedProperties();
				curvySplineSegment.Disconnect();
				MetaCGOptions metadata = curvySplineSegment.GetMetadata<MetaCGOptions>();
				if ((bool)metadata)
				{
					metadata.ResetProperties();
				}
			}
			if (flag)
			{
				Spline.Refresh();
			}
		}

		[UsedImplicitly]
		[Obsolete("Method will be removed in the next major release. If you need it, please copy its implementation")]
		public static List<string> GetShapesMenuNames(bool only2D = false)
		{
			List<string> list = new List<string>();
			foreach (CurvyShapeInfo key in ShapeDefinitions.Keys)
			{
				if (!only2D || key.Is2D)
				{
					list.Add(key.Name);
				}
			}
			return list;
		}

		[UsedImplicitly]
		[Obsolete("This method will become an Editor only method")]
		public static List<string> GetShapesMenuNames(Type currentShapeType, out int currentIndex, bool only2D = false)
		{
			currentIndex = 0;
			if (currentShapeType == null)
			{
				return GetShapesMenuNames(only2D);
			}
			List<string> list = new List<string>();
			foreach (KeyValuePair<CurvyShapeInfo, Type> shapeDefinition in ShapeDefinitions)
			{
				if (!only2D || shapeDefinition.Key.Is2D)
				{
					list.Add(shapeDefinition.Key.Name);
				}
				if (shapeDefinition.Value == currentShapeType)
				{
					currentIndex = list.Count - 1;
				}
			}
			return list;
		}

		[UsedImplicitly]
		[Obsolete("Method will be removed in the next major release. If you need it, please copy its implementation")]
		public static string GetShapeName(Type shapeType)
		{
			foreach (KeyValuePair<CurvyShapeInfo, Type> shapeDefinition in ShapeDefinitions)
			{
				if (shapeDefinition.Value == shapeType)
				{
					return shapeDefinition.Key.Name;
				}
			}
			return null;
		}

		[UsedImplicitly]
		[Obsolete("This method will become an Editor only method")]
		public static Type GetShapeType(string menuName)
		{
			foreach (CurvyShapeInfo key in ShapeDefinitions.Keys)
			{
				if (key.Name == menuName)
				{
					return ShapeDefinitions[key];
				}
			}
			return null;
		}

		private void ApplyPlane()
		{
			switch (Plane)
			{
			case CurvyPlane.XZ:
				applyRotation(Quaternion.Euler(90f, 0f, 0f));
				break;
			case CurvyPlane.YZ:
				applyRotation(Quaternion.Euler(0f, 90f, 0f));
				break;
			default:
				applyRotation(Quaternion.Euler(0f, 0f, 0f));
				break;
			}
			Spline.Restricted2DPlane = Plane;
		}

		private void applyRotation(Quaternion q)
		{
			Spline.transform.localRotation = Quaternion.identity;
			for (int i = 0; i < Spline.ControlPointCount; i++)
			{
				CurvySplineSegment curvySplineSegment = Spline.ControlPointsList[i];
				curvySplineSegment.SetLocalRotation(q * curvySplineSegment.transform.localRotation);
				curvySplineSegment.SetLocalPosition(q * curvySplineSegment.transform.localPosition);
				if (Spline.Interpolation == CurvyInterpolation.Bezier)
				{
					curvySplineSegment.HandleIn = q * curvySplineSegment.HandleIn;
					curvySplineSegment.HandleOut = q * curvySplineSegment.HandleOut;
				}
			}
		}
	}
}
