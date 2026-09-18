using ScheduleOne.Construction.Features;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.UI.Construction.Features
{
	public class FI_Base : MonoBehaviour
	{
		protected Feature feature;

		public UnityEvent onClose;

		public virtual void Initialize(Feature _feature)
		{
			feature = _feature;
		}

		public virtual void Close()
		{
			if (onClose != null)
			{
				onClose.Invoke();
			}
			Object.Destroy(base.gameObject);
		}
	}
}
