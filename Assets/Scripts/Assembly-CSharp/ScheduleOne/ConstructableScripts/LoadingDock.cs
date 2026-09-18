using ScheduleOne.DevUtilities;
using ScheduleOne.Vehicles;
using UnityEngine;

namespace ScheduleOne.ConstructableScripts
{
	public class LoadingDock : Constructable_GridBased
	{
		[Header("References")]
		[SerializeField]
		protected VehicleDetector vehicleDetector;

		[SerializeField]
		protected MeshRenderer[] redLightMeshes;

		[SerializeField]
		protected MeshRenderer[] greenLightMeshes;

		[SerializeField]
		protected Transform[] sideWalls;

		[SerializeField]
		protected Animation gateAnim;

		[SerializeField]
		protected Collider reservationBlocker;

		public Transform vehiclePosition;

		[Header("Materials")]
		[SerializeField]
		protected Material redLightMat_On;

		[SerializeField]
		protected Material redLightMat_Off;

		[SerializeField]
		protected Material greenLightMat_On;

		[SerializeField]
		protected Material greenLightMat_Off;

		private bool wallsOpen;

		private LandVehicle currentOccupant;

		private bool NetworkInitialize___EarlyScheduleOne_002EConstructableScripts_002ELoadingDockAssembly_002DCSharp_002Edll_Excuted;

		private bool NetworkInitialize__LateScheduleOne_002EConstructableScripts_002ELoadingDockAssembly_002DCSharp_002Edll_Excuted;

		public bool isOccupied => vehicleDetector.vehicles.Count > 0;

		public LandVehicle reservant { get; protected set; }

		private void Start()
		{
			reservationBlocker.gameObject.SetActive(value: false);
		}

		protected virtual void Update()
		{
			if (vehicleDetector.vehicles.Count > 0 && !vehicleDetector.closestVehicle.isOccupied)
			{
				wallsOpen = true;
			}
			else
			{
				wallsOpen = false;
			}
			_ = isOccupied;
			if (vehicleDetector.closestVehicle != null)
			{
				if (currentOccupant != vehicleDetector.closestVehicle && currentOccupant != null)
				{
					currentOccupant = vehicleDetector.closestVehicle;
				}
			}
			else if (currentOccupant != null)
			{
				currentOccupant = null;
			}
		}

		protected virtual void LateUpdate()
		{
			if (isOccupied)
			{
				MeshRenderer[] array = redLightMeshes;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].material = redLightMat_On;
				}
				array = greenLightMeshes;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].material = greenLightMat_Off;
				}
			}
			else
			{
				MeshRenderer[] array = redLightMeshes;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].material = redLightMat_Off;
				}
				array = greenLightMeshes;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].material = greenLightMat_On;
				}
			}
			float max = 0.387487f;
			float min = -0.35f;
			if (wallsOpen)
			{
				Transform[] array2 = sideWalls;
				foreach (Transform transform in array2)
				{
					transform.transform.localPosition = new Vector3(transform.transform.localPosition.x, Mathf.Clamp(transform.transform.localPosition.y - Time.deltaTime, min, max), transform.transform.localPosition.z);
				}
			}
			else
			{
				Transform[] array2 = sideWalls;
				foreach (Transform transform2 in array2)
				{
					transform2.transform.localPosition = new Vector3(transform2.transform.localPosition.x, Mathf.Clamp(transform2.transform.localPosition.y + Time.deltaTime, min, max), transform2.transform.localPosition.z);
				}
			}
		}

		public override bool CanBeDestroyed(out string reason)
		{
			if (reservant != null)
			{
				reason = "Reserved for dealer";
				return false;
			}
			return base.CanBeDestroyed(out reason);
		}

		public override void DestroyConstructable(bool callOnServer = true)
		{
			if (isOccupied && vehicleDetector.closestVehicle != null)
			{
				vehicleDetector.closestVehicle.Rb.isKinematic = false;
			}
			base.DestroyConstructable(callOnServer);
		}

		public void SetReservant(LandVehicle _res)
		{
			if (reservant != null)
			{
				Collider[] componentsInChildren = reservant.GetComponentsInChildren<Collider>();
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					Physics.IgnoreCollision(componentsInChildren[i], reservationBlocker, ignore: false);
				}
			}
			reservant = _res;
			if (reservant != null)
			{
				gateAnim.Play("LoadingDock_Gate_Close");
			}
			else
			{
				gateAnim.Play("LoadingDock_Gate_Open");
			}
			if (reservant != null)
			{
				Collider[] componentsInChildren2 = reservant.GetComponentsInChildren<Collider>();
				for (int j = 0; j < componentsInChildren2.Length; j++)
				{
					Physics.IgnoreCollision(componentsInChildren2[j], reservationBlocker, ignore: true);
				}
			}
			reservationBlocker.gameObject.SetActive(reservant != null);
		}

		public override void NetworkInitialize___Early()
		{
			if (!NetworkInitialize___EarlyScheduleOne_002EConstructableScripts_002ELoadingDockAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize___EarlyScheduleOne_002EConstructableScripts_002ELoadingDockAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize___Early();
			}
		}

		public override void NetworkInitialize__Late()
		{
			if (!NetworkInitialize__LateScheduleOne_002EConstructableScripts_002ELoadingDockAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize__LateScheduleOne_002EConstructableScripts_002ELoadingDockAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize__Late();
			}
		}

		public override void NetworkInitializeIfDisabled()
		{
			NetworkInitialize___Early();
			NetworkInitialize__Late();
		}

		public override void Awake()
		{
			NetworkInitialize___Early();
			base.Awake();
			NetworkInitialize__Late();
		}
	}
}
