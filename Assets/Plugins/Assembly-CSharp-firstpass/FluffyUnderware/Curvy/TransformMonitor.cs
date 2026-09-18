using System;
using FluffyUnderware.DevTools.Extensions;
using JetBrains.Annotations;
using UnityEngine;

namespace FluffyUnderware.Curvy
{
	public class TransformMonitor
	{
		[NotNull]
		private readonly Transform transform;

		private Vector3 lastCheckedPosition;

		private Quaternion lastCheckedRotation;

		private Vector3 lastCheckedScale;

		private readonly bool monitorPosition;

		private readonly bool monitorRotation;

		private readonly bool monitorScale;

		public bool HasChanged { get; private set; }

		public TransformMonitor([NotNull] Transform transformToTrack, bool monitorPosition, bool monitorRotation, bool monitorScale)
		{
			if (transformToTrack == null)
			{
				throw new ArgumentNullException("transformToTrack");
			}
			if (!monitorPosition && !monitorRotation && !monitorScale)
			{
				throw new ArgumentException("TransformMonitor has been initialized with no tracking enabled");
			}
			transform = transformToTrack;
			this.monitorPosition = monitorPosition;
			this.monitorRotation = monitorRotation;
			this.monitorScale = monitorScale;
			ResetMonitoring();
		}

		public void ResetMonitoring()
		{
			HasChanged = false;
			MarkCurrentTransformAsChecked();
		}

		public bool CheckForChanges()
		{
			bool flag;
			if (transform.hasChanged)
			{
				transform.hasChanged = false;
				flag = HaveGlobalCoordinatesChanged();
				if (flag)
				{
					MarkCurrentTransformAsChecked();
				}
			}
			else
			{
				flag = false;
			}
			HasChanged = flag;
			return HasChanged;
		}

		private bool HaveGlobalCoordinatesChanged()
		{
			if ((!monitorPosition || !transform.position.NotApproximately(lastCheckedPosition)) && (!monitorRotation || !transform.rotation.DifferentOrientation(lastCheckedRotation)))
			{
				if (monitorScale)
				{
					return transform.lossyScale != lastCheckedScale;
				}
				return false;
			}
			return true;
		}

		private void MarkCurrentTransformAsChecked()
		{
			lastCheckedPosition = transform.position;
			lastCheckedRotation = transform.rotation;
			lastCheckedScale = transform.lossyScale;
		}
	}
}
