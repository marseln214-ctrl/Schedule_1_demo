using System;
using FluffyUnderware.Curvy.Controllers;
using FluffyUnderware.DevTools;
using UnityEngine;

namespace FluffyUnderware.Curvy.Examples
{
	public class E01_MetaDataController : SplineController
	{
		[Section("MetaController", true, false, 100, Sort = 0)]
		[RangeEx(0f, 30f, "", "")]
		[SerializeField]
		private float m_MaxHeight = 5f;

		public float MaxHeight
		{
			get
			{
				return m_MaxHeight;
			}
			set
			{
				if (m_MaxHeight != value)
				{
					m_MaxHeight = value;
				}
			}
		}

		protected override void UserAfterInit()
		{
			setHeight();
		}

		protected override void UserAfterUpdate()
		{
			setHeight();
		}

		private void setHeight()
		{
			if (Spline.Dirty)
			{
				Spline.Refresh();
			}
			float interpolatedMetadata = Spline.GetInterpolatedMetadata<E01_HeightMetadata, float>(base.RelativePosition);
			if (base.TargetComponent != 0)
			{
				throw new NotSupportedException(string.Format("Only controllers with {0} set to {1} are supported", "TargetComponent", TargetComponent.Transform));
			}
			base.transform.Translate(0f, interpolatedMetadata * MaxHeight, 0f, Space.Self);
		}
	}
}
