using System.Collections.Generic;
using ScheduleOne.Building;
using ScheduleOne.EntityFramework;
using ScheduleOne.Tiles;
using UnityEngine;

namespace ScheduleOne.ObjectScripts
{
	public class FloorRack : GridItem, IProceduralTileContainer
	{
		[Header("References")]
		public Transform leg_BottomLeft;

		public Transform leg_BottomRight;

		public Transform leg_TopLeft;

		public Transform leg_TopRight;

		public CornerObstacle obs_BottomLeft;

		public CornerObstacle obs_BottomRight;

		public CornerObstacle obs_TopLeft;

		public CornerObstacle obs_TopRight;

		public List<ProceduralTile> procTiles;

		private bool NetworkInitialize___EarlyScheduleOne_002EObjectScripts_002EFloorRackAssembly_002DCSharp_002Edll_Excuted;

		private bool NetworkInitialize__LateScheduleOne_002EObjectScripts_002EFloorRackAssembly_002DCSharp_002Edll_Excuted;

		public List<ProceduralTile> ProceduralTiles => procTiles;

		public virtual void UpdateLegVisibility()
		{
			CockAndBalls(leg_BottomLeft.gameObject, obs_BottomLeft, -1, -1);
			CockAndBalls(leg_BottomRight.gameObject, obs_BottomRight, 1, -1);
			CockAndBalls(leg_TopLeft.gameObject, obs_TopLeft, -1, 1);
			CockAndBalls(leg_TopRight.gameObject, obs_TopRight, 1, 1);
		}

		protected void CockAndBalls(GameObject leg, CornerObstacle obs, int xOffset, int yOffset)
		{
			FloorRack floorRack = null;
			FloorRack floorRack2 = null;
			FloorRack floorRack3 = null;
			Coordinate coord = new Coordinate(CoordinatePairs[0].coord2.x + xOffset, CoordinatePairs[0].coord2.y + yOffset);
			if (base.OwnerGrid.GetTile(coord) != null && GetFloorRackFromOccupants(base.OwnerGrid.GetTile(coord).BuildableOccupants) != null)
			{
				floorRack = GetFloorRackFromOccupants(base.OwnerGrid.GetTile(coord).BuildableOccupants);
			}
			Coordinate coord2 = new Coordinate(CoordinatePairs[0].coord2.x + xOffset, CoordinatePairs[0].coord2.y);
			if (base.OwnerGrid.GetTile(coord2) != null && GetFloorRackFromOccupants(base.OwnerGrid.GetTile(coord2).BuildableOccupants) != null)
			{
				floorRack2 = GetFloorRackFromOccupants(base.OwnerGrid.GetTile(coord2).BuildableOccupants);
			}
			Coordinate coord3 = new Coordinate(CoordinatePairs[0].coord2.x, CoordinatePairs[0].coord2.y + yOffset);
			if (base.OwnerGrid.GetTile(coord3) != null && GetFloorRackFromOccupants(base.OwnerGrid.GetTile(coord3).BuildableOccupants) != null)
			{
				floorRack3 = GetFloorRackFromOccupants(base.OwnerGrid.GetTile(coord3).BuildableOccupants);
			}
			bool flag = true;
			if ((!(floorRack2 != null) || !(floorRack3 != null) || !(floorRack != null)) && floorRack == null && (!(floorRack2 != null) || !(floorRack3 == null)) && floorRack2 == null)
			{
				_ = floorRack3 != null;
			}
			leg.gameObject.SetActive(flag);
			obs.obstacleEnabled = flag;
		}

		private FloorRack GetFloorRackFromOccupants(List<GridItem> occs)
		{
			for (int i = 0; i < occs.Count; i++)
			{
				if (occs[i] is FloorRack)
				{
					return occs[i] as FloorRack;
				}
			}
			return null;
		}

		public List<FloorRack> GetSurroundingRacks()
		{
			List<FloorRack> list = new List<FloorRack>();
			for (int i = -1; i < 2; i++)
			{
				for (int j = -1; j < 2; j++)
				{
					if (i != 0 || j != 0)
					{
						Coordinate coord = new Coordinate(CoordinatePairs[0].coord2.x + i, CoordinatePairs[0].coord2.y + j);
						if (base.OwnerGrid.GetTile(coord) != null && GetFloorRackFromOccupants(base.OwnerGrid.GetTile(coord).BuildableOccupants) != null)
						{
							list.Add(GetFloorRackFromOccupants(base.OwnerGrid.GetTile(coord).BuildableOccupants));
						}
					}
				}
			}
			return list;
		}

		public override bool CanShareTileWith(List<GridItem> obstacles)
		{
			for (int i = 0; i < obstacles.Count; i++)
			{
				if (obstacles[i] is FloorRack)
				{
					return false;
				}
			}
			return true;
		}

		public override bool CanBeDestroyed(out string reason)
		{
			bool flag = false;
			foreach (ProceduralTile procTile in procTiles)
			{
				if (procTile.Occupants.Count > 0 || procTile.OccupantTiles.Count > 0)
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				reason = base.ItemInstance.Name + " is supporting another item";
				return false;
			}
			return base.CanBeDestroyed(out reason);
		}

		public override void DestroyItem(bool callOnServer = true)
		{
			for (int i = 0; i < CoordinatePairs.Count; i++)
			{
				base.OwnerGrid.GetTile(CoordinatePairs[i].coord2).RemoveOccupant(this, GetFootprintTile(CoordinatePairs[i].coord1));
			}
			base.DestroyItem(callOnServer);
		}

		public override void NetworkInitialize___Early()
		{
			if (!NetworkInitialize___EarlyScheduleOne_002EObjectScripts_002EFloorRackAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize___EarlyScheduleOne_002EObjectScripts_002EFloorRackAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize___Early();
			}
		}

		public override void NetworkInitialize__Late()
		{
			if (!NetworkInitialize__LateScheduleOne_002EObjectScripts_002EFloorRackAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize__LateScheduleOne_002EObjectScripts_002EFloorRackAssembly_002DCSharp_002Edll_Excuted = true;
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
