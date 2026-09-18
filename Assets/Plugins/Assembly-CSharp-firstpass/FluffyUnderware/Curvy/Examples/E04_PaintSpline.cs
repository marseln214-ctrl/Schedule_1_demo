using FluffyUnderware.Curvy.Controllers;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace FluffyUnderware.Curvy.Examples
{
	public class E04_PaintSpline : MonoBehaviour
	{
		public float StepDistance = 30f;

		public SplineController Controller;

		public Text InfoText;

		private CurvySpline mSpline;

		private Vector2 mLastControlPointPos;

		private bool mResetSpline = true;

		[UsedImplicitly]
		private void Awake()
		{
			mSpline = GetComponent<CurvySpline>();
		}

		[UsedImplicitly]
		private void OnGUI()
		{
			if (mSpline == null || !mSpline.IsInitialized || !Controller)
			{
				return;
			}
			Event current = Event.current;
			switch (current.type)
			{
			case EventType.MouseDrag:
				if (mResetSpline)
				{
					mSpline.Clear();
					addCP(current.mousePosition);
					Controller.gameObject.SetActive(value: true);
					Controller.AbsolutePosition = 0f;
					mLastControlPointPos = current.mousePosition;
					mResetSpline = false;
				}
				else if ((current.mousePosition - mLastControlPointPos).magnitude >= StepDistance)
				{
					mLastControlPointPos = current.mousePosition;
					addCP(current.mousePosition);
					if (Controller.PlayState != CurvyController.CurvyControllerState.Playing)
					{
						Controller.Play();
					}
				}
				break;
			case EventType.MouseUp:
				mResetSpline = true;
				break;
			}
		}

		private CurvySplineSegment addCP(Vector3 mousePos)
		{
			Vector3 position = Camera.main.ScreenToWorldPoint(mousePos);
			position.y *= -1f;
			position.z += 100f;
			CurvySplineSegment result = mSpline.InsertAfter(null, position);
			InfoText.text = "Control Points: " + mSpline.ControlPointCount;
			return result;
		}
	}
}
