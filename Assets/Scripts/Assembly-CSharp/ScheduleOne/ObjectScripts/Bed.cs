using FishNet.Object;
using ScheduleOne.DevUtilities;
using ScheduleOne.Employees;
using ScheduleOne.GameTime;
using ScheduleOne.Interaction;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Tools;
using ScheduleOne.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.ObjectScripts
{
	public class Bed : NetworkBehaviour
	{
		public const int MIN_SLEEP_TIME = 1800;

		public const float SLEEP_TIME_SCALE = 1f;

		[Header("References")]
		[SerializeField]
		protected InteractableObject intObj;

		public GameObject Clipboard;

		public SpriteRenderer MugshotSprite;

		public TextMeshPro NameLabel;

		public MeshRenderer BlanketMesh;

		[Header("Materials")]
		public Material DefaultBlanket;

		public Material BotanistBlanket;

		public Material ChemistBlanket;

		public Material PackagerBlanket;

		public Material CleanerBlanket;

		public UnityEvent onAssignedEmployeeChanged;

		private bool NetworkInitialize___EarlyScheduleOne_002EObjectScripts_002EBedAssembly_002DCSharp_002Edll_Excuted;

		private bool NetworkInitialize__LateScheduleOne_002EObjectScripts_002EBedAssembly_002DCSharp_002Edll_Excuted;

		public Employee AssignedEmployee { get; protected set; }

		public virtual void Awake()
		{
			NetworkInitialize___Early();
			Awake_UserLogic_ScheduleOne_002EObjectScripts_002EBed_Assembly_002DCSharp_002Edll();
			NetworkInitialize__Late();
		}

		public void Hovered()
		{
			if (Singleton<ManagementClipboard>.Instance.IsEquipped)
			{
				intObj.SetInteractableState(InteractableObject.EInteractableState.Disabled);
			}
			else if (AssignedEmployee != null)
			{
				intObj.SetInteractableState(InteractableObject.EInteractableState.Invalid);
				intObj.SetMessage("Assigned to " + AssignedEmployee.fullName);
			}
			else if (CanSleep())
			{
				intObj.SetInteractableState(InteractableObject.EInteractableState.Default);
				intObj.SetMessage("Sleep");
			}
			else
			{
				intObj.SetInteractableState(InteractableObject.EInteractableState.Invalid);
				intObj.SetMessage("Can't sleep before " + ScheduleOne.GameTime.TimeManager.Get12HourTime(1800f));
			}
		}

		public void Interacted()
		{
			Player.Local.CurrentBed = base.NetworkObject;
			Singleton<SleepCanvas>.Instance.SetIsOpen(open: true);
		}

		private bool CanSleep()
		{
			return NetworkSingleton<ScheduleOne.GameTime.TimeManager>.Instance.IsCurrentTimeWithinRange(1800, 400);
		}

		public void SetAssignedEmployee(Employee employee)
		{
			AssignedEmployee = employee;
			if (AssignedEmployee != null)
			{
				MugshotSprite.sprite = AssignedEmployee.MugshotSprite;
				NameLabel.text = AssignedEmployee.FirstName + "\n" + AssignedEmployee.LastName;
				Clipboard.gameObject.SetActive(value: true);
			}
			else
			{
				Clipboard.gameObject.SetActive(value: false);
			}
			UpdateMaterial();
			if (onAssignedEmployeeChanged != null)
			{
				onAssignedEmployeeChanged.Invoke();
			}
		}

		private void UpdateMaterial()
		{
			if (BlanketMesh == null)
			{
				return;
			}
			Material material = DefaultBlanket;
			if (AssignedEmployee != null)
			{
				switch (AssignedEmployee.EmployeeType)
				{
				case EEmployeeType.Botanist:
					material = BotanistBlanket;
					break;
				case EEmployeeType.Chemist:
					material = ChemistBlanket;
					break;
				case EEmployeeType.Packager:
					material = PackagerBlanket;
					break;
				case EEmployeeType.Cleaner:
					material = CleanerBlanket;
					break;
				}
			}
			BlanketMesh.material = material;
		}

		public virtual void NetworkInitialize___Early()
		{
			if (!NetworkInitialize___EarlyScheduleOne_002EObjectScripts_002EBedAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize___EarlyScheduleOne_002EObjectScripts_002EBedAssembly_002DCSharp_002Edll_Excuted = true;
			}
		}

		public virtual void NetworkInitialize__Late()
		{
			if (!NetworkInitialize__LateScheduleOne_002EObjectScripts_002EBedAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize__LateScheduleOne_002EObjectScripts_002EBedAssembly_002DCSharp_002Edll_Excuted = true;
			}
		}

		public override void NetworkInitializeIfDisabled()
		{
			NetworkInitialize___Early();
			NetworkInitialize__Late();
		}

		private void Awake_UserLogic_ScheduleOne_002EObjectScripts_002EBed_Assembly_002DCSharp_002Edll()
		{
			if (Clipboard != null)
			{
				Clipboard.gameObject.SetActive(value: false);
			}
			UpdateMaterial();
		}
	}
}
