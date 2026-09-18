using UnityEngine;

namespace ScheduleOne.ObjectScripts.HandheldBin
{
	public class HandheldBin_Functional : MonoBehaviour
	{
		[Header("References")]
		public Transform trash;

		[Header("Settings")]
		public float trash_MinY;

		public float trash_MaxY;

		public float fillLevel { get; protected set; }

		protected virtual void Awake()
		{
			UpdateTrashVisuals();
		}

		public void SetAmount(float amount)
		{
			UpdateTrashVisuals();
		}

		protected virtual void UpdateTrashVisuals()
		{
			trash.gameObject.SetActive(fillLevel > 0f);
		}
	}
}
