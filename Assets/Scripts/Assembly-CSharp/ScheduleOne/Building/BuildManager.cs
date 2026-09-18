using System;
using System.Collections.Generic;
using FishNet.Object;
using ScheduleOne.Audio;
using ScheduleOne.DevUtilities;
using ScheduleOne.EntityFramework;
using ScheduleOne.ItemFramework;
using ScheduleOne.Storage;
using ScheduleOne.Tiles;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

namespace ScheduleOne.Building
{
	public class BuildManager : Singleton<BuildManager>
	{
		[Serializable]
		public class BuildSound
		{
			public BuildableItemDefinition.EBuildSoundType Type;

			public AudioSourceController Sound;
		}

		public List<BuildSound> PlaceSounds = new List<BuildSound>();

		[Header("References")]
		[SerializeField]
		protected Transform tempContainer;

		public NetworkObject networkObject;

		[Header("Prefabs")]
		[SerializeField]
		protected GameObject storedItemBuildHandler;

		[SerializeField]
		protected GameObject cashBuildHandler;

		[Header("Materials")]
		public Material ghostMaterial_White;

		public Material ghostMaterial_Red;

		public Transform _tempContainer => tempContainer;

		public bool isBuilding { get; protected set; }

		public GameObject currentBuildHandler { get; protected set; }

		protected override void Awake()
		{
			base.Awake();
		}

		public void StartBuilding(ItemInstance item)
		{
			if (!(item.Definition is BuildableItemDefinition))
			{
				Console.LogError("StartBuilding called but not passed BuildableItemDefinition");
				return;
			}
			if (isBuilding)
			{
				Console.LogWarning("StartBuilding called but building is already happening!");
				StopBuilding();
			}
			BuildableItem builtItem = (item.Definition as BuildableItemDefinition).BuiltItem;
			if (builtItem == null)
			{
				Console.LogWarning("itemToBuild is null!");
				return;
			}
			isBuilding = true;
			currentBuildHandler = UnityEngine.Object.Instantiate(builtItem.BuildHandler, tempContainer);
			currentBuildHandler.GetComponent<BuildStart_Base>().StartBuilding(item);
		}

		public void StartBuildingStoredItem(ItemInstance item)
		{
			if (!(item.Definition is StorableItemDefinition))
			{
				Console.LogError("StartBuildingStoredItem called but not passed StorableItemDefinition");
				return;
			}
			if (isBuilding)
			{
				Console.LogWarning("StartBuildingStoredItem called but building is already happening!");
				StopBuilding();
			}
			isBuilding = true;
			currentBuildHandler = UnityEngine.Object.Instantiate(storedItemBuildHandler, tempContainer);
			currentBuildHandler.GetComponent<BuildStart_Base>().StartBuilding(item);
		}

		public void StartPlacingCash(ItemInstance item)
		{
			if (isBuilding)
			{
				Console.LogWarning("StartPlacingCash called but building is already happening!");
				StopBuilding();
			}
			isBuilding = true;
			currentBuildHandler = UnityEngine.Object.Instantiate(cashBuildHandler, tempContainer);
			currentBuildHandler.GetComponent<BuildStart_Cash>().StartBuilding(item);
		}

		public void StopBuilding()
		{
			isBuilding = false;
			currentBuildHandler.GetComponent<BuildStop_Base>().Stop_Building();
		}

		public void PlayBuildSound(BuildableItemDefinition.EBuildSoundType type, Vector3 point)
		{
			BuildSound buildSound = PlaceSounds.Find((BuildSound s) => s.Type == type);
			if (buildSound != null)
			{
				buildSound.Sound.transform.position = point;
				buildSound.Sound.Play();
			}
		}

		public void DisableColliders(GameObject obj)
		{
			Collider[] componentsInChildren = obj.GetComponentsInChildren<Collider>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].enabled = false;
			}
		}

		public void DisableLights(GameObject obj)
		{
			Light[] componentsInChildren = obj.GetComponentsInChildren<Light>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].enabled = false;
			}
		}

		public void DisableNetworking(GameObject obj)
		{
			NetworkObject[] componentsInChildren = obj.GetComponentsInChildren<NetworkObject>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				UnityEngine.Object.Destroy(componentsInChildren[i]);
			}
		}

		public void DisableSpriteRenderers(GameObject obj)
		{
			SpriteRenderer[] componentsInChildren = obj.GetComponentsInChildren<SpriteRenderer>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].enabled = false;
			}
		}

		public void ApplyMaterial(GameObject obj, Material mat, bool allMaterials = true)
		{
			MeshRenderer[] componentsInChildren = obj.GetComponentsInChildren<MeshRenderer>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				if ((bool)componentsInChildren[i].gameObject.GetComponentInParent<OverrideGhostMaterial>())
				{
					continue;
				}
				if (allMaterials)
				{
					Material[] materials = componentsInChildren[i].materials;
					for (int j = 0; j < materials.Length; j++)
					{
						materials[j] = mat;
					}
					componentsInChildren[i].materials = materials;
				}
				else
				{
					componentsInChildren[i].material = mat;
				}
			}
		}

		public void DisableNavigation(GameObject obj)
		{
			NavMeshObstacle[] componentsInChildren = obj.GetComponentsInChildren<NavMeshObstacle>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].enabled = false;
			}
			NavMeshSurface[] componentsInChildren2 = obj.GetComponentsInChildren<NavMeshSurface>();
			for (int j = 0; j < componentsInChildren2.Length; j++)
			{
				componentsInChildren2[j].enabled = false;
			}
			NavMeshLink[] componentsInChildren3 = obj.GetComponentsInChildren<NavMeshLink>();
			for (int k = 0; k < componentsInChildren3.Length; k++)
			{
				componentsInChildren3[k].enabled = false;
			}
		}

		public void DisableCanvases(GameObject obj)
		{
			Canvas[] componentsInChildren = obj.GetComponentsInChildren<Canvas>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].enabled = false;
			}
		}

		public GridItem CreateGridItem(ItemInstance item, ScheduleOne.Tiles.Grid grid, Vector2 originCoordinate, int rotation, string guid = "")
		{
			BuildableItemDefinition buildableItemDefinition = item.Definition as BuildableItemDefinition;
			if (buildableItemDefinition == null)
			{
				Console.LogError("BuildGridItem called but could not find BuildableItemDefinition");
				return null;
			}
			if (grid == null)
			{
				Console.LogError("BuildGridItem called and passed null grid");
				return null;
			}
			string gUID = (string.IsNullOrEmpty(guid) ? GUIDManager.GenerateUniqueGUID().ToString() : guid);
			GridItem component = UnityEngine.Object.Instantiate(buildableItemDefinition.BuiltItem.gameObject, null).GetComponent<GridItem>();
			component.SetLocallyBuilt();
			component.InitializeGridItem(item, grid, originCoordinate, rotation, gUID);
			networkObject.Spawn(component.gameObject);
			return component;
		}

		public ProceduralGridItem CreateProceduralGridItem(ItemInstance item, int rotationAngle, List<CoordinateProceduralTilePair> matches, string guid = "")
		{
			BuildableItemDefinition buildableItemDefinition = item.Definition as BuildableItemDefinition;
			if (buildableItemDefinition == null)
			{
				Console.LogError("BuildProceduralGridItem called but could not find BuildableItemDefinition");
				return null;
			}
			string gUID = (string.IsNullOrEmpty(guid) ? GUIDManager.GenerateUniqueGUID().ToString() : guid);
			ProceduralGridItem component = UnityEngine.Object.Instantiate(buildableItemDefinition.BuiltItem.gameObject, null).GetComponent<ProceduralGridItem>();
			component.SetLocallyBuilt();
			component.InitializeProceduralGridItem(item, rotationAngle, matches, gUID);
			networkObject.Spawn(component.gameObject);
			return component;
		}

		public void CreateStoredItem(StorableItemInstance item, IStorageEntity parentStorageEntity, StorageGrid grid, Vector2 originCoord, float rotation)
		{
			if (parentStorageEntity == null)
			{
				Console.LogWarning("CreateStoredItem: parentStorageEntity is null");
			}
			else if (item.Quantity != 1)
			{
				Console.LogWarning("CreateStoredItem: item quantity is '" + item.Quantity + "'. It should be 1!");
			}
		}
	}
}
