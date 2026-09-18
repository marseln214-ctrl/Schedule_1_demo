using System;
using System.Collections.Generic;
using System.Linq;
using FluffyUnderware.Curvy.Utils;
using JetBrains.Annotations;
using UnityEngine;

namespace FluffyUnderware.Curvy.ImportExport
{
	public static class SplineJsonConverter
	{
		public static string SplinesToJson(IEnumerable<CurvySpline> splines, CurvySerializationSpace coordinatesSpace = CurvySerializationSpace.Global, bool prettify = true)
		{
			SerializedCurvySpline[] array = splines.Select((CurvySpline s) => new SerializedCurvySpline(s, coordinatesSpace)).ToArray();
			return JsonUtility.ToJson(new SerializableArray<SerializedCurvySpline>
			{
				Array = array
			}, prettify);
		}

		public static string SplineToJson(CurvySpline spline, CurvySerializationSpace coordinatesSpace = CurvySerializationSpace.Global, bool prettify = true)
		{
			return SplinesToJson(new CurvySpline[1] { spline }, coordinatesSpace, prettify);
		}

		public static CurvySpline[] JsonToSplines(string json, CurvySerializationSpace coordinatesSpace = CurvySerializationSpace.Global)
		{
			SerializedCurvySpline[] array = JsonToSerializedSplines(json);
			CurvySpline[] array2 = new CurvySpline[array.Length];
			for (int i = 0; i < array.Length; i++)
			{
				array[i].WriteIntoSpline(array2[i] = CurvySpline.Create(), coordinatesSpace);
			}
			return array2;
		}

		public static CurvySpline JsonToSpline(string json, CurvySerializationSpace coordinatesSpace = CurvySerializationSpace.Global)
		{
			return JsonToSplines(json, coordinatesSpace).Single();
		}

		public static SerializedCurvySpline[] JsonToSerializedSplines([NotNull] string json)
		{
			if (json == null)
			{
				throw new ArgumentNullException("json");
			}
			if (string.IsNullOrWhiteSpace(json))
			{
				throw new ArgumentException("Value cannot be null or whitespace.", "json");
			}
			if (string.IsNullOrEmpty(json))
			{
				throw new ArgumentException("Value cannot be null or empty.", "json");
			}
			SerializableArray<SerializedCurvySpline> serializableArray = JsonUtility.FromJson<SerializableArray<SerializedCurvySpline>>(json);
			for (int i = 0; i < serializableArray.Array.Length; i++)
			{
				int num = serializableArray.Array[i].ControlPoints.Length;
				SerializedCurvySpline serializedCurvySpline = new SerializedCurvySpline();
				serializedCurvySpline.ControlPoints = new SerializedCurvySplineSegment[num];
				for (int j = 0; j < num; j++)
				{
					serializedCurvySpline.ControlPoints[j] = new SerializedCurvySplineSegment();
				}
				serializableArray.Array[i] = serializedCurvySpline;
			}
			JsonUtility.FromJsonOverwrite(json, serializableArray);
			return serializableArray.Array;
		}
	}
}
