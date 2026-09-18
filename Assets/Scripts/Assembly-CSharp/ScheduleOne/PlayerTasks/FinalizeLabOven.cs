using System.Collections;
using ScheduleOne.DevUtilities;
using ScheduleOne.ItemFramework;
using ScheduleOne.ObjectScripts;
using ScheduleOne.PlayerScripts;
using ScheduleOne.UI.Stations;
using ScheduleOne.Variables;
using UnityEngine;

namespace ScheduleOne.PlayerTasks
{
	public class FinalizeLabOven : Task
	{
		public const float MAX_DISTANCE_FROM_IMPACT_POINT = 0.1f;

		public float SMASH_VELOCITY_THRESHOLD = 1.2f;

		public const int REQUIRED_IMPACTS = 3;

		private Coroutine startSequence;

		private LabOvenHammer hammer;

		private int impactCount;

		private float timeSinceLastImpact = 100f;

		public LabOven Oven { get; private set; }

		public FinalizeLabOven(LabOven oven)
		{
			Oven = oven;
			hammer = oven.CreateHammer();
			hammer.onCollision.AddListener(Collision);
			startSequence = Oven.StartCoroutine(StartSequence());
		}

		public override void Update()
		{
			base.Update();
			timeSinceLastImpact += Time.deltaTime;
		}

		public override void StopTask()
		{
			if (startSequence != null)
			{
				Oven.StopCoroutine(startSequence);
			}
			Object.Destroy(hammer.gameObject);
			Oven.RemoveTrayAnimation.Stop();
			Oven.ResetSquareTray();
			Oven.ClearDecals();
			Oven.OutputVisuals.BlockRefreshes = false;
			Oven.OutputVisuals.RefreshVisuals();
			Oven.Door.SetPosition(0f);
			Oven.Door.SetInteractable(interactable: false);
			Oven.WireTray.SetPosition(0f);
			Oven.Button.SetInteractable(interactable: false);
			Oven.ClearShards();
			Singleton<LabOvenCanvas>.Instance.SetIsOpen(Oven, open: true);
			PlayerSingleton<PlayerCamera>.Instance.OverrideFOV(70f, 0.2f);
			PlayerSingleton<PlayerCamera>.Instance.OverrideTransform(Oven.CameraPosition_Default.position, Oven.CameraPosition_Default.rotation, 0.2f);
			base.StopTask();
		}

		private IEnumerator StartSequence()
		{
			Oven.Door.SetPosition(1f);
			Oven.WireTray.SetPosition(1f);
			yield return new WaitForSeconds(0.5f);
			Oven.SquareTray.SetParent(Oven.transform);
			Oven.RemoveTrayAnimation.Play();
			yield return new WaitForSeconds(0.1f);
			Oven.Door.SetPosition(0f);
			yield return new WaitForSeconds(0.4f);
			PlayerSingleton<PlayerCamera>.Instance.OverrideTransform(Oven.CameraPosition_Breaking.position, Oven.CameraPosition_Breaking.rotation, 0.25f);
			base.CurrentInstruction = "Use hammer to break up the product";
		}

		public void Collision(Collision col)
		{
			if (col.collider != Oven.CookedLiquidCollider || hammer.VelocityCalculator.Velocity.magnitude < SMASH_VELOCITY_THRESHOLD || !hammer.Draggable.IsHeld || timeSinceLastImpact < 0.1f)
			{
				return;
			}
			ContactPoint[] array = new ContactPoint[col.contactCount];
			col.GetContacts(array);
			Vector3 vector = Vector3.zero;
			for (int i = 0; i < array.Length; i++)
			{
				if (Vector3.Distance(array[i].point, hammer.ImpactPoint.position) < 0.1f)
				{
					vector = array[i].point;
					break;
				}
			}
			if (!(vector == Vector3.zero))
			{
				timeSinceLastImpact = 0f;
				impactCount++;
				Oven.CreateImpactEffects(vector);
				if (impactCount == 3)
				{
					Shatter();
				}
			}
		}

		private void Shatter()
		{
			int num = Oven.CurrentOperation.Cookable.ProductQuantity * Oven.CurrentOperation.IngredientQuantity;
			Oven.Shatter(num, Oven.CurrentOperation.Cookable.ProductShardPrefab.gameObject);
			Oven.OutputVisuals.BlockRefreshes = true;
			ItemInstance productItem = Oven.CurrentOperation.GetProductItem(num);
			Oven.OutputSlot.AddItem(productItem);
			NetworkSingleton<VariableDatabase>.Instance.SetVariableValue("Oven_Cooks_Completed", (NetworkSingleton<VariableDatabase>.Instance.GetValue<float>("Oven_Cooks_Completed") + 1f).ToString());
			Oven.SendCookOperation(null);
			Oven.StartCoroutine(Routine());
			IEnumerator Routine()
			{
				yield return new WaitForSeconds(1.4f);
				if (base.TaskActive)
				{
					Success();
				}
			}
		}
	}
}
