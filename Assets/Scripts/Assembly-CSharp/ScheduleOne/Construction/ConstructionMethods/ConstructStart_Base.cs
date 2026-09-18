using ScheduleOne.ConstructableScripts;
using UnityEngine;

namespace ScheduleOne.Construction.ConstructionMethods
{
	public abstract class ConstructStart_Base : MonoBehaviour
	{
		public virtual void StartConstruction(string constructableID, Constructable_GridBased movedConstructable = null)
		{
			if (movedConstructable != null)
			{
				base.gameObject.GetComponent<ConstructUpdate_Base>().MovedConstructable = movedConstructable;
				movedConstructable.SetInvisible();
			}
		}
	}
}
