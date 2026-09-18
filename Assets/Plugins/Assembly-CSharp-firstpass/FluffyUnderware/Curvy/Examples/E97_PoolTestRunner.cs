using FluffyUnderware.Curvy.Components;
using FluffyUnderware.DevTools;
using FluffyUnderware.DevTools.Extensions;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace FluffyUnderware.Curvy.Examples
{
	public class E97_PoolTestRunner : MonoBehaviour
	{
		public CurvySpline Spline;

		public Text PoolCountInfo;

		[UsedImplicitly]
		private void Start()
		{
			checkForSpline();
		}

		[UsedImplicitly]
		private void Update()
		{
			CurvyGlobalManager instance = DTSingleton<CurvyGlobalManager>.Instance;
			PoolCountInfo.text = ((instance != null) ? $"Control Points in Pool: {instance.ControlPointPool.Count}" : "CurvyGlobalManager not found");
		}

		private void checkForSpline()
		{
			if (Spline == null)
			{
				Spline = CurvySpline.Create();
				Camera.main.GetComponent<CurvyGLRenderer>().Add(Spline);
				for (int i = 0; i < 4; i++)
				{
					AddCP();
				}
			}
		}

		public void AddCP()
		{
			checkForSpline();
			Spline.Add(Random.insideUnitCircle * 50f);
			Spline.Refresh();
		}

		public void DeleteCP()
		{
			if ((bool)Spline && Spline.ControlPointCount > 0)
			{
				int index = Random.Range(0, Spline.ControlPointCount - 1);
				Spline.Delete(Spline.ControlPointsList[index]);
			}
		}

		public void ClearSpline()
		{
			if ((bool)Spline)
			{
				Spline.Clear();
			}
		}

		public void DeleteSpline()
		{
			if ((bool)Spline)
			{
				Spline.gameObject.Destroy(isUndoable: false, doPrefabCheck: true);
			}
		}
	}
}
