using System.Collections.Generic;
using ScheduleOne.ConstructableScripts;
using ScheduleOne.DevUtilities;
using ScheduleOne.ObjectScripts;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Property.Utilities.Power;
using ScheduleOne.Tiles;
using ScheduleOne.UI;
using ScheduleOne.UI.Construction;
using UnityEngine;

namespace ScheduleOne.Construction.ConstructionMethods
{
	public class ConstructUpdate_PowerTower : ConstructUpdate_OutdoorGrid
	{
		[Header("Materials")]
		public Material specialMat;

		public Material powerLine_GhostMat;

		[Header("References")]
		[SerializeField]
		protected GameObject cosmeticPowerNode;

		public float LengthFactor = 1.002f;

		protected Transform tempPowerLineContainer;

		protected List<Transform> tempSegments = new List<Transform>();

		private PowerNode hoveredPowerNode;

		protected PowerNode startNode;

		protected float powerLineInitialDistance;

		protected override void Start()
		{
			base.Start();
			tempPowerLineContainer = new GameObject("TempPowerLine").transform;
			tempPowerLineContainer.SetParent(base.transform);
			for (int i = 0; i < PowerLine.powerLine_MaxSegments; i++)
			{
				Transform transform = Object.Instantiate(Singleton<PowerManager>.Instance.powerLineSegmentPrefab, tempPowerLineContainer).transform;
				transform.Find("Model").GetComponent<MeshRenderer>().material = powerLine_GhostMat;
				transform.gameObject.SetActive(value: false);
				tempSegments.Add(transform);
			}
			GameInput.RegisterExitListener(Exit, 5);
		}

		public override void ConstructionStop()
		{
			GameInput.DeregisterExitListener(Exit);
			base.ConstructionStop();
		}

		protected override void Update()
		{
			base.Update();
			hoveredPowerNode = GetHoveredPowerNode();
			GhostModel.gameObject.SetActive(value: true);
			cosmeticPowerNode.SetActive(value: false);
			if (base.isMoving)
			{
				return;
			}
			if (startNode == null)
			{
				if (hoveredPowerNode != null)
				{
					cosmeticPowerNode.transform.position = hoveredPowerNode.transform.position;
					cosmeticPowerNode.transform.rotation = hoveredPowerNode.transform.rotation;
					GhostModel.gameObject.SetActive(value: false);
					cosmeticPowerNode.SetActive(value: true);
					if (GameInput.GetButtonDown(GameInput.ButtonCode.PrimaryClick))
					{
						startNode = hoveredPowerNode;
						powerLineInitialDistance = Vector3.Distance(PlayerSingleton<PlayerCamera>.Instance.transform.position, startNode.pConnectionPoint.transform.position);
					}
				}
			}
			else if (hoveredPowerNode != null && hoveredPowerNode != startNode && PowerLine.CanNodesBeConnected(hoveredPowerNode, startNode))
			{
				cosmeticPowerNode.transform.position = hoveredPowerNode.transform.position;
				cosmeticPowerNode.transform.rotation = hoveredPowerNode.transform.rotation;
				GhostModel.gameObject.SetActive(value: false);
				cosmeticPowerNode.SetActive(value: true);
			}
		}

		protected override void LateUpdate()
		{
			base.LateUpdate();
			if (!(startNode != null) || base.isMoving)
			{
				return;
			}
			Vector3 position = startNode.pConnectionPoint.transform.position;
			Vector3 vector = PlayerSingleton<PlayerCamera>.Instance.Camera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, powerLineInitialDistance));
			if (PlayerSingleton<PlayerCamera>.Instance.MouseRaycast(100f, out var hit, detectionMask))
			{
				vector = hit.point;
			}
			if (validPosition)
			{
				vector = ConstructableClass.PowerNode.pConnectionPoint.position;
				if (Vector3.Distance(startNode.pConnectionPoint.position, vector) > PowerLine.maxLineLength)
				{
					for (int i = 0; i < tempSegments.Count; i++)
					{
						tempSegments[i].gameObject.SetActive(value: false);
					}
					return;
				}
			}
			else
			{
				GhostModel.gameObject.SetActive(value: false);
			}
			if (hoveredPowerNode != null && PowerLine.CanNodesBeConnected(startNode, hoveredPowerNode))
			{
				vector = hoveredPowerNode.pConnectionPoint.position;
			}
			if (position == vector)
			{
				for (int j = 0; j < tempSegments.Count; j++)
				{
					tempSegments[j].gameObject.SetActive(value: false);
				}
				return;
			}
			PowerNode powerNode = GetHoveredPowerNode();
			int segmentCount = PowerLine.GetSegmentCount(position, vector);
			List<Transform> list = new List<Transform>();
			for (int k = 0; k < tempSegments.Count; k++)
			{
				if (k < segmentCount)
				{
					tempSegments[k].gameObject.SetActive(value: true);
					list.Add(tempSegments[k]);
				}
				else
				{
					tempSegments[k].gameObject.SetActive(value: false);
				}
			}
			PowerLine.DrawPowerLine(position, vector, list, LengthFactor);
			if (GameInput.GetButtonDown(GameInput.ButtonCode.PrimaryClick) && powerNode != null && PowerLine.CanNodesBeConnected(startNode, powerNode))
			{
				CompletePowerLine(powerNode);
			}
		}

		public void Exit(ExitAction exit)
		{
			if (!exit.used && startNode != null)
			{
				exit.used = true;
				StopCreatingPowerLine();
			}
		}

		private PowerTower GetHoveredPowerTower()
		{
			if (PlayerSingleton<PlayerCamera>.Instance.MouseRaycast(100f, out var hit, detectionMask))
			{
				if (hit.collider.GetComponentInParent<PowerTower>() != null)
				{
					return hit.collider.GetComponentInParent<PowerTower>();
				}
				if (hit.collider.GetComponentInChildren<Tile>() != null)
				{
					Tile componentInChildren = hit.collider.GetComponentInChildren<Tile>();
					if (componentInChildren.ConstructableOccupants.Count > 0 && componentInChildren.ConstructableOccupants[0] is PowerTower)
					{
						return componentInChildren.ConstructableOccupants[0] as PowerTower;
					}
				}
			}
			return null;
		}

		protected PowerNode GetHoveredPowerNode()
		{
			if (PlayerSingleton<PlayerCamera>.Instance.MouseRaycast(200f, out var hit, 1 << LayerMask.NameToLayer("Default")) && (bool)hit.collider.GetComponentInParent<PowerNodeTag>())
			{
				return hit.collider.GetComponentInParent<PowerNodeTag>().powerNode;
			}
			return null;
		}

		protected override Constructable_GridBased PlaceNewConstructable()
		{
			Constructable_GridBased constructable_GridBased = base.PlaceNewConstructable();
			if (startNode != null && Vector3.Distance(startNode.pConnectionPoint.position, constructable_GridBased.PowerNode.pConnectionPoint.position) <= PowerLine.maxLineLength)
			{
				PowerLine c = Singleton<PowerManager>.Instance.CreatePowerLine(startNode, constructable_GridBased.PowerNode, Singleton<ConstructionManager>.Instance.currentProperty);
				if (Singleton<ConstructionManager>.Instance.onNewConstructableBuilt != null)
				{
					Singleton<ConstructionManager>.Instance.onNewConstructableBuilt(c);
				}
				StopCreatingPowerLine();
				startNode = constructable_GridBased.PowerNode;
			}
			return constructable_GridBased;
		}

		private void CompletePowerLine(PowerNode target)
		{
			PowerLine c = Singleton<PowerManager>.Instance.CreatePowerLine(startNode, target, Singleton<ConstructionManager>.Instance.currentProperty);
			if (Singleton<ConstructionManager>.Instance.onNewConstructableBuilt != null)
			{
				Singleton<ConstructionManager>.Instance.onNewConstructableBuilt(c);
			}
			StopCreatingPowerLine();
			if (Input.GetKey(KeyCode.LeftShift))
			{
				startNode = target;
				return;
			}
			startNode = null;
			Singleton<ConstructionMenu>.Instance.ClearSelectedListing();
		}

		private void StopCreatingPowerLine()
		{
			Singleton<HUD>.Instance.HideTopScreenText();
			startNode = null;
			for (int i = 0; i < tempSegments.Count; i++)
			{
				tempSegments[i].gameObject.SetActive(value: false);
			}
		}
	}
}
