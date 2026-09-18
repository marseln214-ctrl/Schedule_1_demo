using System;
using UnityEngine;

namespace FluffyUnderware.Curvy
{
	public class CameraFrustumPlanesProvider
	{
		private static readonly Lazy<CameraFrustumPlanesProvider> instance = new Lazy<CameraFrustumPlanesProvider>(() => new CameraFrustumPlanesProvider());

		private static object lockObject = new object();

		private readonly Plane[] cachedPlanes = new Plane[6];

		private Vector3 cachedPosition = new Vector3(float.NaN, float.NaN, float.NaN);

		private Vector3 cachedForward = new Vector3(float.NaN, float.NaN, float.NaN);

		private float cachedFov = float.NaN;

		private int cachedPixelWidth = -1;

		private int cachedPixelHeight = -1;

		public static CameraFrustumPlanesProvider Instance => instance.Value;

		public Plane[] GetFrustumPlanes(Camera camera)
		{
			Transform transform = camera.transform;
			Vector3 position = transform.position;
			Vector3 forward = transform.forward;
			int pixelWidth = camera.pixelWidth;
			int pixelHeight = camera.pixelHeight;
			float fieldOfView = camera.fieldOfView;
			if (!IsCacheOutdated(position, forward, pixelWidth, pixelHeight, fieldOfView))
			{
				return cachedPlanes;
			}
			lock (lockObject)
			{
				if (IsCacheOutdated(position, forward, pixelWidth, pixelHeight, fieldOfView))
				{
					cachedPosition = position;
					cachedForward = forward;
					cachedPixelWidth = pixelWidth;
					cachedPixelHeight = pixelHeight;
					cachedFov = fieldOfView;
					GeometryUtility.CalculateFrustumPlanes(camera, cachedPlanes);
				}
			}
			return cachedPlanes;
		}

		private bool IsCacheOutdated(Vector3 cameraPosition, Vector3 cameraZDirection, int cameraPixelWidth, int cameraPixelHeight, float cameraFieldOfView)
		{
			if (!(cachedPosition != cameraPosition) && !(cachedForward != cameraZDirection) && cachedPixelWidth == cameraPixelWidth && cachedPixelHeight == cameraPixelHeight)
			{
				return !Mathf.Approximately(cachedFov, cameraFieldOfView);
			}
			return true;
		}
	}
}
