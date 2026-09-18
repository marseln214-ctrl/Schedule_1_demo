using JetBrains.Annotations;
using UnityEngine;

namespace FluffyUnderware.Curvy.Examples
{
	[ExecuteAlways]
	public class E12_TrainManager : MonoBehaviour
	{
		public CurvySpline Spline;

		public float Speed;

		public float Position;

		public float CarSize = 10f;

		public float AxisDistance = 8f;

		public float CarGap = 1f;

		public float Limit = 0.2f;

		private bool isSetup;

		private E12_TrainCarManager[] Cars;

		[UsedImplicitly]
		private void Start()
		{
			setup();
		}

		[UsedImplicitly]
		private void OnDisable()
		{
			isSetup = false;
		}

		[UsedImplicitly]
		private void LateUpdate()
		{
			if (!isSetup)
			{
				setup();
			}
			if (Cars.Length <= 1)
			{
				return;
			}
			E12_TrainCarManager e12_TrainCarManager = Cars[0];
			E12_TrainCarManager e12_TrainCarManager2 = Cars[Cars.Length - 1];
			if (!(e12_TrainCarManager.FrontAxis.Spline == e12_TrainCarManager2.BackAxis.Spline) || !(e12_TrainCarManager.FrontAxis.RelativePosition > e12_TrainCarManager2.BackAxis.RelativePosition))
			{
				return;
			}
			for (int i = 1; i < Cars.Length; i++)
			{
				float num = Cars[i - 1].Position - Cars[i].Position - CarSize - CarGap;
				if (Mathf.Abs(num) >= Limit)
				{
					Cars[i].Position += num;
				}
			}
		}

		private void setup()
		{
			if (Spline.Dirty)
			{
				Spline.Refresh();
			}
			Cars = GetComponentsInChildren<E12_TrainCarManager>();
			float num = Position - CarSize / 2f;
			for (int i = 0; i < Cars.Length; i++)
			{
				Cars[i].setup();
				if ((bool)Cars[i].BackAxis && (bool)Cars[i].FrontAxis && (bool)Cars[i].Waggon)
				{
					Cars[i].Position = num;
				}
				num -= CarSize + CarGap;
			}
			isSetup = true;
		}
	}
}
