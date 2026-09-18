using System.Collections.Generic;
using ScheduleOne.DevUtilities;
using ScheduleOne.Money;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Property.Utilities.Power;
using ScheduleOne.UI;
using ScheduleOne.UI.Construction;
using UnityEngine;

namespace ScheduleOne.Construction.ConstructionMethods
{
	public class ConstructUpdate_PowerLine : ConstructUpdate_Base
	{
		[Header("Settings")]
		[SerializeField]
		protected Material ghostPowerLine_Material;

		[Header("References")]
		[SerializeField]
		protected GameObject cosmeticPowerNode;

		protected Transform tempPowerLineContainer;

		protected PowerNode hoveredPowerNode;

		protected List<Transform> tempSegments = new List<Transform>();

		protected PowerNode startNode;

		protected float powerLineInitialDistance;

		protected virtual void Start()
		{
			tempPowerLineContainer = new GameObject("TempPowerLine").transform;
			tempPowerLineContainer.SetParent(base.transform);
			for (int i = 0; i < PowerLine.powerLine_MaxSegments; i++)
			{
				Transform transform = Object.Instantiate(Singleton<PowerManager>.Instance.powerLineSegmentPrefab, tempPowerLineContainer).transform;
				transform.Find("Model").GetComponent<MeshRenderer>().material = ghostPowerLine_Material;
				transform.gameObject.SetActive(value: false);
				tempSegments.Add(transform);
			}
			GameInput.RegisterExitListener(Exit, 5);
		}

		public override void ConstructionStop()
		{
			GameInput.DeregisterExitListener(Exit);
			Singleton<HUD>.Instance.HideTopScreenText();
			base.ConstructionStop();
		}

		public void Exit(ExitAction exit)
		{
			if (!exit.used && startNode != null)
			{
				exit.used = true;
				StopCreatingPowerLine();
			}
		}

		protected override void Update()
		{
			base.Update();
			cosmeticPowerNode.SetActive(value: false);
			hoveredPowerNode = GetHoveredPowerNode();
			if (startNode == null)
			{
				Singleton<HUD>.Instance.ShowTopScreenText("Choose start point");
				if (hoveredPowerNode != null)
				{
					cosmeticPowerNode.transform.position = hoveredPowerNode.transform.position;
					cosmeticPowerNode.transform.rotation = hoveredPowerNode.transform.rotation;
					cosmeticPowerNode.gameObject.SetActive(value: true);
					if (GameInput.GetButtonDown(GameInput.ButtonCode.PrimaryClick))
					{
						startNode = hoveredPowerNode;
						powerLineInitialDistance = Vector3.Distance(PlayerSingleton<PlayerCamera>.Instance.transform.position, startNode.pConnectionPoint.transform.position);
					}
				}
			}
			else
			{
				Singleton<HUD>.Instance.ShowTopScreenText("Choose end point");
				if (hoveredPowerNode != null && PowerLine.CanNodesBeConnected(startNode, hoveredPowerNode))
				{
					cosmeticPowerNode.transform.position = hoveredPowerNode.transform.position;
					cosmeticPowerNode.transform.rotation = hoveredPowerNode.transform.rotation;
					cosmeticPowerNode.gameObject.SetActive(value: true);
				}
			}
		}

		protected override void LateUpdate()
		{
			base.LateUpdate();
			if (!(startNode != null))
			{
				return;
			}
			Vector3 position = startNode.pConnectionPoint.transform.position;
			Vector3 vector = PlayerSingleton<PlayerCamera>.Instance.Camera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, powerLineInitialDistance));
			if (PlayerSingleton<PlayerCamera>.Instance.MouseRaycast(powerLineInitialDistance, out var hit, 1 << LayerMask.NameToLayer("Default")))
			{
				vector = hit.point;
			}
			Vector3 vector2 = vector - position;
			vector2 = Vector3.ClampMagnitude(vector2, PowerLine.maxLineLength);
			vector = position + vector2;
			PowerNode powerNode = GetHoveredPowerNode();
			if (powerNode != null && PowerLine.CanNodesBeConnected(startNode, powerNode))
			{
				vector = GetHoveredPowerNode().pConnectionPoint.transform.position;
			}
			int segmentCount = PowerLine.GetSegmentCount(position, vector);
			List<Transform> list = new List<Transform>();
			for (int i = 0; i < tempSegments.Count; i++)
			{
				if (i < segmentCount)
				{
					tempSegments[i].gameObject.SetActive(value: true);
					list.Add(tempSegments[i]);
				}
				else
				{
					tempSegments[i].gameObject.SetActive(value: false);
				}
			}
			PowerLine.DrawPowerLine(position, vector, list, 1.002f);
			if (GameInput.GetButtonDown(GameInput.ButtonCode.PrimaryClick) && powerNode != null && PowerLine.CanNodesBeConnected(startNode, powerNode) && NetworkSingleton<MoneyManager>.Instance.SyncAccessor_onlineBalance >= Singleton<ConstructionMenu>.Instance.GetListingPrice("Utilities/PowerLine/PowerLine"))
			{
				CompletePowerLine(powerNode);
			}
		}

		protected PowerNode GetHoveredPowerNode()
		{
			if (PlayerSingleton<PlayerCamera>.Instance.MouseRaycast(200f, out var hit, 1 << LayerMask.NameToLayer("Default")) && (bool)hit.collider.GetComponentInParent<PowerNodeTag>())
			{
				return hit.collider.GetComponentInParent<PowerNodeTag>().powerNode;
			}
			return null;
		}

		private void CompletePowerLine(PowerNode target)
		{
			NetworkSingleton<MoneyManager>.Instance.CreateOnlineTransaction("Power Line", 0f - Singleton<ConstructionMenu>.Instance.GetListingPrice("Utilities/PowerLine/PowerLine"), 1f, string.Empty);
			PowerLine c = Singleton<PowerManager>.Instance.CreatePowerLine(startNode, target, Singleton<ConstructionManager>.Instance.currentProperty);
			if (Singleton<ConstructionManager>.Instance.onNewConstructableBuilt != null)
			{
				Singleton<ConstructionManager>.Instance.onNewConstructableBuilt(c);
			}
			StopCreatingPowerLine();
			if (Input.GetKey(KeyCode.LeftShift))
			{
				startNode = target;
				if (startNode != null)
				{
					powerLineInitialDistance = Vector3.Distance(PlayerSingleton<PlayerCamera>.Instance.transform.position, target.pConnectionPoint.transform.position);
				}
			}
		}

		private void StopCreatingPowerLine()
		{
			startNode = null;
			for (int i = 0; i < tempSegments.Count; i++)
			{
				tempSegments[i].gameObject.SetActive(value: false);
			}
		}
	}
}
