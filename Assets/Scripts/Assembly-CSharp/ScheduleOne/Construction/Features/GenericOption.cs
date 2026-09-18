using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.Construction.Features
{
	public class GenericOption : MonoBehaviour
	{
		[Header("Interface settings")]
		public string optionName;

		public Color optionButtonColor;

		public float optionPrice;

		[Header("Events")]
		public UnityEvent onInstalled;

		public UnityEvent onUninstalled;

		public UnityEvent onSetVisible;

		public UnityEvent onSetInvisible;

		public virtual void Install()
		{
			if (onInstalled != null)
			{
				onInstalled.Invoke();
			}
			SetVisible();
		}

		public virtual void Uninstall()
		{
			if (onUninstalled != null)
			{
				onUninstalled.Invoke();
			}
			SetInvisible();
		}

		public virtual void SetVisible()
		{
			if (onSetVisible != null)
			{
				onSetVisible.Invoke();
			}
		}

		public virtual void SetInvisible()
		{
			if (onSetInvisible != null)
			{
				onSetInvisible.Invoke();
			}
		}
	}
}
