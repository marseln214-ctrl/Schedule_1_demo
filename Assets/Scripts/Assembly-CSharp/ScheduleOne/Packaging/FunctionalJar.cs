using ScheduleOne.ObjectScripts;
using ScheduleOne.PlayerScripts;
using ScheduleOne.PlayerTasks;
using UnityEngine;

namespace ScheduleOne.Packaging
{
	public class FunctionalJar : FunctionalPackaging
	{
		[Header("References")]
		public Draggable Lid;

		public Transform LidStartPoint;

		public Collider LidSensor;

		public Collider LidCollider;

		public GameObject FullyPackedBlocker;

		private GameObject LidObject;

		private Vector3 lidPosition = Vector3.zero;

		public override CursorManager.ECursorType HoveredCursor { get; protected set; } = CursorManager.ECursorType.Finger;


		public override void Initialize(PackagingStation _station, Transform alignment, bool align = false)
		{
			base.Initialize(_station, alignment, align);
			lidPosition = base.transform.InverseTransformPoint(Lid.transform.position);
			LidObject = Lid.gameObject;
			Lid.transform.SetParent(_station.Container);
			Lid.transform.position = LidStartPoint.position;
			Lid.transform.rotation = LidStartPoint.rotation;
			LidSensor.enabled = false;
		}

		public override void Destroy()
		{
			Object.Destroy(LidObject);
			base.Destroy();
		}

		protected override void EnableSealing()
		{
			base.EnableSealing();
			Lid.enabled = true;
			Lid.ClickableEnabled = true;
			Lid.Rb.isKinematic = false;
			LidSensor.enabled = true;
		}

		protected override void LateUpdate()
		{
			base.LateUpdate();
		}

		protected override void OnTriggerStay(Collider other)
		{
			base.OnTriggerStay(other);
			if (Lid != null && Lid.enabled && other.gameObject.name == "LidTrigger")
			{
				Seal();
			}
		}

		public override void Seal()
		{
			base.Seal();
			Lid.enabled = false;
			Lid.ClickableEnabled = false;
			Lid.transform.SetParent(base.transform);
			Object.Destroy(Lid.Rb);
			Object.Destroy(Lid);
			Object.Destroy(LidCollider);
			Lid.transform.position = base.transform.TransformPoint(lidPosition);
			LidSensor.enabled = false;
		}

		protected override void FullyPacked()
		{
			base.FullyPacked();
			FullyPackedBlocker.SetActive(value: true);
		}
	}
}
