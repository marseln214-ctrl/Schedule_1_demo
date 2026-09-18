using System;
using FluffyUnderware.DevTools;
using JetBrains.Annotations;
using UnityEngine;

namespace FluffyUnderware.Curvy
{
	[ExecuteAlways]
	public abstract class SplineProcessor : DTVersionedMonoBehaviour
	{
		[SerializeField]
		protected CurvySpline m_Spline;

		public CurvySpline Spline
		{
			get
			{
				return m_Spline;
			}
			set
			{
				if (m_Spline != value)
				{
					UnbindEvents();
					m_Spline = value;
					if (base.IsActiveAndEnabled)
					{
						BindEvents();
						Refresh();
					}
				}
			}
		}

		public abstract void Refresh();

		private void OnSplineRefresh(CurvySplineEventArgs e)
		{
			ProcessEvent(e.Spline);
		}

		private void OnSplineCoordinatesChanged(CurvySpline spline)
		{
			ProcessEvent(spline);
		}

		private void ProcessEvent([NotNull] CurvySpline spline)
		{
			if (Spline != spline)
			{
				UnbindEvents(spline);
			}
			else if (base.IsActiveAndEnabled)
			{
				Refresh();
			}
		}

		[UsedImplicitly]
		protected virtual void Awake()
		{
			if (m_Spline == null)
			{
				m_Spline = GetComponent<CurvySpline>();
				if ((object)m_Spline != null)
				{
					DTLog.Log($"[Curvy] Spline '{base.name}' was assigned to the {GetType().Name} by default.", this);
				}
			}
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			UnbindEvents();
			BindEvents();
			Refresh();
		}

		protected override void OnDisable()
		{
			base.OnDisable();
			UnbindEvents();
		}

		protected override void OnValidate()
		{
			base.OnValidate();
			if (base.IsActiveAndEnabled)
			{
				BindEvents();
				Refresh();
			}
		}

		[UsedImplicitly]
		protected virtual void Start()
		{
			Refresh();
		}

		protected void BindEvents()
		{
			if ((bool)Spline)
			{
				Spline.OnRefresh.AddListenerOnce(OnSplineRefresh);
				CurvySpline spline = Spline;
				spline.OnGlobalCoordinatesChanged = (Action<CurvySpline>)Delegate.Remove(spline.OnGlobalCoordinatesChanged, new Action<CurvySpline>(OnSplineCoordinatesChanged));
				CurvySpline spline2 = Spline;
				spline2.OnGlobalCoordinatesChanged = (Action<CurvySpline>)Delegate.Combine(spline2.OnGlobalCoordinatesChanged, new Action<CurvySpline>(OnSplineCoordinatesChanged));
			}
		}

		protected void UnbindEvents()
		{
			if ((bool)Spline)
			{
				UnbindEvents(Spline);
			}
		}

		private void UnbindEvents([NotNull] CurvySpline spline)
		{
			spline.OnRefresh.RemoveListener(OnSplineRefresh);
			spline.OnGlobalCoordinatesChanged = (Action<CurvySpline>)Delegate.Remove(spline.OnGlobalCoordinatesChanged, new Action<CurvySpline>(OnSplineCoordinatesChanged));
		}
	}
}
