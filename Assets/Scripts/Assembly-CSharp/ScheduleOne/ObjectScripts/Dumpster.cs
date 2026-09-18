using ScheduleOne.DevUtilities;
using ScheduleOne.EntityFramework;
using ScheduleOne.Equipping;
using ScheduleOne.Interaction;
using ScheduleOne.PlayerScripts;
using UnityEngine;

namespace ScheduleOne.ObjectScripts
{
	public class Dumpster : GridItem
	{
		public static float capacity = 100f;

		[Header("References")]
		[SerializeField]
		protected InteractableObject lid_IntObj;

		[SerializeField]
		protected InteractableObject inner_IntObj;

		[SerializeField]
		protected Transform lid;

		[SerializeField]
		protected Transform trash;

		public Transform standPoint;

		[Header("Settings")]
		[SerializeField]
		protected float trash_MinY;

		[SerializeField]
		protected float trash_MaxY;

		private float lid_CurrentAngle;

		private bool NetworkInitialize___EarlyScheduleOne_002EObjectScripts_002EDumpsterAssembly_002DCSharp_002Edll_Excuted;

		private bool NetworkInitialize__LateScheduleOne_002EObjectScripts_002EDumpsterAssembly_002DCSharp_002Edll_Excuted;

		public bool lidOpen { get; protected set; }

		public float currentTrashLevel { get; protected set; }

		public bool isFull => currentTrashLevel >= capacity;

		protected virtual void Update()
		{
			if (lidOpen)
			{
				lid_CurrentAngle = Mathf.Clamp(lid_CurrentAngle + Time.deltaTime * 90f * 3f, 0f, 90f);
			}
			else
			{
				lid_CurrentAngle = Mathf.Clamp(lid_CurrentAngle - Time.deltaTime * 90f * 3f, 0f, 90f);
			}
			lid.localRotation = Quaternion.Euler(0f, 0f, 0f - lid_CurrentAngle);
		}

		public virtual void Lid_Hovered()
		{
			if (lidOpen)
			{
				lid_IntObj.SetMessage("Close dumpster");
			}
			else
			{
				lid_IntObj.SetMessage("Open dumpster");
			}
		}

		public virtual void Lid_Interacted()
		{
			lidOpen = !lidOpen;
		}

		protected bool DoesPlayerHaveBinEquipped()
		{
			if (PlayerSingleton<PlayerInventory>.Instance.equippedSlot != null && PlayerSingleton<PlayerInventory>.Instance.equippedSlot.Equippable is Equippable_Bin)
			{
				return true;
			}
			return false;
		}

		public void ChangeTrashLevel(float change)
		{
			SetTrashLevel(currentTrashLevel + change);
		}

		public void SetTrashLevel(float trashLevel)
		{
			currentTrashLevel = Mathf.Clamp(trashLevel, 0f, capacity);
			UpdateTrashVisuals();
		}

		private void UpdateTrashVisuals()
		{
			trash.localPosition = new Vector3(trash.localPosition.x, trash_MinY + currentTrashLevel / capacity * (trash_MaxY - trash_MinY));
			trash.gameObject.SetActive(currentTrashLevel > 0f);
		}

		public override bool CanBeDestroyed(out string reason)
		{
			if (currentTrashLevel > 0f)
			{
				reason = "Dumpster is not empty";
				return false;
			}
			return base.CanBeDestroyed(out reason);
		}

		public override void NetworkInitialize___Early()
		{
			if (!NetworkInitialize___EarlyScheduleOne_002EObjectScripts_002EDumpsterAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize___EarlyScheduleOne_002EObjectScripts_002EDumpsterAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize___Early();
			}
		}

		public override void NetworkInitialize__Late()
		{
			if (!NetworkInitialize__LateScheduleOne_002EObjectScripts_002EDumpsterAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize__LateScheduleOne_002EObjectScripts_002EDumpsterAssembly_002DCSharp_002Edll_Excuted = true;
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
