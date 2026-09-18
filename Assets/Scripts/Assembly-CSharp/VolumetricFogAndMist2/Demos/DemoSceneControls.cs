using UnityEngine;
using UnityEngine.UI;

namespace VolumetricFogAndMist2.Demos
{
	public class DemoSceneControls : MonoBehaviour
	{
		//public VolumetricFogProfile[] profiles;

		//public VolumetricFog fogVolume;

		public Text presetNameDisplay;

		private int index;

		private void Start()
		{
			SetProfile(index);
		}

		private void Update()
		{
			if (Input.GetKeyDown(KeyCode.F))
			{
				//index++;
				//if (index >= profiles.Length)
				//{
				//	index = 0;
				//}
				//SetProfile(index);
			}
			if (Input.GetKeyDown(KeyCode.T))
			{
				//fogVolume.gameObject.SetActive(!fogVolume.gameObject.activeSelf);
			}
		}

		private void SetProfile(int profileIndex)
		{
			//if (profileIndex < 2)
			//{
			//	fogVolume.transform.position = Vector3.up * 25f;
			//}
			//else
			//{
			//	fogVolume.transform.position = Vector3.zero;
			//}
			//fogVolume.profile = profiles[profileIndex];
			//presetNameDisplay.text = "Current fog preset: " + profiles[profileIndex].name;
			//fogVolume.UpdateMaterialPropertiesNow();
		}
	}
}
