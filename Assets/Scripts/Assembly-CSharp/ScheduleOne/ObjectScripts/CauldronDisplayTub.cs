using UnityEngine;

namespace ScheduleOne.ObjectScripts
{
	public class CauldronDisplayTub : MonoBehaviour
	{
		public enum EContents
		{
			None = 0,
			CocaLeaf = 1
		}

		public Transform CocaLeafContainer;

		public Transform Container_Min;

		public Transform Container_Max;

		public void Configure(EContents contentsType, float fillLevel)
		{
			CocaLeafContainer.gameObject.SetActive(value: false);
			Transform transform = null;
			if (contentsType == EContents.CocaLeaf)
			{
				transform = CocaLeafContainer;
			}
			if (transform != null)
			{
				transform.transform.localPosition = Vector3.Lerp(Container_Min.localPosition, Container_Max.localPosition, fillLevel);
				transform.gameObject.SetActive(fillLevel > 0f);
			}
		}
	}
}
