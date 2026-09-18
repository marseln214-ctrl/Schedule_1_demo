using ScheduleOne.Packaging;
using ScheduleOne.Product;
using UnityEngine;

namespace ScheduleOne.ObjectScripts
{
	public class BrickPressContainer : MonoBehaviour
	{
		public FilledPackagingVisuals Visuals;

		public Transform ContentsContainer;

		public Transform Contents_Min;

		public Transform Contents_Max;

		public void SetContents(ProductItemInstance product, float fillLevel)
		{
			fillLevel = Mathf.Clamp01(fillLevel);
			if (product == null || fillLevel == 0f)
			{
				ContentsContainer.gameObject.SetActive(value: false);
				return;
			}
			product.SetupPackagingVisuals(Visuals);
			ContentsContainer.localPosition = Vector3.Lerp(Contents_Min.localPosition, Contents_Max.localPosition, fillLevel);
			ContentsContainer.gameObject.SetActive(value: true);
		}
	}
}
