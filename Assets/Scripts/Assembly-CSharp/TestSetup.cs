using UnityEngine;

public class TestSetup : MonoBehaviour
{
	private void Start()
	{
		QualitySettings.vSyncCount = 0;
		Application.targetFrameRate = 3000;
		Screen.SetResolution(1920, 1080, fullscreen: true);
	}

	private void Update()
	{
		Debug.Log(1f / Time.deltaTime);
		QualitySettings.vSyncCount = 0;
		Application.targetFrameRate = 3000;
		if (Input.GetKeyDown(KeyCode.A))
		{
			Debug.Log("A");
			Screen.SetResolution(1920, 1080, fullscreen: true);
		}
	}
}
