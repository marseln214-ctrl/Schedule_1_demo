using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace ScheduleOne.NPCs
{
	public class NPCPathCache
	{
		[Serializable]
		public class PathCache
		{
			public Vector3 Start;

			public Vector3 End;

			public NavMeshPath Path;

			public PathCache(Vector3 start, Vector3 end, NavMeshPath path)
			{
				Start = start;
				End = end;
				Path = path;
			}
		}

		public List<PathCache> Paths { get; private set; } = new List<PathCache>();


		public NavMeshPath GetPath(Vector3 start, Vector3 end, float sqrMaxDistance)
		{
			foreach (PathCache path in Paths)
			{
				if ((path.Start - start).sqrMagnitude < sqrMaxDistance && (path.End - end).sqrMagnitude < sqrMaxDistance)
				{
					return path.Path;
				}
			}
			return null;
		}

		public void AddPath(Vector3 start, Vector3 end, NavMeshPath path)
		{
			Paths.Add(new PathCache(start, end, path));
		}
	}
}
