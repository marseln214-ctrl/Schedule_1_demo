using System;
using System.Collections.Generic;
using ScheduleOne.DevUtilities;
using ScheduleOne.GameTime;
using ScheduleOne.PlayerScripts;
using UnityEngine;

namespace ScheduleOne.Misc
{
	public class TreeScaler : MonoBehaviour
	{
		[Header("References")]
		[SerializeField]
		protected List<Transform> branchMeshes = new List<Transform>();

		public float minScale = 1f;

		public float maxScale = 1.3f;

		public float minScaleDistance = 20f;

		public float maxScaleDistance = 100f;

		protected virtual void Start()
		{
			TimeManager instance = NetworkSingleton<TimeManager>.Instance;
			instance.onMinutePass = (Action)Delegate.Combine(instance.onMinutePass, new Action(UpdateScale));
		}

		private void UpdateScale()
		{
			float num = Mathf.Clamp(Vector3.Distance(base.transform.position, PlayerSingleton<PlayerCamera>.Instance.transform.position), minScaleDistance, maxScaleDistance) / (maxScaleDistance - minScaleDistance);
			float num2 = minScale + (maxScale - minScale) * num;
			foreach (Transform branchMesh in branchMeshes)
			{
				branchMesh.localScale = new Vector3(num2, 1f, num2);
			}
		}
	}
}
