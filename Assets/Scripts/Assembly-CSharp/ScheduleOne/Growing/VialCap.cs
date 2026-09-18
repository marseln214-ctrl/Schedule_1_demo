using ScheduleOne.PlayerTasks;
using UnityEngine;

namespace ScheduleOne.Growing
{
	public class VialCap : Clickable
	{
		public Collider Collider;

		private Rigidbody RigidBody;

		public bool Removed { get; protected set; }

		public override void StartClick(RaycastHit hit)
		{
			base.StartClick(hit);
			Pop();
		}

		private void Pop()
		{
			RigidBody = base.gameObject.AddComponent<Rigidbody>();
			Removed = true;
			Collider.enabled = false;
			RigidBody.isKinematic = false;
			RigidBody.useGravity = true;
			base.transform.SetParent(null);
			RigidBody.AddRelativeForce(Vector3.forward * 1.5f, ForceMode.VelocityChange);
			RigidBody.AddRelativeForce(Vector3.up * 0.5f, ForceMode.VelocityChange);
			RigidBody.AddTorque(Vector3.up * 1.5f, ForceMode.VelocityChange);
			Object.Destroy(base.gameObject, 3f);
			base.enabled = false;
		}
	}
}
