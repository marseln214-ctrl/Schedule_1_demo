using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Pathfinding;
using ScheduleOne.DevUtilities;
using ScheduleOne.Math;
using UnityEngine;

namespace ScheduleOne.Vehicles.AI
{
	public class NavigationUtility
	{
		public enum ENavigationCalculationResult
		{
			Success = 0,
			Failed = 1
		}

		public delegate void NavigationCalculationCallback(ENavigationCalculationResult result, PathSmoothingUtility.SmoothedPath path);

		public delegate void PathGroupEvent(PathGroup calculatedGroup);

		public const float ROAD_MULTIPLIER = 1f;

		public const float OFFROAD_MULTIPLIER = 3f;

		public static Coroutine CalculatePath(Vector3 startPosition, Vector3 destination, NavigationSettings navSettings, DriveFlags flags, Seeker generalSeeker, Seeker roadSeeker, NavigationCalculationCallback callback)
		{
			PathGroup lastGeneratedPathGroup;
			bool pathGroupGenerated;
			Path lastCalculatedPath;
			return Singleton<CoroutineService>.Instance.StartCoroutine(Routine());
			void PathCompleted(Path p)
			{
				lastCalculatedPath = p;
			}
			void PathGroupCallback(PathGroup pg)
			{
				lastGeneratedPathGroup = pg;
				pathGroupGenerated = true;
			}
			IEnumerator Routine()
			{
				_ = Time.realtimeSinceStartup;
				PathGroup finalGroup = null;
				if (flags.UseRoads)
				{
					List<NodeLink> closestNodeLinks = NodeLink.GetClosestLinks(startPosition);
					List<NodeLink> nodeLinksClosestToLocation = NodeLink.GetClosestLinks(destination);
					FunnelZone funnelZone = FunnelZone.GetFunnelZone(destination);
					if (funnelZone != null)
					{
						nodeLinksClosestToLocation = NodeLink.GetClosestLinks(funnelZone.entryPoint.position);
					}
					int entryPointChecks = 3;
					List<Vector3> checkedEntryPoints = new List<Vector3>();
					List<PathGroup> groups2 = new List<PathGroup>();
					for (int i = 0; i < entryPointChecks; i++)
					{
						Vector3 entryPoint = closestNodeLinks[i].GetClosestPoint(startPosition);
						if (DoesCloseDistanceExist(checkedEntryPoints, entryPoint, 1f))
						{
							entryPointChecks += 2;
						}
						else
						{
							checkedEntryPoints.Add(entryPoint);
							int exitPointChecks = 3;
							List<Vector3> checkedExitPoints = new List<Vector3>();
							for (int j = 0; j < exitPointChecks; j++)
							{
								NodeLink nodeLink = nodeLinksClosestToLocation[j];
								Vector3 closestPoint = nodeLink.GetClosestPoint(destination);
								if (DoesCloseDistanceExist(checkedExitPoints, closestPoint, 1f))
								{
									exitPointChecks += 2;
								}
								else
								{
									checkedExitPoints.Add(closestPoint);
									lastGeneratedPathGroup = null;
									pathGroupGenerated = false;
									Singleton<CoroutineService>.Instance.StartCoroutine(GenerateNavigationGroup(startPosition, entryPoint, nodeLink, closestPoint, destination, generalSeeker, roadSeeker, PathGroupCallback));
									yield return new WaitUntil(() => pathGroupGenerated);
									if (lastGeneratedPathGroup != null)
									{
										groups2.Add(lastGeneratedPathGroup);
									}
								}
							}
						}
					}
					if (groups2.Count > 0)
					{
						groups2 = groups2.OrderBy((PathGroup x) => ((x.startToEntryPath.vectorPath == null) ? 0f : (x.startToEntryPath.GetTotalLength() * 3f)) + ((x.entryToExitPath.vectorPath == null) ? 0f : (x.entryToExitPath.GetTotalLength() * 1f)) + ((x.exitToDestinationPath.vectorPath == null) ? 0f : (x.exitToDestinationPath.GetTotalLength() * 3f))).ToList();
						finalGroup = groups2[0];
						if (navSettings.endAtRoad)
						{
							finalGroup.exitToDestinationPath = null;
						}
						lastCalculatedPath = null;
						generalSeeker.StartPath(startPosition, destination, PathCompleted);
						yield return new WaitUntil(() => lastCalculatedPath != null);
						Path path = lastCalculatedPath;
						if (finalGroup.startToEntryPath.GetTotalLength() > path.GetTotalLength())
						{
							finalGroup = new PathGroup
							{
								startToEntryPath = path
							};
						}
					}
				}
				else
				{
					lastCalculatedPath = null;
					generalSeeker.StartPath(startPosition, destination, PathCompleted);
					yield return new WaitUntil(() => lastCalculatedPath != null);
					if (!lastCalculatedPath.error)
					{
						finalGroup = new PathGroup
						{
							entryToExitPath = lastCalculatedPath
						};
					}
				}
				int num = 0;
				if (finalGroup != null)
				{
					if (finalGroup.startToEntryPath != null)
					{
						num += finalGroup.startToEntryPath.vectorPath.Count;
					}
					if (finalGroup.entryToExitPath != null)
					{
						num += finalGroup.entryToExitPath.vectorPath.Count;
					}
					if (finalGroup.exitToDestinationPath != null)
					{
						num += finalGroup.exitToDestinationPath.vectorPath.Count;
					}
				}
				if (finalGroup == null || num < 2)
				{
					if (callback != null)
					{
						callback(ENavigationCalculationResult.Failed, null);
					}
				}
				else
				{
					AdjustEntryPoint(finalGroup);
					if (finalGroup.entryToExitPath != null && finalGroup.exitToDestinationPath != null)
					{
						AdjustExitPoint(finalGroup);
					}
					PathSmoothingUtility.SmoothedPath smoothedPath = GetSmoothedPath(finalGroup);
					callback(ENavigationCalculationResult.Success, smoothedPath);
				}
			}
		}

		private static void AdjustExitPoint(PathGroup group)
		{
			if (group.entryToExitPath.vectorPath.Count < 4 || group.exitToDestinationPath.vectorPath.Count < 2 || group.exitToDestinationPath.GetTotalLength() < 5f)
			{
				return;
			}
			for (int num = Mathf.Min(5, group.exitToDestinationPath.vectorPath.Count - 1); num >= 0; num--)
			{
				Vector3 vector = group.exitToDestinationPath.vectorPath[num];
				Vector3 vector2 = Vector3.zero;
				float num2 = float.MaxValue;
				int num3 = 0;
				for (int i = 0; i < 3; i++)
				{
					int num4 = group.entryToExitPath.vectorPath.Count - 1 - i;
					int index = num4 - 1;
					Vector3 line_end = group.entryToExitPath.vectorPath[num4];
					Vector3 line_start = group.entryToExitPath.vectorPath[index];
					Vector3 closestPointOnFiniteLine = GetClosestPointOnFiniteLine(vector, line_start, line_end);
					if (Vector3.Distance(vector, closestPointOnFiniteLine) < num2)
					{
						num2 = Vector3.Distance(vector, closestPointOnFiniteLine);
						vector2 = closestPointOnFiniteLine;
						num3 = num4;
					}
				}
				if (vector2 == Vector3.zero)
				{
					Debug.LogWarning("Failed to find closest entry-to-exit path point");
					break;
				}
				float num5 = 0f;
				for (int j = 0; j < num; j++)
				{
					num5 += Vector3.Distance(group.exitToDestinationPath.vectorPath[j], group.exitToDestinationPath.vectorPath[j + 1]);
				}
				num5 += Vector3.Distance(vector2, group.entryToExitPath.vectorPath[num3]);
				for (int k = num3; k < group.entryToExitPath.vectorPath.Count - 1; k++)
				{
					num5 += Vector3.Distance(group.entryToExitPath.vectorPath[k], group.entryToExitPath.vectorPath[k + 1]);
				}
				if (num2 < num5 * 0.5f)
				{
					for (int l = num3; l < group.entryToExitPath.vectorPath.Count; l++)
					{
						group.entryToExitPath.vectorPath.RemoveAt(num3);
					}
					group.entryToExitPath.vectorPath.Insert(num3, vector2);
					for (int m = 0; m < num; m++)
					{
						group.exitToDestinationPath.vectorPath.RemoveAt(0);
					}
					Debug.DrawLine(vector, vector2, Color.green, 1f);
					break;
				}
			}
		}

		private static void AdjustEntryPoint(PathGroup group)
		{
			if (group.startToEntryPath != null && group.startToEntryPath.vectorPath.Count >= 2 && !(group.startToEntryPath.GetTotalLength() < 5f) && group.entryToExitPath != null && group.entryToExitPath.vectorPath.Count >= 2 && !(group.entryToExitPath.GetTotalLength() < 5f))
			{
				float num = 2f;
				Vector3 vector = group.startToEntryPath.vectorPath[group.startToEntryPath.vectorPath.Count - 1];
				Vector3 vector2 = group.startToEntryPath.vectorPath[group.startToEntryPath.vectorPath.Count - 2];
				Vector3 normalized = (vector - vector2).normalized;
				Vector3 value = vector - normalized * num;
				group.startToEntryPath.vectorPath[group.startToEntryPath.vectorPath.Count - 1] = value;
				Vector3 vector3 = group.entryToExitPath.vectorPath[0];
				normalized = (group.entryToExitPath.vectorPath[1] - vector3).normalized;
				Vector3 value2 = vector3 + normalized * num;
				group.entryToExitPath.vectorPath[0] = value2;
			}
		}

		private static bool DoesCloseDistanceExist(List<Vector3> vectorList, Vector3 point, float thresholdDistance)
		{
			foreach (Vector3 vector in vectorList)
			{
				if (Vector3.Distance(vector, point) <= thresholdDistance)
				{
					return true;
				}
			}
			return false;
		}

		private static IEnumerator GenerateNavigationGroup(Vector3 startPoint, Vector3 entryPoint, NodeLink exitLink, Vector3 exitPoint, Vector3 destination, Seeker generalSeeker, Seeker roadSeeker, PathGroupEvent callback)
		{
			Vector3 closestPointOnGraph = AstarUtility.GetClosestPointOnGraph(startPoint, "General Vehicle Graph");
			Vector3 destinationOnGraph = AstarUtility.GetClosestPointOnGraph(destination, "General Vehicle Graph");
			Path lastCalculatedPath = null;
			generalSeeker.StartPath(closestPointOnGraph, entryPoint, PathCompleted);
			yield return new WaitUntil(() => lastCalculatedPath != null);
			if (lastCalculatedPath.error)
			{
				callback(null);
				yield break;
			}
			Path path_StartToEntry = lastCalculatedPath;
			lastCalculatedPath = null;
			Vector3 position = NodeLink.GetClosestLinks(entryPoint)[0].Start.position;
			roadSeeker.StartPath(position, exitLink.Start.position, PathCompleted);
			yield return new WaitUntil(() => lastCalculatedPath != null);
			if (lastCalculatedPath.error)
			{
				callback(null);
				yield break;
			}
			lastCalculatedPath.vectorPath[0] = entryPoint;
			lastCalculatedPath.vectorPath.Add(exitPoint);
			Path path_EntryToExit = lastCalculatedPath;
			lastCalculatedPath = null;
			generalSeeker.StartPath(exitPoint, destinationOnGraph, PathCompleted);
			yield return new WaitUntil(() => lastCalculatedPath != null);
			if (lastCalculatedPath.error)
			{
				callback(null);
				yield break;
			}
			Path exitToDestinationPath = lastCalculatedPath;
			PathGroup pathGroup = new PathGroup();
			pathGroup.entryPoint = entryPoint;
			pathGroup.startToEntryPath = path_StartToEntry;
			pathGroup.entryToExitPath = path_EntryToExit;
			pathGroup.exitToDestinationPath = exitToDestinationPath;
			callback(pathGroup);
			void PathCompleted(Path p)
			{
				lastCalculatedPath = p;
			}
		}

		public static void DrawPath(PathGroup group, float duration = 10f)
		{
			if (group.startToEntryPath != null)
			{
				for (int i = 1; i < group.startToEntryPath.vectorPath.Count; i++)
				{
					Debug.DrawLine(group.startToEntryPath.vectorPath[i], group.startToEntryPath.vectorPath[i - 1], Color.red, duration);
				}
			}
			if (group.entryToExitPath != null)
			{
				for (int j = 1; j < group.entryToExitPath.vectorPath.Count; j++)
				{
					if (j % 2 == 0)
					{
						Debug.DrawLine(group.entryToExitPath.vectorPath[j], group.entryToExitPath.vectorPath[j - 1], Color.blue, duration);
					}
					else
					{
						Debug.DrawLine(group.entryToExitPath.vectorPath[j], group.entryToExitPath.vectorPath[j - 1], Color.white, duration);
					}
				}
			}
			if (group.exitToDestinationPath != null)
			{
				for (int k = 1; k < group.exitToDestinationPath.vectorPath.Count; k++)
				{
					Debug.DrawLine(group.exitToDestinationPath.vectorPath[k], group.exitToDestinationPath.vectorPath[k - 1], Color.yellow, duration);
				}
			}
		}

		private static PathSmoothingUtility.SmoothedPath GetSmoothedPath(PathGroup group)
		{
			List<Vector3> list = new List<Vector3>();
			if (group.startToEntryPath != null)
			{
				list.AddRange(group.startToEntryPath.vectorPath);
			}
			if (group.entryToExitPath != null)
			{
				list.AddRange(group.entryToExitPath.vectorPath);
			}
			if (group.exitToDestinationPath != null)
			{
				list.AddRange(group.exitToDestinationPath.vectorPath);
			}
			return PathSmoothingUtility.CalculateSmoothedPath(list);
		}

		public static Vector3 SampleVehicleGraph(Vector3 destination)
		{
			NNConstraint nNConstraint = new NNConstraint();
			nNConstraint.graphMask = GraphMask.FromGraphName("General Vehicle Graph");
			return AstarPath.active.GetNearest(destination, nNConstraint).position;
		}

		public static Vector3 GetClosestPointOnFiniteLine(Vector3 point, Vector3 line_start, Vector3 line_end)
		{
			Vector3 vector = line_end - line_start;
			float magnitude = vector.magnitude;
			vector.Normalize();
			float num = Mathf.Clamp(Vector3.Dot(point - line_start, vector), 0f, magnitude);
			return line_start + vector * num;
		}
	}
}
