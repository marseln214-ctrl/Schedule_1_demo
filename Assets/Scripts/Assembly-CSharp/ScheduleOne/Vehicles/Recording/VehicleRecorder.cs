using System.Collections.Generic;
using ScheduleOne.DevUtilities;
using ScheduleOne.PlayerScripts;
using UnityEngine;

namespace ScheduleOne.Vehicles.Recording
{
	public class VehicleRecorder : MonoBehaviour
	{
		public static int frameRate = 24;

		public bool IS_RECORDING;

		public List<VehicleKeyFrame> keyFrames = new List<VehicleKeyFrame>();

		private LandVehicle vehicleToRecord;

		private float timeSinceKeyFrame;

		protected virtual void Update()
		{
			if (Input.GetKeyDown(KeyCode.P))
			{
				IS_RECORDING = !IS_RECORDING;
				if (IS_RECORDING)
				{
					keyFrames.Clear();
					vehicleToRecord = PlayerSingleton<PlayerMovement>.Instance.currentVehicle;
				}
			}
			if ((bool)vehicleToRecord && IS_RECORDING)
			{
				if (timeSinceKeyFrame >= 1f / (float)frameRate)
				{
					timeSinceKeyFrame = 0f;
					VehicleKeyFrame item = Capture();
					keyFrames.Add(item);
				}
				Console.Log(vehicleToRecord.speed_Kmh);
				timeSinceKeyFrame += Time.deltaTime;
			}
		}

		private VehicleKeyFrame Capture()
		{
			VehicleKeyFrame vehicleKeyFrame = new VehicleKeyFrame();
			vehicleKeyFrame.position = vehicleToRecord.transform.position;
			vehicleKeyFrame.rotation = vehicleToRecord.transform.rotation;
			vehicleKeyFrame.brakesApplied = vehicleToRecord.brakesApplied;
			vehicleKeyFrame.reversing = vehicleToRecord.isReversing;
			if ((bool)vehicleToRecord.GetComponent<VehicleLights>())
			{
				vehicleKeyFrame.headlightsOn = vehicleToRecord.GetComponent<VehicleLights>().headLightsOn;
			}
			foreach (Wheel wheel in vehicleToRecord.wheels)
			{
				vehicleKeyFrame.wheels.Add(CaptureWheel(wheel));
			}
			return vehicleKeyFrame;
		}

		private VehicleKeyFrame.WheelTransform CaptureWheel(Wheel wheel)
		{
			return new VehicleKeyFrame.WheelTransform
			{
				yPos = wheel.transform.Find("Model").transform.localPosition.y,
				rotation = wheel.transform.Find("Model").transform.localRotation
			};
		}
	}
}
