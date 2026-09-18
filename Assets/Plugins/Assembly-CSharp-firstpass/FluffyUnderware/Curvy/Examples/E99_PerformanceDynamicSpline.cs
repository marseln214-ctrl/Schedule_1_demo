using System;
using FluffyUnderware.Curvy.Generator;
using FluffyUnderware.DevTools;
using JetBrains.Annotations;
using UnityEngine;

namespace FluffyUnderware.Curvy.Examples
{
	public class E99_PerformanceDynamicSpline : MonoBehaviour
	{
		private CurvySpline mSpline;

		public CurvyGenerator Generator;

		[Positive]
		public int UpdateInterval = 200;

		[RangeEx(2f, 2000f, "", "")]
		public int CPCount = 100;

		[Positive]
		public float Radius = 20f;

		public bool AlwaysClear;

		public bool UpdateCG;

		private float mAngleStep;

		private float mCurrentAngle;

		private float mLastUpdateTime;

		private readonly TimeMeasure ExecTimes = new TimeMeasure(10);

		[UsedImplicitly]
		private void Awake()
		{
			mSpline = GetComponent<CurvySpline>();
		}

		[UsedImplicitly]
		private void Start()
		{
			for (int i = 0; i < CPCount; i++)
			{
				addCP();
			}
			mSpline.Refresh();
			mLastUpdateTime = Time.timeSinceLevelLoad + 0.1f;
		}

		[UsedImplicitly]
		private void Update()
		{
			if (Time.timeSinceLevelLoad - (float)UpdateInterval * 0.001f > mLastUpdateTime)
			{
				mLastUpdateTime = Time.timeSinceLevelLoad;
				ExecTimes.Start();
				if (AlwaysClear)
				{
					mSpline.Clear();
				}
				while (mSpline.ControlPointCount > CPCount)
				{
					mSpline.Delete(mSpline.ControlPointsList[0], skipRefreshingAndEvents: true);
				}
				while (mSpline.ControlPointCount <= CPCount)
				{
					addCP();
				}
				mSpline.Refresh();
				ExecTimes.Stop();
			}
		}

		private void addCP()
		{
			mAngleStep = MathF.PI * 2f / ((float)CPCount + (float)CPCount * 0.25f);
			Vector3 position = base.transform.localToWorldMatrix.MultiplyPoint3x4(new Vector3(Mathf.Sin(mCurrentAngle) * Radius, Mathf.Cos(mCurrentAngle) * Radius, 0f));
			mSpline.InsertAfter(null, position, skipRefreshingAndEvents: true);
			mCurrentAngle = Mathf.Repeat(mCurrentAngle + mAngleStep, MathF.PI * 2f);
		}

		[UsedImplicitly]
		private void OnGUI()
		{
			GUILayout.BeginVertical(GUI.skin.box);
			GUILayout.BeginHorizontal();
			GUILayout.Label("Interval", GUILayout.Width(130f));
			UpdateInterval = (int)GUILayout.HorizontalSlider(UpdateInterval, 0f, 5000f, GUILayout.Width(200f));
			GUILayout.Label(UpdateInterval.ToString());
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			GUILayout.Label("# of Control Points", GUILayout.Width(130f));
			CPCount = (int)GUILayout.HorizontalSlider(CPCount, 2f, 200f, GUILayout.Width(200f));
			GUILayout.Label(CPCount.ToString());
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			GUILayout.Label("Radius", GUILayout.Width(130f));
			Radius = GUILayout.HorizontalSlider(Radius, 10f, 100f, GUILayout.Width(200f));
			GUILayout.Label(Radius.ToString("0.00"));
			GUILayout.EndHorizontal();
			AlwaysClear = GUILayout.Toggle(AlwaysClear, "Always clear");
			bool updateCG = UpdateCG;
			UpdateCG = GUILayout.Toggle(UpdateCG, "Use Curvy Generator");
			if (updateCG != UpdateCG)
			{
				Generator.gameObject.SetActive(UpdateCG);
			}
			GUILayout.Label("Avg. Execution Time (ms): " + ExecTimes.AverageMS.ToString("0.000"));
			GUILayout.EndVertical();
		}
	}
}
