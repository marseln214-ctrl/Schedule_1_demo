using UnityEngine;

namespace ScheduleOne.Construction.ConstructionMethods
{
	public class ConstructStop_Base : MonoBehaviour
	{
		public virtual void StopConstruction()
		{
			GetComponent<ConstructUpdate_Base>().ConstructionStop();
			Object.Destroy(base.gameObject);
		}
	}
}
