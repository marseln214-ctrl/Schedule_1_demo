using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using FluffyUnderware.DevTools;
using FluffyUnderware.DevTools.Extensions;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FluffyUnderware.Curvy
{
	[ExecuteInEditMode]
	[HelpURL("https://curvyeditor.com/doclink/curvyconnection")]
	public class CurvyConnection : DTVersionedMonoBehaviour, ISerializationCallbackReceiver
	{
		private class TransformSynchronizer
		{
			[NotNull]
			private readonly CurvyConnection connection;

			[CanBeNull]
			private TransformMonitor connectionMonitor;

			[NotNull]
			private readonly Dictionary<CurvySplineSegment, (Vector3, Quaternion)> monitoredCPCoordinated = new Dictionary<CurvySplineSegment, (Vector3, Quaternion)>();

			[NotNull]
			private TransformMonitor ConnectionMonitor
			{
				get
				{
					if (connectionMonitor == null)
					{
						connectionMonitor = new TransformMonitor(connection.transform, monitorPosition: true, monitorRotation: true, monitorScale: false);
					}
					return connectionMonitor;
				}
			}

			private bool IsCPsMonitorValid
			{
				get
				{
					if (monitoredCPCoordinated.Count != connection.Count)
					{
						return false;
					}
					return connection.ControlPointsList.All((CurvySplineSegment controlPoint) => monitoredCPCoordinated.ContainsKey(controlPoint));
				}
			}

			public TransformSynchronizer([NotNull] CurvyConnection connection)
			{
				this.connection = connection;
			}

			public void OnControlPointsUpdated()
			{
				ResetCPsMonitoring();
			}

			public void OnUpdate()
			{
				EnsureCPsMonitorIsValid();
				GetMonitorChanges(out var positionChange, out var rotationChange);
				if (positionChange.HasValue || rotationChange.HasValue)
				{
					ApplyTransform(positionChange ?? connection.transform.position, rotationChange ?? connection.transform.rotation);
				}
			}

			private void EnsureCPsMonitorIsValid()
			{
				if (!IsCPsMonitorValid)
				{
					ResetCPsMonitoring();
				}
			}

			private void GetMonitorChanges(out Vector3? positionChange, out Quaternion? rotationChange)
			{
				if (!GetConnectionMonitorChanges(out positionChange, out rotationChange))
				{
					GetCPsMonitorChanges(out positionChange, out rotationChange);
				}
			}

			private bool GetConnectionMonitorChanges(out Vector3? positionChange, out Quaternion? rotationChange)
			{
				bool num = ConnectionMonitor.CheckForChanges();
				if (num)
				{
					positionChange = connection.transform.position;
					rotationChange = connection.transform.rotation;
					return num;
				}
				positionChange = null;
				rotationChange = null;
				return num;
			}

			private void GetCPsMonitorChanges(out Vector3? position, out Quaternion? rotation)
			{
				position = null;
				rotation = null;
				bool flag = false;
				bool flag2 = false;
				foreach (CurvySplineSegment controlPoints in connection.ControlPointsList)
				{
					if (controlPoints.gameObject == null)
					{
						DTLog.LogError("[Curvy] Connection named '" + connection.name + "' had in its list a control point with no game object. Control point was ignored", connection);
						continue;
					}
					GetCPMonitorChanges(controlPoints, out var position2, out var rotation2);
					if (position2.HasValue)
					{
						flag = true;
						position = position2;
					}
					if (rotation2.HasValue)
					{
						flag2 = true;
						rotation = rotation2;
					}
					if (!(flag && flag2))
					{
						continue;
					}
					break;
				}
			}

			private void GetCPMonitorChanges([NotNull] CurvySplineSegment controlPoint, out Vector3? position, out Quaternion? rotation)
			{
				IsCPTriggeringTransformChange(controlPoint, out var syncPosition, out var syncRotation);
				position = (syncPosition ? new Vector3?(controlPoint.transform.position) : null);
				rotation = (syncRotation ? new Quaternion?(controlPoint.transform.rotation) : null);
			}

			private void IsCPTriggeringTransformChange([NotNull] CurvySplineSegment controlPoint, out bool syncPosition, out bool syncRotation)
			{
				if (!controlPoint.ConnectionSyncPosition && !controlPoint.ConnectionSyncRotation)
				{
					syncPosition = false;
					syncRotation = false;
					return;
				}
				(Vector3, Quaternion) tuple = monitoredCPCoordinated[controlPoint];
				Transform transform = controlPoint.transform;
				syncPosition = controlPoint.ConnectionSyncPosition && transform.position.NotApproximately(tuple.Item1);
				syncRotation = controlPoint.ConnectionSyncRotation && transform.rotation.DifferentOrientation(tuple.Item2);
			}

			public void ApplyTransform(Vector3 position, Quaternion rotation)
			{
				ApplyTransformToConnection(position, rotation);
				ApplyTransformToCPs(position, rotation);
				ResetMonitoring();
			}

			private void ApplyTransformToConnection(Vector3 position, Quaternion rotation)
			{
				Transform transform = connection.transform;
				transform.position = position;
				transform.rotation = rotation;
			}

			private void ApplyTransformToCPs(Vector3 referencePosition, Quaternion referenceRotation)
			{
				for (int i = 0; i < connection.Count; i++)
				{
					CurvySplineSegment curvySplineSegment = connection.ControlPointsList[i];
					bool flag = curvySplineSegment.ConnectionSyncPosition && curvySplineSegment.transform.position.NotApproximately(referencePosition);
					bool flag2 = curvySplineSegment.ConnectionSyncRotation && curvySplineSegment.transform.rotation.DifferentOrientation(referenceRotation);
					if (flag)
					{
						curvySplineSegment.transform.position = referencePosition;
					}
					if (flag2)
					{
						curvySplineSegment.transform.rotation = referenceRotation;
					}
					if (flag || (flag2 && curvySplineSegment.OrientationInfluencesSpline))
					{
						if (curvySplineSegment.Spline == null)
						{
							throw new InvalidOperationException("[Curvy] Control point named '" + curvySplineSegment.name + "' has no spline. Please raise a bug report");
						}
						curvySplineSegment.Spline.SetDirtyPartial(curvySplineSegment, flag ? SplineDirtyingType.Everything : SplineDirtyingType.OrientationOnly);
					}
				}
			}

			public void ResetMonitoring()
			{
				ResetConnectionMonitoring();
				ResetCPsMonitoring();
			}

			[UsedImplicitly]
			private void ResetConnectionMonitoring()
			{
				ConnectionMonitor.ResetMonitoring();
			}

			[UsedImplicitly]
			private void ResetCPsMonitoring()
			{
				monitoredCPCoordinated.Clear();
				foreach (CurvySplineSegment controlPoints in connection.ControlPointsList)
				{
					monitoredCPCoordinated[controlPoints] = (controlPoints.transform.position, controlPoints.transform.rotation);
				}
			}
		}

		private class UndoFixer
		{
			private readonly CurvyConnection curvyConnection;

			public UndoFixer(CurvyConnection curvyConnection)
			{
				this.curvyConnection = curvyConnection;
			}

			public void FixIssuesIntroducedByUndoing()
			{
			}
		}

		[SerializeField]
		[Hide]
		private List<CurvySplineSegment> m_ControlPoints = new List<CurvySplineSegment>();

		private ReadOnlyCollection<CurvySplineSegment> readOnlyControlPoints;

		[NotNull]
		private readonly TransformSynchronizer transformSynchronizer;

		[NotNull]
		private readonly UndoFixer undoFixer;

		public ReadOnlyCollection<CurvySplineSegment> ControlPointsList
		{
			get
			{
				if (readOnlyControlPoints == null)
				{
					readOnlyControlPoints = m_ControlPoints.AsReadOnly();
				}
				return readOnlyControlPoints;
			}
		}

		public int Count => m_ControlPoints.Count;

		public CurvySplineSegment this[int idx] => m_ControlPoints[idx];

		public CurvyConnection()
		{
			transformSynchronizer = new TransformSynchronizer(this);
			undoFixer = new UndoFixer(this);
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			SceneManager.sceneLoaded += OnSceneLoaded;
			transformSynchronizer.ResetMonitoring();
		}

		protected override void OnDisable()
		{
			base.OnDisable();
			SceneManager.sceneLoaded -= OnSceneLoaded;
		}

		[UsedImplicitly]
		private void Update()
		{
			if (Application.isPlaying)
			{
				DoUpdate();
			}
		}

		[UsedImplicitly]
		private void LateUpdate()
		{
			if (Application.isPlaying)
			{
				DoUpdate();
			}
		}

		[UsedImplicitly]
		private void FixedUpdate()
		{
			if (Application.isPlaying)
			{
				DoUpdate();
			}
		}

		[UsedImplicitly]
		private void OnDestroy()
		{
			foreach (CurvySplineSegment item in new List<CurvySplineSegment>(m_ControlPoints))
			{
				item.Disconnect(destroyEmptyConnection: false);
			}
			m_ControlPoints.Clear();
			transformSynchronizer.OnControlPointsUpdated();
		}

		public static CurvyConnection Create(params CurvySplineSegment[] controlPoints)
		{
			CurvyGlobalManager instance = DTSingleton<CurvyGlobalManager>.Instance;
			if (instance == null)
			{
				DTLog.LogError("[Curvy] Couldn't find Curvy Global Manager. Please raise a bug report.");
				return null;
			}
			GameObject obj = new GameObject("Connection");
			obj.transform.UndoableSetParent(instance.transform, worldPositionStays: true, "Add Connection");
			CurvyConnection curvyConnection = obj.UndoableAddComponent<CurvyConnection>();
			if (curvyConnection == null)
			{
				return null;
			}
			if (controlPoints.Length == 0)
			{
				return curvyConnection;
			}
			curvyConnection.transform.position = controlPoints[0].transform.position;
			curvyConnection.AddControlPoints(controlPoints);
			return curvyConnection;
		}

		public void AddControlPoints(params CurvySplineSegment[] controlPoints)
		{
			foreach (CurvySplineSegment curvySplineSegment in controlPoints)
			{
				if ((bool)curvySplineSegment.Connection)
				{
					DTLog.LogErrorFormat(this, "[Curvy] CurvyConnection.AddControlPoints called on a control point '{0}' that has already a connection. Only control points with no connection can be added.", curvySplineSegment);
				}
				else
				{
					m_ControlPoints.Add(curvySplineSegment);
					curvySplineSegment.Connection = this;
				}
			}
			transformSynchronizer.OnControlPointsUpdated();
			AutoSetFollowUp();
		}

		public void AutoSetFollowUp()
		{
			if (Count != 2)
			{
				return;
			}
			CurvySplineSegment curvySplineSegment = m_ControlPoints[0];
			CurvySplineSegment curvySplineSegment2 = m_ControlPoints[1];
			if (curvySplineSegment.transform.position == curvySplineSegment2.transform.position && curvySplineSegment.ConnectionSyncPosition && curvySplineSegment2.ConnectionSyncPosition)
			{
				if (curvySplineSegment.FollowUp == null && (bool)curvySplineSegment.Spline && curvySplineSegment.Spline.CanControlPointHaveFollowUp(curvySplineSegment))
				{
					curvySplineSegment.SetFollowUp(curvySplineSegment2);
				}
				if (curvySplineSegment2.FollowUp == null && (bool)curvySplineSegment2.Spline && curvySplineSegment2.Spline.CanControlPointHaveFollowUp(curvySplineSegment2))
				{
					curvySplineSegment2.SetFollowUp(curvySplineSegment);
				}
			}
		}

		public void RemoveControlPoint(CurvySplineSegment controlPoint, bool destroySelfIfEmpty = true)
		{
			controlPoint.Connection = null;
			m_ControlPoints.Remove(controlPoint);
			transformSynchronizer.OnControlPointsUpdated();
			foreach (CurvySplineSegment controlPoint2 in m_ControlPoints)
			{
				if (controlPoint2.FollowUp == controlPoint)
				{
					controlPoint2.SetFollowUp(null);
				}
			}
			if (m_ControlPoints.Count == 0 && destroySelfIfEmpty)
			{
				Delete();
			}
		}

		public void Delete()
		{
			base.gameObject.Destroy(isUndoable: true, doPrefabCheck: true);
		}

		[Obsolete("Inline the method's body if needed")]
		public List<CurvySplineSegment> OtherControlPoints(CurvySplineSegment source)
		{
			return ControlPointsList.Where((CurvySplineSegment cp) => cp != source).ToList();
		}

		public void SetSynchronisationPositionAndRotation(Vector3 referencePosition, Quaternion referenceRotation)
		{
			transformSynchronizer.ApplyTransform(referencePosition, referenceRotation);
		}

		public void OnBeforeSerialize()
		{
			RemoveNullCPs();
		}

		public void OnAfterDeserialize()
		{
			RemoveNullCPs();
		}

		private void RemoveNullCPs()
		{
			m_ControlPoints.RemoveAll((CurvySplineSegment cp) => (object)cp == null);
		}

		private void DoUpdate()
		{
			transformSynchronizer.OnUpdate();
		}

		private void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
		{
			if (m_ControlPoints.RemoveAll((CurvySplineSegment cp) => cp == null) != 0)
			{
				if (m_ControlPoints.Count == 0)
				{
					Delete();
					return;
				}
				DTLog.LogWarning("[Curvy] Connection " + base.name + " was not destroyed after scene switch. That should not happen. Please raise a bug report.", this);
				transformSynchronizer.ResetMonitoring();
			}
		}

		protected override void ResetOnEnable()
		{
			base.ResetOnEnable();
			readOnlyControlPoints = null;
		}
	}
}
