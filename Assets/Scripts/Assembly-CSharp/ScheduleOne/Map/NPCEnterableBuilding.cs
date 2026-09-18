using System;
using System.Collections.Generic;
using System.Linq;
using EasyButtons;
using ScheduleOne.Audio;
using ScheduleOne.DevUtilities;
using ScheduleOne.Doors;
using ScheduleOne.NPCs;
using ScheduleOne.PlayerScripts;
using UnityEngine;

namespace ScheduleOne.Map
{
	public class NPCEnterableBuilding : MonoBehaviour, IGUIDRegisterable
	{
		public const float DOOR_SOUND_DISTANCE_LIMIT = 15f;

		[Header("Settings")]
		public string BuildingName;

		[SerializeField]
		protected string BakedGUID = string.Empty;

		[Header("References")]
		public StaticDoor[] Doors;

		[Header("Readonly")]
		[SerializeField]
		private List<NPC> Occupants = new List<NPC>();

		public Guid GUID { get; protected set; }

		public int OccupantCount => Occupants.Count;

		protected virtual void Awake()
		{
			if (!GUIDManager.IsGUIDValid(BakedGUID))
			{
				Console.LogError(base.gameObject.name + "'s baked GUID is not valid! Bad.");
			}
			GUID = new Guid(BakedGUID);
			GUIDManager.RegisterObject(this);
			if (Doors.Length == 0)
			{
				GetDoors();
				if (Doors.Length == 0)
				{
					Console.LogError(BuildingName + " has no doors! NPCs won't be able to enter the building.");
				}
			}
		}

		public void SetGUID(Guid guid)
		{
			GUID = guid;
			GUIDManager.RegisterObject(this);
		}

		public virtual void NPCEnteredBuilding(NPC npc)
		{
			if (!Occupants.Contains(npc))
			{
				Occupants.Add(npc);
			}
			if (PlayerSingleton<PlayerCamera>.InstanceExists && !(Vector3.Distance(PlayerSingleton<PlayerCamera>.Instance.transform.position, npc.Movement.FootPosition) > 15f))
			{
				AudioSourceController audioSourceController = UnityEngine.Object.Instantiate(Singleton<AudioManager>.Instance.DoorOpen, NetworkSingleton<GameManager>.Instance.Temp.transform);
				audioSourceController.transform.position = npc.Avatar.transform.position;
				audioSourceController.Play();
				UnityEngine.Object.Destroy(audioSourceController.gameObject, audioSourceController.AudioSource.clip.length);
			}
		}

		public virtual void NPCExitedBuilding(NPC npc)
		{
			Occupants.Remove(npc);
			if (PlayerSingleton<PlayerCamera>.InstanceExists && !(Vector3.Distance(PlayerSingleton<PlayerCamera>.Instance.transform.position, npc.Avatar.transform.position) > 15f) && Singleton<AudioManager>.InstanceExists && NetworkSingleton<GameManager>.InstanceExists)
			{
				AudioSourceController audioSourceController = UnityEngine.Object.Instantiate(Singleton<AudioManager>.Instance.DoorClose, NetworkSingleton<GameManager>.Instance.Temp.transform);
				audioSourceController.Play();
				UnityEngine.Object.Destroy(audioSourceController.gameObject, audioSourceController.AudioSource.clip.length);
			}
		}

		[Button]
		public void GetDoors()
		{
			Doors = GetComponentsInChildren<StaticDoor>();
		}

		public List<NPC> GetSummonableNPCs()
		{
			return Occupants.Where((NPC npc) => npc.CanBeSummoned).ToList();
		}

		public StaticDoor GetClosestDoor(Vector3 pos, bool useableOnly)
		{
			return (from door in Doors
				where !useableOnly || door.Usable
				orderby Vector3.Distance(door.transform.position, pos)
				select door).FirstOrDefault();
		}
	}
}
