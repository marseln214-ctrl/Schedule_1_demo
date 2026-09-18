using System;
using System.Collections.Generic;
using FishNet;
using ScheduleOne.Audio;
using ScheduleOne.Combat;
using ScheduleOne.DevUtilities;
using ScheduleOne.Dragging;
using ScheduleOne.Equipping;
using ScheduleOne.GameTime;
using ScheduleOne.Interaction;
using ScheduleOne.Persistence;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.Persistence.Loaders;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Property;
using UnityEngine;

namespace ScheduleOne.Trash
{
	[RequireComponent(typeof(Rigidbody))]
	[RequireComponent(typeof(Draggable))]
	[RequireComponent(typeof(PhysicsDamageable))]
	public class TrashItem : MonoBehaviour, IGUIDRegisterable, ISaveable
	{
		public const float POSITION_CHANGE_THRESHOLD = 1f;

		public const float LINEAR_DRAG = 0.1f;

		public const float ANGULAR_DRAG = 0.1f;

		public const float MIN_Y = -100f;

		public const int INTERACTION_PRIORITY = 5;

		public Rigidbody Rigidbody;

		public Draggable Draggable;

		[Header("Settings")]
		public string ID = "trashid";

		[Range(0f, 5f)]
		public int Size = 2;

		[Range(0f, 10f)]
		public int SellValue = 1;

		public bool CanGoInContainer = true;

		public Collider[] colliders;

		private Vector3 lastPosition = Vector3.zero;

		public Action<TrashItem> onDestroyed;

		private bool collidersEnabled = true;

		private float timeOnPhysicsEnabled;

		public Guid GUID { get; protected set; }

		public ScheduleOne.Property.Property CurrentProperty { get; protected set; }

		public string SaveFolderName => "Trash_" + GUID.ToString().Substring(0, 6);

		public string SaveFileName => "Trash_" + GUID.ToString().Substring(0, 6);

		public Loader Loader => null;

		public bool ShouldSaveUnderFolder => false;

		public List<string> LocalExtraFiles { get; set; } = new List<string>();


		public List<string> LocalExtraFolders { get; set; } = new List<string>();


		public bool HasChanged { get; set; }

		protected void Awake()
		{
			LayerUtility.SetLayerRecursively(base.gameObject, LayerMask.NameToLayer("Trash"));
			RecheckPosition();
			InvokeRepeating("RecheckPosition", UnityEngine.Random.Range(0f, 1f), 1f);
			SetPhysicsActive(active: false);
			Rigidbody.drag = 0.1f;
			Rigidbody.angularDrag = 0.1f;
			Rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
			Rigidbody.collisionDetectionMode = CollisionDetectionMode.Discrete;
			Rigidbody.sleepThreshold = 0.01f;
			Draggable.onDragStart.AddListener(delegate
			{
				SetContinuousCollisionDetection();
			});
			PhysicsDamageable physicsDamageable = GetComponent<PhysicsDamageable>();
			if (physicsDamageable == null)
			{
				physicsDamageable = base.gameObject.AddComponent<PhysicsDamageable>();
			}
			PhysicsDamageable physicsDamageable2 = physicsDamageable;
			physicsDamageable2.onImpacted = (Action<Impact>)Delegate.Combine(physicsDamageable2.onImpacted, (Action<Impact>)delegate(Impact impact)
			{
				if (impact.ImpactForce > 0f)
				{
					SetContinuousCollisionDetection();
				}
			});
		}

		protected void Start()
		{
			InitializeSaveable();
			TimeManager.onSleepEnd = (Action<int>)Delegate.Combine(TimeManager.onSleepEnd, new Action<int>(SleepEnd));
			TimeManager instance = NetworkSingleton<TimeManager>.Instance;
			instance.onMinutePass = (Action)Delegate.Combine(instance.onMinutePass, new Action(MinPass));
			Draggable.onHovered.AddListener(Hovered);
			Draggable.onInteracted.AddListener(Interacted);
		}

		public virtual void InitializeSaveable()
		{
			Singleton<SaveManager>.Instance.RegisterSaveable(this);
		}

		protected void OnValidate()
		{
			if (Rigidbody == null)
			{
				Rigidbody = GetComponent<Rigidbody>();
			}
			if (Draggable == null)
			{
				Draggable = GetComponent<Draggable>();
			}
			if (colliders == null || colliders.Length == 0)
			{
				colliders = GetComponentsInChildren<Collider>();
			}
			if (GetComponent<ImpactSoundEntity>() == null)
			{
				base.gameObject.AddComponent<ImpactSoundEntity>();
			}
		}

		protected void MinPass()
		{
			if (Time.time - timeOnPhysicsEnabled > 30f)
			{
				float num = Vector3.SqrMagnitude(PlayerSingleton<PlayerMovement>.Instance.transform.position - base.transform.position);
				SetCollidersEnabled(num < 900f);
			}
			if (base.transform.position.y < -100f && InstanceFinder.IsServer)
			{
				Console.LogWarning("Trash item fell below the world. Destroying.");
				DestroyTrash();
			}
		}

		protected void SleepEnd(int mins)
		{
		}

		protected void Hovered()
		{
			if (Equippable_TrashGrabber.IsEquipped && CanGoInContainer)
			{
				if (Equippable_TrashGrabber.Instance.GetCapacity() > 0)
				{
					Draggable.IntObj.SetMessage("Pick up");
					Draggable.IntObj.SetInteractableState(InteractableObject.EInteractableState.Default);
				}
				else
				{
					Draggable.IntObj.SetMessage("Bin is full");
					Draggable.IntObj.SetInteractableState(InteractableObject.EInteractableState.Invalid);
				}
			}
		}

		protected void Interacted()
		{
			if (Equippable_TrashGrabber.IsEquipped && CanGoInContainer && Equippable_TrashGrabber.Instance.GetCapacity() > 0)
			{
				Equippable_TrashGrabber.Instance.PickupTrash(this);
			}
		}

		public void SetGUID(Guid guid)
		{
			GUID = guid;
			GUIDManager.RegisterObject(this);
			string text = GUID.ToString();
			text = ((text[text.Length - 1] == '1') ? (text.Substring(0, text.Length - 1) + "2") : (text.Substring(0, text.Length - 1) + "1"));
			Draggable.SetGUID(new Guid(text));
		}

		public void SetVelocity(Vector3 velocity)
		{
			Rigidbody.velocity = velocity;
			HasChanged = true;
		}

		public void DestroyTrash()
		{
			NetworkSingleton<TrashManager>.Instance.DestroyTrash(this);
		}

		private void OnDestroy()
		{
			TimeManager.onSleepEnd = (Action<int>)Delegate.Remove(TimeManager.onSleepEnd, new Action<int>(SleepEnd));
			if (NetworkSingleton<TimeManager>.InstanceExists)
			{
				TimeManager instance = NetworkSingleton<TimeManager>.Instance;
				instance.onMinutePass = (Action)Delegate.Remove(instance.onMinutePass, new Action(MinPass));
			}
		}

		private void RecheckPosition()
		{
			if (Vector3.Distance(lastPosition, base.transform.position) > 1f)
			{
				lastPosition = base.transform.position;
				HasChanged = true;
				RecheckProperty();
			}
		}

		public virtual string GetSaveString()
		{
			return new TrashItemData(ID, GUID.ToString(), base.transform.position, base.transform.rotation).GetJson();
		}

		public virtual bool ShouldSave()
		{
			return true;
		}

		private void RecheckProperty()
		{
			if (CurrentProperty != null && CurrentProperty.DoBoundsContainPoint(base.transform.position))
			{
				return;
			}
			CurrentProperty = null;
			for (int i = 0; i < ScheduleOne.Property.Property.OwnedProperties.Count; i++)
			{
				if (!(Vector3.Distance(base.transform.position, ScheduleOne.Property.Property.OwnedProperties[i].BoundingBox.transform.position) > 25f) && ScheduleOne.Property.Property.OwnedProperties[i].DoBoundsContainPoint(base.transform.position))
				{
					CurrentProperty = ScheduleOne.Property.Property.OwnedProperties[i];
					break;
				}
			}
		}

		public void SetContinuousCollisionDetection()
		{
			Rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
			SetPhysicsActive(active: true);
			CancelInvoke("SetDiscreteCollisionDetection");
			Invoke("SetDiscreteCollisionDetection", 60f);
		}

		public void SetDiscreteCollisionDetection()
		{
			if (!(Rigidbody == null))
			{
				SetPhysicsActive(active: false);
				Rigidbody.collisionDetectionMode = CollisionDetectionMode.Discrete;
			}
		}

		public void SetPhysicsActive(bool active)
		{
			Rigidbody.isKinematic = !active;
			SetCollidersEnabled(active);
			if (active)
			{
				timeOnPhysicsEnabled = Time.time;
			}
		}

		public void SetCollidersEnabled(bool enabled)
		{
			if (collidersEnabled != enabled)
			{
				collidersEnabled = enabled;
				for (int i = 0; i < colliders.Length; i++)
				{
					colliders[i].enabled = true;
				}
				if (!collidersEnabled)
				{
					Rigidbody.isKinematic = true;
				}
			}
		}
	}
}
