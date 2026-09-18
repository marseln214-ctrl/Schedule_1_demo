using ScheduleOne.PlayerScripts;
using UnityEngine;

namespace ScheduleOne.DevUtilities
{
	public class LightOptimizer : MonoBehaviour
	{
		public bool LightsEnabled = true;

		[Header("References")]
		[SerializeField]
		protected BoxCollider[] activationZones;

		[SerializeField]
		protected Transform[] viewPoints;

		[Header("Settings")]
		public float checkRange = 50f;

		protected OptimizedLight[] lights;

		public void Awake()
		{
			lights = GetComponentsInChildren<OptimizedLight>();
		}

		public void FixedUpdate()
		{
			if (!PlayerSingleton<PlayerCamera>.InstanceExists)
			{
				return;
			}
			OptimizedLight[] array;
			if (Vector3.Distance(base.transform.position, PlayerSingleton<PlayerCamera>.Instance.transform.position) > checkRange)
			{
				array = lights;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].DisabledForOptimization = true;
				}
				return;
			}
			if (activationZones.Length == 0 && viewPoints.Length == 0)
			{
				ApplyLights();
				return;
			}
			BoxCollider[] array2 = activationZones;
			for (int i = 0; i < array2.Length; i++)
			{
				if (array2[i].bounds.Contains(PlayerSingleton<PlayerCamera>.Instance.transform.position))
				{
					ApplyLights();
					return;
				}
			}
			GeometryUtility.CalculateFrustumPlanes(PlayerSingleton<PlayerCamera>.Instance.Camera);
			Transform[] array3 = viewPoints;
			foreach (Transform transform in array3)
			{
				if (PointInCameraView(transform.position))
				{
					ApplyLights();
					return;
				}
			}
			array = lights;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].DisabledForOptimization = true;
			}
		}

		public void ApplyLights()
		{
			OptimizedLight[] array = lights;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].DisabledForOptimization = false;
			}
		}

		public bool PointInCameraView(Vector3 point)
		{
			Camera camera = PlayerSingleton<PlayerCamera>.Instance.Camera;
			bool num = camera.WorldToViewportPoint(point).z > -1f;
			bool flag = false;
			if (Physics.Raycast(direction: (point - camera.transform.position).normalized, maxDistance: Vector3.Distance(camera.transform.position, point) + 0.05f, origin: camera.transform.position, hitInfo: out var hitInfo, layerMask: 1 << LayerMask.NameToLayer("Default")) && hitInfo.point != point)
			{
				flag = true;
			}
			if (num)
			{
				return !flag;
			}
			return false;
		}

		public bool Is01(float a)
		{
			if (a > 0f)
			{
				return a < 1f;
			}
			return false;
		}

		public void LightsEnabled_True()
		{
			LightsEnabled = true;
		}

		public void LightsEnabled_False()
		{
			LightsEnabled = false;
		}
	}
}
