using System;
using System.Collections.Generic;
using System.Linq;
using ScheduleOne.Audio;
using ScheduleOne.ObjectScripts;
using ScheduleOne.PlayerScripts;
using ScheduleOne.PlayerTasks;
using ScheduleOne.Product.Packaging;
using ScheduleOne.Tools;
using UnityEngine;

namespace ScheduleOne.Packaging
{
	public class FunctionalPackaging : Draggable
	{
		[Header("Settings")]
		public string SealInstruction = "Seal packaging";

		public bool AutoEnableSealing = true;

		public float ProductContactTime = 0.1f;

		public float ProductContactMaxVelocity = 0.3f;

		[Header("References")]
		public PackagingDefinition Definition;

		public Transform AlignmentPoint;

		public Transform[] ProductAlignmentPoints;

		public AudioSourceController SealSound;

		protected List<FunctionalProduct> PackedProducts = new List<FunctionalProduct>();

		public Action onFullyPacked;

		public Action onSealed;

		public Action onReachOutput;

		private PackagingStation station;

		private Dictionary<FunctionalProduct, float> productContactTime = new Dictionary<FunctionalProduct, float>();

		private SmoothedVelocityCalculator VelocityCalculator;

		public bool IsSealed { get; protected set; }

		public bool IsFull { get; protected set; }

		public bool ReachedOutput { get; protected set; }

		public virtual void Initialize(PackagingStation _station, Transform alignment, bool align = true)
		{
			station = _station;
			if (align)
			{
				AlignTo(alignment);
			}
			ClickableEnabled = false;
			base.Rb.isKinematic = true;
			if (VelocityCalculator == null)
			{
				VelocityCalculator = base.gameObject.AddComponent<SmoothedVelocityCalculator>();
				VelocityCalculator.MaxReasonableVelocity = 2f;
			}
		}

		public void AlignTo(Transform alignment)
		{
			base.transform.rotation = alignment.rotation * (Quaternion.Inverse(AlignmentPoint.rotation) * base.transform.rotation);
			Vector3 vector = base.transform.position - AlignmentPoint.position;
			base.transform.position = alignment.position + vector;
			base.Rb.position = base.transform.position;
			base.Rb.rotation = base.transform.rotation;
		}

		public virtual void Destroy()
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}

		protected override void FixedUpdate()
		{
			base.FixedUpdate();
			if (IsFull)
			{
				return;
			}
			foreach (FunctionalProduct item in productContactTime.Keys.ToList())
			{
				if (!(item.Rb == null) && productContactTime[item] > ProductContactTime && !PackedProducts.Contains(item) && !item.IsHeld)
				{
					PackProduct(item);
				}
			}
		}

		protected virtual void PackProduct(FunctionalProduct product)
		{
			product.ClickableEnabled = false;
			product.ClampZ = false;
			UnityEngine.Object.Destroy(product.Rb);
			product.transform.SetParent(base.transform);
			if (ProductAlignmentPoints.Length > PackedProducts.Count)
			{
				product.transform.position = ProductAlignmentPoints[PackedProducts.Count].position;
				product.transform.rotation = ProductAlignmentPoints[PackedProducts.Count].rotation;
			}
			PackedProducts.Add(product);
			if (PackedProducts.Count >= Definition.Quantity && !IsFull)
			{
				FullyPacked();
			}
		}

		protected virtual void FullyPacked()
		{
			IsFull = true;
			if (onFullyPacked != null)
			{
				onFullyPacked();
			}
			foreach (FunctionalProduct packedProduct in PackedProducts)
			{
				UnityEngine.Object.Destroy(packedProduct.Rb);
			}
			if (AutoEnableSealing)
			{
				EnableSealing();
			}
		}

		protected virtual void OnTriggerStay(Collider other)
		{
			if (station == null)
			{
				return;
			}
			FunctionalProduct componentInParent = other.GetComponentInParent<FunctionalProduct>();
			if (componentInParent != null && componentInParent.IsHeld)
			{
				return;
			}
			if (componentInParent != null)
			{
				if (!productContactTime.ContainsKey(componentInParent))
				{
					productContactTime.Add(componentInParent, 0f);
				}
				Vector3 velocity = componentInParent.VelocityCalculator.Velocity;
				Vector3 velocity2 = VelocityCalculator.Velocity;
				Vector3 vector = velocity - velocity2;
				Debug.DrawRay(componentInParent.transform.position, velocity, Color.red);
				Debug.DrawRay(base.transform.position, velocity2, Color.blue);
				if (vector.magnitude < ProductContactMaxVelocity)
				{
					productContactTime[componentInParent] += Time.fixedDeltaTime;
				}
			}
			if (other.gameObject.name == station.OutputCollider.name && !ReachedOutput && IsSealed && !base.IsHeld)
			{
				ReachedOutput = true;
				if (onReachOutput != null)
				{
					onReachOutput();
				}
			}
		}

		protected virtual void EnableSealing()
		{
			ClickableEnabled = true;
		}

		public virtual void Seal()
		{
			IsSealed = true;
			foreach (FunctionalProduct packedProduct in PackedProducts)
			{
				Collider[] componentsInChildren = packedProduct.GetComponentsInChildren<Collider>();
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					componentsInChildren[i].enabled = false;
				}
			}
			if (SealSound != null)
			{
				SealSound.Play();
			}
			HoveredCursor = CursorManager.ECursorType.OpenHand;
			ClickableEnabled = true;
			base.Rb.isKinematic = false;
			if (onSealed != null)
			{
				onSealed();
			}
		}
	}
}
