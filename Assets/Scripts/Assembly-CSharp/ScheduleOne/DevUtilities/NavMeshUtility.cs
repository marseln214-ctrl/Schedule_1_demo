using System.Collections.Generic;
using ScheduleOne.EntityFramework;
using ScheduleOne.Management;
using ScheduleOne.NPCs;
using UnityEngine;
using UnityEngine.AI;

namespace ScheduleOne.DevUtilities
{
	public static class NavMeshUtility
	{
		public const float SAMPLE_MAX_DISTANCE = 2f;

		public static Dictionary<Vector3, Vector3> SampleCache = new Dictionary<Vector3, Vector3>();

		public static List<Vector3> sampleCacheKeys = new List<Vector3>();

		public const float SAMPLE_CACHE_MAX_SQR_DIST = 1f;

		public const float MAX_CACHE_SIZE = 10000f;

		public static float GetPathLength(NavMeshPath path)
		{
			if (path == null)
			{
				return 0f;
			}
			float num = 0f;
			for (int i = 1; i < path.corners.Length; i++)
			{
				num += Vector3.Distance(path.corners[i - 1], path.corners[i]);
			}
			return num;
		}

		public static Transform GetAccessPoint(ITransitEntity entity, NPC npc)
		{
			float num = float.MaxValue;
			Transform result = null;
			BuildableItem buildableItem = entity as BuildableItem;
			for (int i = 0; i < entity.AccessPoints.Length; i++)
			{
				if (buildableItem != null && !buildableItem.ParentProperty.DoBoundsContainPoint(entity.AccessPoints[i].position))
				{
					Debug.LogWarning("Access point is not within property bounds!");
					continue;
				}
				if (!npc.Movement.CanGetTo(entity.AccessPoints[i].position, 1f, out var path))
				{
					Debug.LogWarning("Path not found!");
					Debug.DrawLine(npc.transform.position, entity.AccessPoints[i].position, Color.red, 5f);
					continue;
				}
				float num2 = ((path != null) ? GetPathLength(path) : Vector3.Distance(npc.transform.position, entity.AccessPoints[i].position));
				if (num2 < num)
				{
					num = num2;
					result = entity.AccessPoints[i];
				}
			}
			return result;
		}

		public static bool IsAtTransitEntity(ITransitEntity entity, NPC npc)
		{
			for (int i = 0; i < entity.AccessPoints.Length; i++)
			{
				if (npc.Movement.IsAsCloseAsPossible(entity.AccessPoints[i].transform.position))
				{
					return true;
				}
			}
			return false;
		}

		public static int GetNavMeshAgentID(string name)
		{
			for (int i = 0; i < NavMesh.GetSettingsCount(); i++)
			{
				NavMeshBuildSettings settingsByIndex = NavMesh.GetSettingsByIndex(i);
				if (name == NavMesh.GetSettingsNameFromID(settingsByIndex.agentTypeID))
				{
					return settingsByIndex.agentTypeID;
				}
			}
			return -1;
		}

		public static bool SamplePosition(Vector3 sourcePosition, out NavMeshHit hit, float maxDistance, int areaMask, bool useCache = true)
		{
			if (useCache)
			{
				for (int i = 0; i < sampleCacheKeys.Count; i++)
				{
					if (Vector3.SqrMagnitude(sourcePosition - sampleCacheKeys[i]) < 1f)
					{
						hit = default(NavMeshHit);
						hit.position = SampleCache[sampleCacheKeys[i]];
						return true;
					}
				}
			}
			bool num = NavMesh.SamplePosition(sourcePosition, out hit, maxDistance, areaMask);
			if (num)
			{
				if ((float)sampleCacheKeys.Count >= 10000f)
				{
					Console.LogWarning("Sample cache is full! Clearing cache...");
					ClearCache();
				}
				Vector3 vector = Quantize(sourcePosition);
				sampleCacheKeys.Add(vector);
				SampleCache.Add(vector, hit.position);
			}
			return num;
		}

		private static Vector3 Quantize(Vector3 position, float precision = 0.1f)
		{
			return new Vector3(Mathf.Round(position.x / precision) * precision, Mathf.Round(position.y / precision) * precision, Mathf.Round(position.z / precision) * precision);
		}

		public static void ClearCache()
		{
			SampleCache.Clear();
			sampleCacheKeys.Clear();
		}
	}
}
