using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.Tools
{
	public class ImpactDetector : MonoBehaviour
	{
		public bool DestroyScriptOnImpact;

		public UnityEvent onImpact = new UnityEvent();

		private void OnCollisionEnter(Collision collision)
		{
			onImpact.Invoke();
			if (DestroyScriptOnImpact)
			{
				Object.Destroy(this);
			}
		}
	}
}
