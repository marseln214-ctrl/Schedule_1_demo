using UnityEngine;

namespace ScheduleOne.Trash
{
	[RequireComponent(typeof(TrashContainer))]
	public class TrashContainerContents : MonoBehaviour
	{
		public TrashContainer TrashContainer;

		[Header("References")]
		public Transform ContentsTransform;

		public Transform VisualsContainer;

		public Transform VisualsMinTransform;

		public Transform VisualsMaxTransform;

		public Collider Collider;

		protected void Start()
		{
			TrashContainer.onTrashLevelChanged.AddListener(UpdateVisuals);
			UpdateVisuals();
		}

		private void UpdateVisuals()
		{
			float t = (float)TrashContainer.TrashLevel / (float)TrashContainer.TrashCapacity;
			ContentsTransform.transform.localPosition = Vector3.Lerp(VisualsMinTransform.localPosition, VisualsMaxTransform.localPosition, t);
			ContentsTransform.transform.localScale = Vector3.Lerp(VisualsMinTransform.localScale, VisualsMaxTransform.localScale, t);
			VisualsContainer.gameObject.SetActive(TrashContainer.TrashLevel > 0);
			Collider.enabled = TrashContainer.TrashLevel >= TrashContainer.TrashCapacity;
		}
	}
}
