using System.Collections.Generic;
using System.Reflection;
using FluffyUnderware.DevTools;
using FluffyUnderware.DevTools.Extensions;
using JetBrains.Annotations;
using UnityEngine;

namespace FluffyUnderware.Curvy.Examples
{
	public class E99_PerformanceAPI : MonoBehaviour
	{
		private const int LOOPS = 20;

		private readonly List<string> mTests = new List<string>();

		private readonly List<string> mTestResults = new List<string>();

		private CurvyInterpolation mInterpolation = CurvyInterpolation.CatmullRom;

		private CurvyOrientation mOrientation = CurvyOrientation.Dynamic;

		private int mCacheSize = 50;

		private int mControlPointCount = 20;

		private int mTotalSplineLength = 100;

		private bool mUseCache;

		private bool mUseMultiThreads = true;

		private int mCurrentTest = -1;

		private bool mExecuting;

		private readonly TimeMeasure Timer = new TimeMeasure(20);

		private MethodInfo mGUIMethod;

		private MethodInfo mRunMethod;

		private bool mInterpolate_UseDistance;

		private int mRefresh_Mode;

		[UsedImplicitly]
		private void Awake()
		{
			mTests.Add("Interpolate");
			mTests.Add("Refresh");
		}

		[UsedImplicitly]
		private void OnGUI()
		{
			GUILayout.BeginVertical(GUI.skin.box);
			GUILayout.Label("Curvy offers various options to fine-tune performance vs. precision balance:");
			GUILayout.BeginHorizontal();
			GUILayout.Label("Interpolation: ");
			if (GUILayout.Toggle(mInterpolation == CurvyInterpolation.Linear, "Linear", GUI.skin.button))
			{
				mInterpolation = CurvyInterpolation.Linear;
			}
			if (GUILayout.Toggle(mInterpolation == CurvyInterpolation.Bezier, "Bezier", GUI.skin.button))
			{
				mInterpolation = CurvyInterpolation.Bezier;
			}
			if (GUILayout.Toggle(mInterpolation == CurvyInterpolation.CatmullRom, "CatmullRom", GUI.skin.button))
			{
				mInterpolation = CurvyInterpolation.CatmullRom;
			}
			if (GUILayout.Toggle(mInterpolation == CurvyInterpolation.TCB, "TCB", GUI.skin.button))
			{
				mInterpolation = CurvyInterpolation.TCB;
			}
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			GUILayout.Label("Orientation: ");
			if (GUILayout.Toggle(mOrientation == CurvyOrientation.None, "None", GUI.skin.button))
			{
				mOrientation = CurvyOrientation.None;
			}
			if (GUILayout.Toggle(mOrientation == CurvyOrientation.Static, "Static", GUI.skin.button))
			{
				mOrientation = CurvyOrientation.Static;
			}
			if (GUILayout.Toggle(mOrientation == CurvyOrientation.Dynamic, "Dynamic", GUI.skin.button))
			{
				mOrientation = CurvyOrientation.Dynamic;
			}
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			GUILayout.Label("Control Points (max): " + mControlPointCount);
			mControlPointCount = (int)GUILayout.HorizontalSlider(mControlPointCount, 2f, 1000f);
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			GUILayout.Label("Total spline length: " + mTotalSplineLength);
			mTotalSplineLength = (int)GUILayout.HorizontalSlider(mTotalSplineLength, 5f, 10000f);
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			GUILayout.Label("Cache Density: " + mCacheSize);
			mCacheSize = (int)GUILayout.HorizontalSlider(mCacheSize, 1f, 100f);
			GUILayout.EndHorizontal();
			mUseCache = GUILayout.Toggle(mUseCache, "Use Cache (where applicable)");
			mUseMultiThreads = GUILayout.Toggle(mUseMultiThreads, "Use Multiple Threads (where applicable)");
			GUILayout.Label("Select Test:");
			int num = GUILayout.SelectionGrid(Mathf.Max(0, mCurrentTest), mTests.ToArray(), 4);
			if (num != mCurrentTest)
			{
				mCurrentTest = num;
				Timer.Clear();
				mTestResults.Clear();
				mGUIMethod = GetType().MethodByName("GUI_" + mTests[mCurrentTest], includeInherited: false, includePrivate: true);
				mRunMethod = GetType().MethodByName("Test_" + mTests[mCurrentTest], includeInherited: false, includePrivate: true);
			}
			GUILayout.Space(5f);
			if (mGUIMethod != null)
			{
				mGUIMethod.Invoke(this, null);
			}
			GUI.enabled = !mExecuting && mRunMethod != null;
			if (GUILayout.Button(mExecuting ? "Please wait..." : ("Run (" + 20 + " times)")))
			{
				mExecuting = true;
				Timer.Clear();
				mTestResults.Clear();
				Invoke("runTest", 0.5f);
			}
			GUI.enabled = true;
			if (Timer.Count > 0)
			{
				foreach (string mTestResult in mTestResults)
				{
					GUILayout.Label(mTestResult);
				}
				GUILayout.Label($"Average (ms): {Timer.AverageMS:0.0000}");
				GUILayout.Label($"Minimum (ms): {Timer.MinimumMS:0.0000}");
				GUILayout.Label($"Maximum (ms): {Timer.MaximumMS:0.0000}");
			}
			GUILayout.EndVertical();
		}

		[UsedImplicitly]
		private void GUI_Interpolate()
		{
			GUILayout.Label("Interpolates position");
			mInterpolate_UseDistance = GUILayout.Toggle(mInterpolate_UseDistance, "By Distance");
		}

		[UsedImplicitly]
		private void Test_Interpolate()
		{
			CurvySpline spline = getSpline();
			AddCPs(ref spline, mControlPointCount, mTotalSplineLength);
			mTestResults.Add("Cache Points: " + spline.CacheSize);
			mTestResults.Add($"Cache Point Distance: {(float)mTotalSplineLength / (float)spline.CacheSize:0.000}");
			Vector3 vector = Vector3.zero;
			if (mInterpolate_UseDistance)
			{
				for (int i = 0; i < 20; i++)
				{
					float distance = Random.Range(0f, spline.Length);
					if (mUseCache)
					{
						Timer.Start();
						vector = spline.InterpolateByDistanceFast(distance);
						Timer.Stop();
					}
					else
					{
						Timer.Start();
						vector = spline.InterpolateByDistance(distance);
						Timer.Stop();
					}
				}
			}
			else
			{
				for (int j = 0; j < 20; j++)
				{
					float tf = Random.Range(0, 1);
					if (mUseCache)
					{
						Timer.Start();
						vector = spline.InterpolateFast(tf);
						Timer.Stop();
					}
					else
					{
						Timer.Start();
						vector = spline.Interpolate(tf);
						Timer.Stop();
					}
				}
			}
			Object.Destroy(spline.gameObject);
			vector.Set(0f, 0f, 0f);
		}

		[UsedImplicitly]
		private void GUI_Refresh()
		{
			GUILayout.Label("Refresh Spline or Single segment!");
			GUILayout.BeginHorizontal();
			GUILayout.Label("Mode:");
			mRefresh_Mode = GUILayout.SelectionGrid(mRefresh_Mode, new string[2] { "All", "Single random segment" }, 2);
			GUILayout.EndHorizontal();
		}

		[UsedImplicitly]
		private void Test_Refresh()
		{
			CurvySpline spline = getSpline();
			AddCPs(ref spline, mControlPointCount, mTotalSplineLength);
			mTestResults.Add("Cache Points: " + spline.CacheSize);
			mTestResults.Add($"Cache Point Distance: {(float)mTotalSplineLength / (float)spline.CacheSize:0.000}");
			for (int i = 0; i < 20; i++)
			{
				int idx = Random.Range(0, spline.Count - 1);
				if (mRefresh_Mode == 0)
				{
					Timer.Start();
					spline.SetDirtyAll(SplineDirtyingType.Everything, dirtyConnectedControlPoints: true);
					spline.Refresh();
					Timer.Stop();
				}
				else
				{
					Timer.Start();
					spline.SetDirty(spline[idx], SplineDirtyingType.Everything);
					spline.Refresh();
					Timer.Stop();
				}
			}
			Object.Destroy(spline.gameObject);
		}

		private CurvySpline getSpline()
		{
			CurvySpline curvySpline = CurvySpline.Create();
			curvySpline.Interpolation = mInterpolation;
			curvySpline.Orientation = mOrientation;
			curvySpline.CacheDensity = mCacheSize;
			curvySpline.UseThreading = mUseMultiThreads;
			curvySpline.Refresh();
			return curvySpline;
		}

		private void AddCPs(ref CurvySpline spline, int count, int totalLength)
		{
			Vector3[] array = new Vector3[count];
			float x = (float)totalLength / (float)(count - 1);
			array[0] = Vector3.zero;
			for (int i = 1; i < count; i++)
			{
				array[i] = array[i - 1] + new Vector3(x, 0f, 0f);
			}
			spline.Add(array);
			spline.ControlPointsList[0].Swirl = CurvyOrientationSwirl.AnchorGroupAbs;
			spline.ControlPointsList[0].SwirlTurns = 1f;
			spline.Refresh();
		}

		private void runTest()
		{
			mRunMethod.Invoke(this, null);
			mExecuting = false;
		}
	}
}
