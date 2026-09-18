using TMPro;
using UnityEngine;

namespace ScheduleOne.DevUtilities
{
	[ExecuteInEditMode]
	public class PlaceholderBuilding : MonoBehaviour
	{
		[Header("Settings")]
		public string Name;

		public Vector3 Dimensions;

		public bool AutoGround = true;

		[Header("References")]
		public Transform Model;

		public TextMeshPro Label;

		private Vector3 lastFramePosition = Vector3.zero;

		private void Awake()
		{
			if (Application.isPlaying)
			{
				Model.GetComponent<Collider>().enabled = true;
			}
		}

		protected virtual void LateUpdate()
		{
			if (Application.isPlaying)
			{
				return;
			}
			base.gameObject.name = "Placeholder (" + Name + ")";
			Label.text = Name;
			Model.localScale = Dimensions;
			if (base.transform.position != lastFramePosition)
			{
				if (AutoGround && Physics.Raycast(base.transform.position + Vector3.up * 50f, Vector3.down, out var hitInfo, 100f, 1 << LayerMask.NameToLayer("Default")))
				{
					Model.transform.position = new Vector3(Model.transform.position.x, hitInfo.point.y + Dimensions.y / 2f, Model.transform.position.z);
				}
				lastFramePosition = base.transform.position;
			}
			Label.transform.position = new Vector3(Label.transform.position.x, Model.transform.position.y + Dimensions.y / 2f + 0.1f, Label.transform.position.z);
		}
	}
}
