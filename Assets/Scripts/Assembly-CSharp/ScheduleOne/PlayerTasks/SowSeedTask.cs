using System;
using ScheduleOne.DevUtilities;
using ScheduleOne.Growing;
using ScheduleOne.ObjectScripts;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Trash;
using ScheduleOne.UI;
using ScheduleOne.Variables;
using UnityEngine;

namespace ScheduleOne.PlayerTasks
{
	public class SowSeedTask : Task
	{
		protected Pot pot;

		protected SeedDefinition definition;

		protected FunctionalSeed seed;

		private bool seedExitedVial;

		private bool seedReachedDestination;

		private bool successfullyPlanted;

		private float weedSeedStationaryTime;

		private bool capRemoved;

		public override string TaskName { get; protected set; } = "Sow seed";


		public SowSeedTask(Pot _pot, SeedDefinition def)
		{
			if (_pot == null)
			{
				Console.LogWarning("PourIntoPotTask: pot null");
				StopTask();
				return;
			}
			if (def == null)
			{
				Console.LogWarning("SowSeedTask: seed definition null");
				StopTask();
				return;
			}
			ClickDetectionEnabled = true;
			pot = _pot;
			pot.TaskBounds.gameObject.SetActive(value: true);
			definition = def;
			pot.SetPlayerUser(Player.Local.NetworkObject);
			base.CurrentInstruction = "Click cap to remove";
			pot.PositionCameraContainer();
			PlayerSingleton<PlayerCamera>.Instance.OverrideTransform(pot.CloseupPosition.position, pot.CloseupPosition.rotation, 0.25f);
			PlayerSingleton<PlayerCamera>.Instance.OverrideFOV(60f, 0.25f);
			PlayerSingleton<PlayerCamera>.Instance.FreeMouse();
			PlayerSingleton<PlayerMovement>.Instance.canMove = false;
			PlayerSingleton<PlayerInventory>.Instance.SetInventoryEnabled(enabled: false);
			seed = UnityEngine.Object.Instantiate(def.FunctionSeedPrefab, GameObject.Find("_Temp").transform).GetComponent<FunctionalSeed>();
			seed.transform.position = pot.SeedStartPoint.position;
			Vector3 vector = PlayerSingleton<PlayerCamera>.Instance.transform.position - seed.transform.position;
			seed.transform.forward = new Vector3(vector.x, 0f, vector.z);
			FunctionalSeed functionalSeed = seed;
			functionalSeed.onSeedExitVial = (Action)Delegate.Combine(functionalSeed.onSeedExitVial, new Action(OnSeedExitVial));
			Singleton<InputPromptsCanvas>.Instance.LoadModule("pourable");
			pot.SetSoilState(Pot.ESoilState.Parted);
			SoilChunk[] soilChunks = pot.SoilChunks;
			for (int i = 0; i < soilChunks.Length; i++)
			{
				soilChunks[i].ClickableEnabled = false;
			}
		}

		public override void Update()
		{
			base.Update();
			if (seedExitedVial && !seedReachedDestination && capRemoved)
			{
				seed.Vial.idleUpForce = 0f;
				if (seed.SeedRigidbody.velocity.magnitude < 0.08f)
				{
					weedSeedStationaryTime += Time.deltaTime;
				}
				else
				{
					weedSeedStationaryTime = 0f;
				}
				if (weedSeedStationaryTime > 0.2f && Vector3.Distance(seed.SeedCollider.transform.position, pot.SeedRestingPoint.position) < 0.1f)
				{
					OnSeedReachedDestination();
				}
			}
			if (!capRemoved)
			{
				if (seed.Cap.Removed)
				{
					capRemoved = true;
				}
			}
			else
			{
				base.CurrentInstruction = "Drop seed into hole";
			}
			seed.SeedBlocker.enabled = !capRemoved;
			if (!seedReachedDestination)
			{
				return;
			}
			int num = 0;
			SoilChunk[] soilChunks = pot.SoilChunks;
			for (int i = 0; i < soilChunks.Length; i++)
			{
				if (soilChunks[i].CurrentLerp > 0f)
				{
					num++;
				}
			}
			base.CurrentInstruction = "Click soil chunks to bury seed (" + num + "/" + pot.SoilChunks.Length + ")";
			if (num == pot.SoilChunks.Length)
			{
				Success();
			}
		}

		public override void Success()
		{
			successfullyPlanted = true;
			PlayerSingleton<PlayerInventory>.Instance.RemoveAmountOfItem(definition.ID);
			NetworkSingleton<TrashManager>.Instance.CreateTrashItem(seed.TrashPrefab.ID, Player.Local.Avatar.CenterPoint, UnityEngine.Random.rotation);
			pot.SendPlantSeed(definition.ID, 0f, -1f, -1f);
			pot.SetSoilState(Pot.ESoilState.Packed);
			float value = NetworkSingleton<VariableDatabase>.Instance.GetValue<float>("SownSeedsCount");
			NetworkSingleton<VariableDatabase>.Instance.SetVariableValue("SownSeedsCount", (value + 1f).ToString());
			base.Success();
		}

		public override void StopTask()
		{
			base.StopTask();
			PlayerSingleton<PlayerCamera>.Instance.StopTransformOverride(0.25f);
			PlayerSingleton<PlayerCamera>.Instance.StopFOVOverride(0.25f);
			PlayerSingleton<PlayerCamera>.Instance.LockMouse();
			PlayerSingleton<PlayerInventory>.Instance.SetInventoryEnabled(enabled: true);
			PlayerSingleton<PlayerMovement>.Instance.canMove = true;
			UnityEngine.Object.Destroy(seed.gameObject);
			Singleton<InputPromptsCanvas>.Instance.UnloadModule();
			if (!successfullyPlanted)
			{
				pot.SetSoilState(Pot.ESoilState.Flat);
			}
			SoilChunk[] soilChunks = pot.SoilChunks;
			foreach (SoilChunk obj in soilChunks)
			{
				obj.StopLerp();
				obj.ClickableEnabled = false;
			}
			pot.TaskBounds.gameObject.SetActive(value: false);
			pot.SetPlayerUser(null);
		}

		private void OnSeedExitVial()
		{
			seedExitedVial = true;
		}

		private void OnSeedReachedDestination()
		{
			seedReachedDestination = true;
			seed.SeedCollider.GetComponent<Rigidbody>().isKinematic = true;
			seed.SeedCollider.GetComponent<Draggable>().enabled = false;
			seed.Vial.gameObject.SetActive(value: false);
			PlayerSingleton<PlayerCamera>.Instance.OverrideFOV(50f, 0.25f);
			SoilChunk[] soilChunks = pot.SoilChunks;
			for (int i = 0; i < soilChunks.Length; i++)
			{
				soilChunks[i].ClickableEnabled = true;
			}
		}
	}
}
