using ScheduleOne.DevUtilities;
using ScheduleOne.ItemFramework;
using ScheduleOne.ObjectScripts;
using ScheduleOne.PlayerTasks;
using ScheduleOne.Product;
using ScheduleOne.Tools;
using UnityEngine;

namespace ScheduleOne.Packaging
{
	public class FunctionalProduct : Draggable
	{
		public bool ClampZ = true;

		[Header("References")]
		public Transform AlignmentPoint;

		public FilledPackagingVisuals Visuals;

		private Vector3 startLocalPos;

		private float lowestMaxZ = 500f;

		public SmoothedVelocityCalculator VelocityCalculator { get; private set; }

		public virtual void Initialize(PackagingStation station, ItemInstance item, Transform alignment, bool align = true)
		{
			if (align)
			{
				AlignTo(alignment);
			}
			startLocalPos = base.transform.localPosition;
			LayerUtility.SetLayerRecursively(base.gameObject, LayerMask.NameToLayer("Task"));
			InitializeVisuals(item);
			base.Rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
			if (VelocityCalculator == null)
			{
				VelocityCalculator = base.gameObject.AddComponent<SmoothedVelocityCalculator>();
				VelocityCalculator.MaxReasonableVelocity = 2f;
			}
		}

		public virtual void Initialize(ItemInstance item)
		{
			startLocalPos = base.transform.localPosition;
			LayerUtility.SetLayerRecursively(base.gameObject, LayerMask.NameToLayer("Task"));
			InitializeVisuals(item);
			base.Rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
			if (VelocityCalculator == null)
			{
				VelocityCalculator = base.gameObject.AddComponent<SmoothedVelocityCalculator>();
				VelocityCalculator.MaxReasonableVelocity = 2f;
			}
		}

		public virtual void InitializeVisuals(ItemInstance item)
		{
			if (!(item is ProductItemInstance productItemInstance))
			{
				Console.LogError("Item instance is not a product instance!");
			}
			else
			{
				productItemInstance.SetupPackagingVisuals(Visuals);
			}
		}

		public void AlignTo(Transform alignment)
		{
			base.transform.rotation = alignment.rotation * (Quaternion.Inverse(AlignmentPoint.rotation) * base.transform.rotation);
			base.transform.position = alignment.position + (base.transform.position - AlignmentPoint.position);
		}

		protected override void FixedUpdate()
		{
			base.FixedUpdate();
		}

		protected override void LateUpdate()
		{
			base.LateUpdate();
			if (ClampZ)
			{
				Clamp();
			}
		}

		private void Clamp()
		{
			float num = Mathf.Clamp(Mathf.Abs(base.transform.localPosition.x / startLocalPos.x), 0f, 1f);
			float num2 = (lowestMaxZ = Mathf.Min(Mathf.Abs(startLocalPos.z) * num, lowestMaxZ));
			Vector3 position = base.transform.parent.InverseTransformPoint(base.originalHitPoint);
			position.z = Mathf.Clamp(position.z, 0f - num2, num2);
			Vector3 vector = base.transform.parent.TransformPoint(position);
			SetOriginalHitPoint(vector);
		}
	}
}
