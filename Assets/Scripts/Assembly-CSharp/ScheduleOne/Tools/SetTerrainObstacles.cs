using UnityEngine;
using UnityEngine.AI;

namespace ScheduleOne.Tools
{
	public class SetTerrainObstacles : MonoBehaviour
	{
		public BoxCollider Bounds;

		private TreeInstance[] Obstacle;

		private Terrain terrain;

		private float width;

		private float lenght;

		private float hight;

		private bool isError;

		private void Start()
		{
			if (Application.isEditor || Debug.isDebugBuild)
			{
				Console.Log("Skipping SetTerrainObstacles in Editor");
				return;
			}
			terrain = Terrain.activeTerrain;
			Obstacle = terrain.terrainData.treeInstances;
			lenght = terrain.terrainData.size.z;
			width = terrain.terrainData.size.x;
			hight = terrain.terrainData.size.y;
			int num = 0;
			GameObject gameObject = new GameObject("Tree_Obstacles");
			gameObject.transform.SetParent(base.transform);
			TreeInstance[] obstacle = Obstacle;
			for (int i = 0; i < obstacle.Length; i++)
			{
				TreeInstance treeInstance = obstacle[i];
				Vector3 position = Vector3.Scale(treeInstance.position, terrain.terrainData.size) + terrain.transform.position;
				if (!(position.x < Bounds.bounds.min.x) && !(position.x > Bounds.bounds.max.x) && !(position.z < Bounds.bounds.min.z) && !(position.z > Bounds.bounds.max.z))
				{
					Quaternion rotation = Quaternion.AngleAxis(treeInstance.rotation * 57.29578f, Vector3.up);
					GameObject obj = new GameObject("Obstacle" + num);
					obj.transform.SetParent(gameObject.transform);
					obj.transform.position = position;
					obj.transform.rotation = rotation;
					obj.AddComponent<NavMeshObstacle>();
					NavMeshObstacle component = obj.GetComponent<NavMeshObstacle>();
					component.carving = true;
					component.carveOnlyStationary = true;
					if (terrain.terrainData.treePrototypes[treeInstance.prototypeIndex].prefab.GetComponent<Collider>() == null)
					{
						isError = true;
						Debug.LogError("ERROR  There is no CapsuleCollider or BoxCollider attached to ''" + terrain.terrainData.treePrototypes[treeInstance.prototypeIndex].prefab.name + "'' please add one of them.");
						break;
					}
					Collider component2 = terrain.terrainData.treePrototypes[treeInstance.prototypeIndex].prefab.GetComponent<Collider>();
					if (!(component2.GetType() == typeof(CapsuleCollider)) && !(component2.GetType() == typeof(BoxCollider)))
					{
						isError = true;
						Debug.LogError("ERROR  There is no CapsuleCollider or BoxCollider attached to ''" + terrain.terrainData.treePrototypes[treeInstance.prototypeIndex].prefab.name + "'' please add one of them.");
						break;
					}
					if (component2.GetType() == typeof(CapsuleCollider))
					{
						CapsuleCollider component3 = terrain.terrainData.treePrototypes[treeInstance.prototypeIndex].prefab.GetComponent<CapsuleCollider>();
						component.shape = NavMeshObstacleShape.Capsule;
						component.center = component3.center;
						component.radius = component3.radius;
						component.height = component3.height;
					}
					else if (component2.GetType() == typeof(BoxCollider))
					{
						BoxCollider component4 = terrain.terrainData.treePrototypes[treeInstance.prototypeIndex].prefab.GetComponent<BoxCollider>();
						component.shape = NavMeshObstacleShape.Box;
						component.center = component4.center;
						component.size = component4.size;
					}
					num++;
				}
			}
			if (!isError)
			{
				Debug.Log(Obstacle.Length + " NavMeshObstacles were succesfully added to scene");
			}
		}
	}
}
