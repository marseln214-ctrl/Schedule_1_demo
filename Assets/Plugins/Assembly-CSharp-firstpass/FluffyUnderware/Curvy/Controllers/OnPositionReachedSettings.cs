using System;
using UnityEngine;

namespace FluffyUnderware.Curvy.Controllers
{
	[Serializable]
	public class OnPositionReachedSettings : ISerializationCallbackReceiver
	{
		public string Name;

		public CurvySplineMoveEvent Event = new CurvySplineMoveEvent();

		public float Position;

		public CurvyPositionMode PositionMode;

		public TriggeringDirections TriggeringDirections;

		public Color GizmoColor;

		[SerializeField]
		[HideInInspector]
		private bool initialized;

		public OnPositionReachedSettings()
		{
			InitializeFieldsWithDefaultValue();
		}

		private void InitializeFieldsWithDefaultValue()
		{
			Name = "My Event";
			PositionMode = CurvyPositionMode.WorldUnits;
			TriggeringDirections = TriggeringDirections.All;
			GizmoColor = new Color(0.652f, 0.652f, 0.652f);
			initialized = true;
		}

		public void OnBeforeSerialize()
		{
		}

		public void OnAfterDeserialize()
		{
			if (!initialized)
			{
				InitializeFieldsWithDefaultValue();
			}
		}

		public OnPositionReachedSettings Clone()
		{
			return (OnPositionReachedSettings)MemberwiseClone();
		}
	}
}
