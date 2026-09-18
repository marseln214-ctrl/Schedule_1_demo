using System.Collections;
using UnityEngine;

namespace MTAssets.UltimateLODSystem
{
	[AddComponentMenu("")]
	public class RuntimeCameraDetector : MonoBehaviour
	{
		private WaitForSecondsRealtime DELAY_BETWEEN_ARRAY_OF_CAMERAS_UPDATE = new WaitForSecondsRealtime(0.5f);

		private WaitForSecondsRealtime DELAY_BETWEEN_CURRENT_CAMERA_DETECTOR = new WaitForSecondsRealtime(0.09f);

		private Camera[] currentArrayOfCameras = new Camera[0];

		public void Awake()
		{
			currentArrayOfCameras = Camera.allCameras;
			StartCoroutine(ArrayOfCamerasDelayedUpdater());
			StartCoroutine(CurrentCameraOnScreenDetector());
		}

		private IEnumerator ArrayOfCamerasDelayedUpdater()
		{
			while (true)
			{
				yield return DELAY_BETWEEN_ARRAY_OF_CAMERAS_UPDATE;
				currentArrayOfCameras = Camera.allCameras;
			}
		}

		private IEnumerator CurrentCameraOnScreenDetector()
		{
			while (true)
			{
				int num = -1;
				float num2 = -1000f;
				for (int i = 0; i < currentArrayOfCameras.Length; i++)
				{
					if (!(currentArrayOfCameras[i] == null) && !(currentArrayOfCameras[i].targetTexture != null) && !currentArrayOfCameras[i].orthographic && currentArrayOfCameras[i].rect.width == 1f && currentArrayOfCameras[i].rect.height == 1f)
					{
						if (num2 == currentArrayOfCameras[i].depth)
						{
							Debug.LogError("Ultimate LOD System: There are one or more cameras active in this scene, which have the same depth level. This causes the camera detection algorithm that is currently appearing on the screen to not work. Please set different depth values for each camera that is active at the same time in your scene, or disable cameras that are not being used and leave only the cameras that are being used, enabled.");
						}
						if (currentArrayOfCameras[i].depth > num2)
						{
							num = i;
							num2 = currentArrayOfCameras[i].depth;
						}
					}
				}
				if (num == -1)
				{
					UltimateLevelOfDetailGlobal.currentCameraThatIsOnTopOfScreenInThisScene = null;
				}
				if (num != -1)
				{
					UltimateLevelOfDetailGlobal.currentCameraThatIsOnTopOfScreenInThisScene = currentArrayOfCameras[num];
				}
				yield return DELAY_BETWEEN_CURRENT_CAMERA_DETECTOR;
			}
		}
	}
}
