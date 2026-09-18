using UnityEngine;

namespace ScheduleOne.AvatarFramework.Equipping
{
	[RequireComponent(typeof(AvatarEquippable))]
	public class AvatarEquippableLookAt : MonoBehaviour
	{
		public int Priority;

		private Avatar avatar;

		private void Start()
		{
			avatar = GetComponentInParent<Avatar>();
			if (avatar == null)
			{
				Debug.LogError("AvatarEquippableLookAt must be a child of an Avatar object.");
			}
		}

		private void Update()
		{
			if (!(avatar == null))
			{
				avatar.LookController.OverrideLookTarget(avatar.CurrentEquippable.transform.position, Priority);
			}
		}
	}
}
