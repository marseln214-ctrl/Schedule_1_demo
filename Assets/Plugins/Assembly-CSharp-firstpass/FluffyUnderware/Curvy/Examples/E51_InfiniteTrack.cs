using System;
using System.Collections;
using FluffyUnderware.Curvy.Controllers;
using FluffyUnderware.Curvy.Generator;
using FluffyUnderware.Curvy.Generator.Modules;
using FluffyUnderware.Curvy.Shapes;
using FluffyUnderware.DevTools;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace FluffyUnderware.Curvy.Examples
{
	public class E51_InfiniteTrack : MonoBehaviour
	{
		public CurvySpline TrackSpline;

		public CurvyController Controller;

		public Material RoadMaterial;

		public Text TxtStats;

		[Positive]
		public float CurvationX = 10f;

		[Positive]
		public float CurvationY = 10f;

		[Positive]
		public float CPStepSize = 20f;

		[Positive]
		public int HeadCP = 3;

		[Positive]
		public int TailCP = 2;

		[FluffyUnderware.DevTools.Min(3f, "", "")]
		public int Sections = 6;

		[FluffyUnderware.DevTools.Min(1f, "", "")]
		public int SectionCPCount = 2;

		private int mInitState;

		private bool mUpdateSpline;

		private int mUpdateIn;

		private CurvyGenerator[] mGenerators;

		private int mCurrentGen;

		private float lastSectionEndV;

		private Vector3 mDir;

		private readonly TimeMeasure timeSpline = new TimeMeasure(30);

		private readonly TimeMeasure timeCG = new TimeMeasure(1);

		[UsedImplicitly]
		private void Start()
		{
			updateStats();
		}

		[UsedImplicitly]
		private void FixedUpdate()
		{
			if (mInitState == 0)
			{
				StartCoroutine("setup");
			}
			if (mInitState == 2 && mUpdateSpline)
			{
				advanceTrack();
			}
		}

		private IEnumerator setup()
		{
			mInitState = 1;
			mGenerators = new CurvyGenerator[Sections];
			TrackSpline.InsertAfter(null, Vector3.zero, skipRefreshingAndEvents: true);
			mDir = Vector3.forward;
			int num = TailCP + HeadCP + Sections * SectionCPCount;
			for (int j = 0; j < num; j++)
			{
				addTrackCP();
			}
			TrackSpline.Refresh();
			for (int k = 0; k < Sections; k++)
			{
				mGenerators[k] = buildGenerator();
				mGenerators[k].name = "Generator " + k;
			}
			for (int i = 0; i < Sections; i++)
			{
				while (!mGenerators[i].IsInitialized)
				{
					yield return 0;
				}
			}
			for (int l = 0; l < Sections; l++)
			{
				updateSectionGenerator(mGenerators[l], l * SectionCPCount + TailCP, (l + 1) * SectionCPCount + TailCP);
			}
			mInitState = 2;
			mUpdateIn = SectionCPCount;
			Controller.AbsolutePosition = TrackSpline.ControlPointsList[TailCP + 2].Distance;
		}

		private CurvyGenerator buildGenerator()
		{
			CurvyGenerator curvyGenerator = CurvyGenerator.Create();
			curvyGenerator.AutoRefresh = false;
			InputSplinePath inputSplinePath = curvyGenerator.AddModule<InputSplinePath>();
			InputSplineShape inputSplineShape = curvyGenerator.AddModule<InputSplineShape>();
			BuildShapeExtrusion buildShapeExtrusion = curvyGenerator.AddModule<BuildShapeExtrusion>();
			BuildVolumeMesh buildVolumeMesh = curvyGenerator.AddModule<BuildVolumeMesh>();
			CreateMesh createMesh = curvyGenerator.AddModule<CreateMesh>();
			inputSplinePath.OutputByName["Path"].LinkTo(buildShapeExtrusion.InputByName["Path"]);
			inputSplineShape.OutputByName["Shape"].LinkTo(buildShapeExtrusion.InputByName["Cross"]);
			buildShapeExtrusion.OutputByName["Volume"].LinkTo(buildVolumeMesh.InputByName["Volume"]);
			buildVolumeMesh.OutputByName["VMesh"].LinkTo(createMesh.InputByName["VMesh"]);
			inputSplinePath.Spline = TrackSpline;
			inputSplinePath.UseCache = true;
			CSRectangle cSRectangle = inputSplineShape.SetManagedShape<CSRectangle>();
			cSRectangle.Width = 20f;
			cSRectangle.Height = 2f;
			buildShapeExtrusion.Optimize = false;
			buildShapeExtrusion.CrossHardEdges = true;
			buildVolumeMesh.Split = false;
			buildVolumeMesh.SetMaterial(0, RoadMaterial);
			buildVolumeMesh.MaterialSettings[0].SwapUV = true;
			createMesh.Collider = CGColliderEnum.None;
			return curvyGenerator;
		}

		private void advanceTrack()
		{
			timeSpline.Start();
			float num = Controller.AbsolutePosition;
			for (int i = 0; i < SectionCPCount; i++)
			{
				num -= TrackSpline.ControlPointsList[0].Length;
				TrackSpline.Delete(TrackSpline.ControlPointsList[0], skipRefreshingAndEvents: true);
			}
			for (int j = 0; j < SectionCPCount; j++)
			{
				addTrackCP();
			}
			TrackSpline.Refresh();
			Controller.AbsolutePosition = num;
			mUpdateSpline = false;
			timeSpline.Stop();
			advanceSections();
			updateStats();
		}

		private void advanceSections()
		{
			CurvyGenerator gen = mGenerators[mCurrentGen++];
			int num = TrackSpline.ControlPointCount - HeadCP - 1;
			updateSectionGenerator(gen, num - SectionCPCount, num);
			if (mCurrentGen == Sections)
			{
				mCurrentGen = 0;
			}
		}

		private void updateStats()
		{
			TxtStats.text = $"Spline Update: {timeSpline.AverageMS:0.00} ms\nGenerator Update: {timeCG.AverageMS:0.00} ms";
		}

		private void updateSectionGenerator(CurvyGenerator gen, int startCP, int endCP)
		{
			gen.FindModules<InputSplinePath>(includeOnRequestProcessing: true)[0].SetRange(TrackSpline.ControlPointsList[startCP], TrackSpline.ControlPointsList[endCP]);
			BuildVolumeMesh buildVolumeMesh = gen.FindModules<BuildVolumeMesh>(includeOnRequestProcessing: false)[0];
			buildVolumeMesh.MaterialSettings[0].UVOffset.y = lastSectionEndV % 1f;
			timeCG.Start();
			gen.Refresh();
			timeCG.Stop();
			if (buildVolumeMesh.OutVMesh.Data.Length == 0)
			{
				throw new InvalidOperationException("No VMesh data found");
			}
			CGVMesh cGVMesh = (CGVMesh)buildVolumeMesh.OutVMesh.Data[0];
			lastSectionEndV = cGVMesh.UVs.Array[cGVMesh.Count - 1].y;
		}

		public void Track_OnControlPointReached(CurvySplineMoveEventArgs e)
		{
			if (--mUpdateIn == 0)
			{
				mUpdateSpline = true;
				mUpdateIn = SectionCPCount;
			}
		}

		private void addTrackCP()
		{
			Vector3 localPosition = TrackSpline.ControlPointsList[TrackSpline.ControlPointCount - 1].transform.localPosition;
			Vector3 position = TrackSpline.transform.localToWorldMatrix.MultiplyPoint3x4(localPosition + mDir * CPStepSize);
			float x = UnityEngine.Random.value * CurvationX * DTUtility.RandomSign();
			float y = UnityEngine.Random.value * CurvationY * DTUtility.RandomSign();
			mDir = Quaternion.Euler(x, y, 0f) * mDir;
			CurvySplineSegment curvySplineSegment = TrackSpline.InsertAfter(null, position, skipRefreshingAndEvents: true);
			if ((TrackSpline.ControlPointCount - 1 - TailCP) % SectionCPCount == 0)
			{
				curvySplineSegment.SerializedOrientationAnchor = true;
			}
		}
	}
}
