using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluffyUnderware.DevTools;
using JetBrains.Annotations;
using ToolBuddy.ThirdParty.VectorGraphics;

namespace FluffyUnderware.Curvy.ImportExport
{
	public static class SplineSvgConverter
	{
		public static CurvySpline[] SvgToSplines(string svg, CurvySerializationSpace coordinatesSpace = CurvySerializationSpace.Global)
		{
			List<SerializedCurvySpline> list = SvgToSerializedSplines(svg);
			CurvySpline[] array = new CurvySpline[list.Count];
			for (int i = 0; i < list.Count; i++)
			{
				list[i].WriteIntoSpline(array[i] = CurvySpline.Create(), coordinatesSpace);
			}
			return array;
		}

		public static CurvySpline SvgToSpline(string svg, CurvySerializationSpace coordinatesSpace = CurvySerializationSpace.Global)
		{
			return SvgToSplines(svg, coordinatesSpace).Single();
		}

		public static List<SerializedCurvySpline> SvgToSerializedSplines([NotNull] string svg, bool invertY = true)
		{
			if (svg == null)
			{
				throw new ArgumentNullException("svg");
			}
			if (string.IsNullOrWhiteSpace(svg))
			{
				throw new ArgumentException("Value cannot be null or whitespace.", "svg");
			}
			if (string.IsNullOrEmpty(svg))
			{
				throw new ArgumentException("Value cannot be null or empty.", "svg");
			}
			List<SerializedCurvySpline> list = new List<SerializedCurvySpline>();
			using (StringReader textReader = new StringReader(svg))
			{
				SVGParser.SceneInfo sceneInfo = SVGParser.ImportSVG(textReader);
				DrawNode(sceneInfo.Scene.Root, sceneInfo.Scene.Root.Transform, list);
			}
			if (invertY)
			{
				foreach (SerializedCurvySpline item in list)
				{
					SerializedCurvySplineSegment[] controlPoints = item.ControlPoints;
					foreach (SerializedCurvySplineSegment obj in controlPoints)
					{
						obj.Position.y *= -1f;
						obj.HandleIn.y *= -1f;
						obj.HandleOut.y *= -1f;
					}
				}
			}
			return list;
		}

		private static void DrawNode(SceneNode node, Matrix2D rootTransform, List<SerializedCurvySpline> splines)
		{
			if (node.Clipper != null)
			{
				DTLog.LogWarning("[Curvy] SVG Import: A clipper was encountered. Clippers are not supported.");
			}
			if (node.Shapes != null)
			{
				Matrix2D matrix2D = rootTransform * node.Transform;
				foreach (Shape shape in node.Shapes)
				{
					BezierContour[] contours = shape.Contours;
					for (int i = 0; i < contours.Length; i++)
					{
						BezierContour bezierContour = contours[i];
						BezierPathSegment[] segments = bezierContour.Segments;
						List<SerializedCurvySplineSegment> list = new List<SerializedCurvySplineSegment>(segments.Length);
						if (segments.Length == 0)
						{
							continue;
						}
						if (segments.Length == 1)
						{
							DTLog.LogError("[Curvy] SVG Import: A segments array had only one element. This is unexpected. That contour was ignored. Please raise a bug report.");
							continue;
						}
						SerializedCurvySpline serializedCurvySpline = new SerializedCurvySpline();
						serializedCurvySpline.Interpolation = CurvyInterpolation.Bezier;
						serializedCurvySpline.Closed = bezierContour.Closed;
						serializedCurvySpline.Name = $"SVG Spline {splines.Count}";
						splines.Add(serializedCurvySpline);
						BezierPathSegment bezierPathSegment = segments.First();
						BezierPathSegment bezierPathSegment2 = segments.Last();
						SerializedCurvySplineSegment serializedCurvySplineSegment = new SerializedCurvySplineSegment();
						serializedCurvySplineSegment.Position = matrix2D.MultiplyPoint(bezierPathSegment.P0);
						serializedCurvySplineSegment.AutoHandles = false;
						serializedCurvySplineSegment.HandleIn = matrix2D.MultiplyVector(bezierPathSegment2.P2 - bezierPathSegment.P0);
						serializedCurvySplineSegment.HandleOut = matrix2D.MultiplyVector(bezierPathSegment.P1 - bezierPathSegment.P0);
						list.Add(serializedCurvySplineSegment);
						for (int j = 1; j < segments.Length; j++)
						{
							BezierPathSegment bezierPathSegment3 = segments[j - 1];
							BezierPathSegment bezierPathSegment4 = segments[j];
							SerializedCurvySplineSegment serializedCurvySplineSegment2 = new SerializedCurvySplineSegment();
							serializedCurvySplineSegment2.Position = matrix2D.MultiplyPoint(bezierPathSegment4.P0);
							serializedCurvySplineSegment2.AutoHandles = false;
							serializedCurvySplineSegment2.HandleIn = matrix2D.MultiplyVector(bezierPathSegment3.P2 - bezierPathSegment4.P0);
							serializedCurvySplineSegment2.HandleOut = matrix2D.MultiplyVector(bezierPathSegment4.P1 - bezierPathSegment4.P0);
							list.Add(serializedCurvySplineSegment2);
						}
						serializedCurvySpline.ControlPoints = list.ToArray();
					}
				}
			}
			if (node.Children == null)
			{
				return;
			}
			foreach (SceneNode child in node.Children)
			{
				DrawNode(child, rootTransform * child.Transform, splines);
			}
		}
	}
}
