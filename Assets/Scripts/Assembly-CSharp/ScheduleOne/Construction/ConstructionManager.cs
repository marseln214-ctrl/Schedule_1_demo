using System;
using FishNet.Object;
using ScheduleOne.ConstructableScripts;
using ScheduleOne.Construction.ConstructionMethods;
using ScheduleOne.DevUtilities;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Property;
using ScheduleOne.Tiles;
using ScheduleOne.UI.Construction;
using UnityEngine;

namespace ScheduleOne.Construction
{
	public class ConstructionManager : Singleton<ConstructionManager>
	{
		public class WorldIntersection
		{
			public FootprintTile footprint;

			public Tile tile;
		}

		public delegate void ConstructableNotification(Constructable c);

		public NetworkObject networkObject;

		public Action onConstructionModeEnabled;

		public Action onConstructionModeDisabled;

		public GameObject constructHandler;

		public ConstructableNotification onNewConstructableBuilt;

		public ConstructableNotification onConstructableMoved;

		public ScheduleOne.Property.Property currentProperty;

		public bool constructionModeEnabled { get; protected set; }

		public bool isDeployingConstructable { get; protected set; }

		public bool isMovingConstructable { get; protected set; }

		protected override void Start()
		{
			base.Start();
			GameInput.RegisterExitListener(Exit);
		}

		public void EnterConstructionMode(ScheduleOne.Property.Property prop)
		{
			currentProperty = prop;
			constructionModeEnabled = true;
			prop.SetBoundsVisible(vis: true);
			PlayerSingleton<PlayerMovement>.Instance.canMove = false;
			PlayerSingleton<PlayerInventory>.Instance.SetInventoryEnabled(enabled: false);
			PlayerSingleton<PlayerCamera>.Instance.FreeMouse();
			if (onConstructionModeEnabled != null)
			{
				onConstructionModeEnabled();
			}
		}

		public void ExitConstructionMode()
		{
			currentProperty.SetBoundsVisible(vis: false);
			constructionModeEnabled = false;
			PlayerSingleton<PlayerMovement>.Instance.canMove = true;
			PlayerSingleton<PlayerInventory>.Instance.SetInventoryEnabled(enabled: true);
			Singleton<BirdsEyeView>.Instance.Disable();
			PlayerSingleton<PlayerCamera>.Instance.LockMouse();
			currentProperty = null;
			if (onConstructionModeDisabled != null)
			{
				onConstructionModeDisabled();
			}
		}

		public void DeployConstructable(ConstructionMenu.ConstructionMenuListing listing)
		{
			isDeployingConstructable = true;
			if (Registry.GetConstructable(listing.ID)._constructionHandler_Asset != null)
			{
				constructHandler = UnityEngine.Object.Instantiate(Registry.GetConstructable(listing.ID)._constructionHandler_Asset, base.transform);
				constructHandler.GetComponent<ConstructStart_Base>().StartConstruction(listing.ID);
			}
			else
			{
				Console.LogWarning("Constructable doesn't have a construction handler!");
			}
		}

		public void StopConstructableDeploy()
		{
			isDeployingConstructable = false;
			constructHandler.GetComponent<ConstructStop_Base>().StopConstruction();
		}

		public void MoveConstructable(Constructable_GridBased c)
		{
			isMovingConstructable = true;
			if (c._constructionHandler_Asset != null)
			{
				constructHandler = UnityEngine.Object.Instantiate(c._constructionHandler_Asset, base.transform);
				constructHandler.GetComponent<ConstructStart_Base>().StartConstruction(c.PrefabID, c);
			}
			else
			{
				Console.LogWarning("Constructable doesn't have a construction handler!");
			}
		}

		public void StopMovingConstructable()
		{
			isMovingConstructable = false;
			constructHandler.GetComponent<ConstructStop_Base>().StopConstruction();
		}

		private void Exit(ExitAction exit)
		{
			if (!exit.used && constructionModeEnabled)
			{
				if (isDeployingConstructable)
				{
					exit.used = true;
					Singleton<ConstructionMenu>.Instance.ClearSelectedListing();
				}
				else if (isMovingConstructable)
				{
					exit.used = true;
					StopMovingConstructable();
				}
				else if (exit.exitType == ExitType.Escape)
				{
					exit.used = true;
					ExitConstructionMode();
				}
			}
		}

		public Constructable_GridBased CreateConstructable_GridBased(string ID, ScheduleOne.Tiles.Grid grid, Vector2 originCoordinate, float rotation)
		{
			Constructable_GridBased component = UnityEngine.Object.Instantiate(Registry.GetPrefab(ID), null).GetComponent<Constructable_GridBased>();
			component.InitializeConstructable_GridBased(grid, originCoordinate, rotation);
			networkObject.Spawn(component.gameObject);
			return component;
		}

		public Constructable CreateConstructable(string prefabID)
		{
			return UnityEngine.Object.Instantiate(Registry.GetPrefab(prefabID), null).GetComponent<Constructable>();
		}
	}
}
