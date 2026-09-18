using UnityEngine;

namespace ScheduleOne.DevUtilities
{
	public class SetTransform : MonoBehaviour
	{
		[Header("Frequency Settings")]
		public bool SetOnAwake = true;

		public bool SetOnUpdate;

		public bool SetOnLateUpdate;

		[Header("Transform Settings")]
		public bool SetPosition;

		public Vector3 LocalPosition = Vector3.zero;

		public bool SetRotation;

		public Vector3 LocalRotation = Vector3.zero;

		public bool SetScale;

		public Vector3 LocalScale = Vector3.one;

		private void Awake()
		{
			if (SetOnAwake)
			{
				Set();
			}
		}

		private void Update()
		{
			if (SetOnUpdate)
			{
				Set();
			}
		}

		private void LateUpdate()
		{
			if (SetOnLateUpdate)
			{
				Set();
			}
		}

		private void Set()
		{
			if (base.gameObject.isStatic)
			{
				Console.LogWarning("SetTransform is being used on a static object.");
			}
			if (SetPosition)
			{
				base.transform.localPosition = LocalPosition;
			}
			if (SetRotation)
			{
				base.transform.localRotation = Quaternion.Euler(LocalRotation);
			}
			if (SetScale)
			{
				base.transform.localScale = LocalScale;
			}
		}
	}
}
