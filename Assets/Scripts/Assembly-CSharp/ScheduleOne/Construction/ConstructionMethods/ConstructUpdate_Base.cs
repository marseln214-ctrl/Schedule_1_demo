using ScheduleOne.ConstructableScripts;
using UnityEngine;

namespace ScheduleOne.Construction.ConstructionMethods
{
	public class ConstructUpdate_Base : MonoBehaviour
	{
		public Constructable_GridBased MovedConstructable;

		public bool isMoving => MovedConstructable != null;

		protected virtual void Update()
		{
		}

		protected virtual void LateUpdate()
		{
		}

		public virtual void ConstructionStop()
		{
			if (MovedConstructable != null)
			{
				MovedConstructable.RestoreVisibility();
			}
		}
	}
}
