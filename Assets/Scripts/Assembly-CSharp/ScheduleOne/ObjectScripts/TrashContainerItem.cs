using System;
using System.Collections.Generic;
using ScheduleOne.Audio;
using ScheduleOne.DevUtilities;
using ScheduleOne.EntityFramework;
using ScheduleOne.ItemFramework;
using ScheduleOne.Management;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.Trash;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace ScheduleOne.ObjectScripts
{
	[RequireComponent(typeof(TrashContainer))]
	public class TrashContainerItem : GridItem, ITransitEntity
	{
		public const float MAX_VERTICAL_OFFSET = 2f;

		public TrashContainer Container;

		public ParticleSystem Flies;

		public AudioSourceController TrashAddedSound;

		public DecalProjector PickupAreaProjector;

		public Transform[] accessPoints;

		[Header("Pickup settings")]
		public bool UsableByCleaners = true;

		public float PickupRadius = 5f;

		public List<TrashItem> TrashItemsInRadius = new List<TrashItem>();

		public List<TrashBag> TrashBagsInRadius = new List<TrashBag>();

		private bool NetworkInitialize___EarlyScheduleOne_002EObjectScripts_002ETrashContainerItemAssembly_002DCSharp_002Edll_Excuted;

		private bool NetworkInitialize__LateScheduleOne_002EObjectScripts_002ETrashContainerItemAssembly_002DCSharp_002Edll_Excuted;

		public List<ItemSlot> InputSlots { get; set; } = new List<ItemSlot>();


		public List<ItemSlot> OutputSlots { get; set; } = new List<ItemSlot>();


		public Vector3 LinkOriginPoint => base.transform.position;

		public Transform[] AccessPoints => accessPoints;

		public bool Selectable { get; }

		public bool IsAcceptingItems { get; set; }

		public override void Awake()
		{
			NetworkInitialize___Early();
			Awake_UserLogic_ScheduleOne_002EObjectScripts_002ETrashContainerItem_Assembly_002DCSharp_002Edll();
			NetworkInitialize__Late();
		}

		protected override void Start()
		{
			base.Start();
			Container.onTrashLevelChanged.AddListener(TrashLevelChanged);
			Container.onTrashAdded.AddListener(TrashAdded);
			if (!isGhost)
			{
				InvokeRepeating("CheckTrashItems", UnityEngine.Random.Range(0f, 1f), 1f);
			}
		}

		private void TrashLevelChanged()
		{
			base.HasChanged = true;
			if (Container.NormalizedTrashLevel > 0.75f)
			{
				if (!Flies.isPlaying)
				{
					Flies.Play();
				}
			}
			else if (Flies.isPlaying)
			{
				Flies.Stop();
			}
		}

		public override bool CanBeDestroyed(out string reason)
		{
			if (Container.TrashLevel > 0)
			{
				reason = "Contains trash";
				return false;
			}
			return base.CanBeDestroyed(out reason);
		}

		public override string GetSaveString()
		{
			return new TrashContainerData(base.GUID, base.ItemInstance, 0, base.OwnerGrid, OriginCoordinate, Rotation, Container.Content.GetData()).GetJson();
		}

		private void TrashAdded(string trashID)
		{
			if (!(TrashAddedSound == null))
			{
				float volumeMultiplier = Mathf.Clamp01((float)NetworkSingleton<TrashManager>.Instance.GetTrashPrefab(trashID).Size / 4f);
				TrashAddedSound.VolumeMultiplier = volumeMultiplier;
				TrashAddedSound.Play();
			}
		}

		public override void ShowOutline(Color color)
		{
			base.ShowOutline(color);
			PickupAreaProjector.enabled = true;
		}

		public override void HideOutline()
		{
			base.HideOutline();
			PickupAreaProjector.enabled = false;
		}

		private void CheckTrashItems()
		{
			for (int i = 0; i < TrashItemsInRadius.Count; i++)
			{
				if (!IsTrashValid(TrashItemsInRadius[i]))
				{
					RemoveTrashItemFromRadius(TrashItemsInRadius[i]);
					i--;
				}
			}
			Collider[] array = Physics.OverlapSphere(base.transform.position, PickupRadius, LayerMask.GetMask("Trash"), QueryTriggerInteraction.Ignore);
			for (int j = 0; j < array.Length; j++)
			{
				if (IsPointInRadius(array[j].transform.position))
				{
					TrashItem componentInParent = array[j].GetComponentInParent<TrashItem>();
					if (componentInParent != null && IsTrashValid(componentInParent))
					{
						AddTrashToRadius(componentInParent);
					}
				}
			}
		}

		private void AddTrashToRadius(TrashItem trashItem)
		{
			if (trashItem is TrashBag)
			{
				AddTrashBagToRadius(trashItem as TrashBag);
			}
			else if (!TrashItemsInRadius.Contains(trashItem))
			{
				TrashItemsInRadius.Add(trashItem);
				trashItem.onDestroyed = (Action<TrashItem>)Delegate.Combine(trashItem.onDestroyed, new Action<TrashItem>(RemoveTrashItemFromRadius));
			}
		}

		private void AddTrashBagToRadius(TrashBag trashBag)
		{
			if (!TrashBagsInRadius.Contains(trashBag))
			{
				TrashBagsInRadius.Add(trashBag);
				trashBag.onDestroyed = (Action<TrashItem>)Delegate.Combine(trashBag.onDestroyed, new Action<TrashItem>(RemoveTrashItemFromRadius));
			}
		}

		private void RemoveTrashItemFromRadius(TrashItem trashItem)
		{
			if (trashItem is TrashBag)
			{
				RemoveTrashBagFromRadius(trashItem as TrashBag);
			}
			else if (TrashItemsInRadius.Contains(trashItem))
			{
				TrashItemsInRadius.Remove(trashItem);
				trashItem.onDestroyed = (Action<TrashItem>)Delegate.Remove(trashItem.onDestroyed, new Action<TrashItem>(RemoveTrashItemFromRadius));
			}
		}

		private void RemoveTrashBagFromRadius(TrashBag trashBag)
		{
			if (TrashBagsInRadius.Contains(trashBag))
			{
				TrashBagsInRadius.Remove(trashBag);
				trashBag.onDestroyed = (Action<TrashItem>)Delegate.Remove(trashBag.onDestroyed, new Action<TrashItem>(RemoveTrashItemFromRadius));
			}
		}

		private bool IsTrashValid(TrashItem trashItem)
		{
			if (trashItem == null)
			{
				return false;
			}
			if (!IsPointInRadius(trashItem.transform.position))
			{
				return false;
			}
			if (trashItem.Draggable.IsBeingDragged)
			{
				return false;
			}
			return true;
		}

		public bool IsPointInRadius(Vector3 point)
		{
			float num = Vector3.Distance(point, base.transform.position);
			float num2 = Mathf.Abs(point.y - base.transform.position.y);
			if (num <= PickupRadius + 0.2f)
			{
				return num2 <= 2f;
			}
			return false;
		}

		public override void NetworkInitialize___Early()
		{
			if (!NetworkInitialize___EarlyScheduleOne_002EObjectScripts_002ETrashContainerItemAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize___EarlyScheduleOne_002EObjectScripts_002ETrashContainerItemAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize___Early();
			}
		}

		public override void NetworkInitialize__Late()
		{
			if (!NetworkInitialize__LateScheduleOne_002EObjectScripts_002ETrashContainerItemAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize__LateScheduleOne_002EObjectScripts_002ETrashContainerItemAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize__Late();
			}
		}

		public override void NetworkInitializeIfDisabled()
		{
			NetworkInitialize___Early();
			NetworkInitialize__Late();
		}

		protected virtual void Awake_UserLogic_ScheduleOne_002EObjectScripts_002ETrashContainerItem_Assembly_002DCSharp_002Edll()
		{
			base.Awake();
			PickupAreaProjector.size = new Vector3(PickupRadius * 2f, PickupRadius * 2f, 0.2f);
			PickupAreaProjector.enabled = false;
		}
	}
}
