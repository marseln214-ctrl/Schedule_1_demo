using System.Collections;
using System.Collections.Generic;
using ScheduleOne.DevUtilities;
using ScheduleOne.ObjectScripts;
using ScheduleOne.Packaging;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Product;
using ScheduleOne.UI;
using ScheduleOne.UI.Stations;
using UnityEngine;

namespace ScheduleOne.PlayerTasks
{
	public class UseBrickPress : Task
	{
		public enum EStep
		{
			Pouring = 0,
			Pressing = 1,
			Complete = 2
		}

		public const float PRODUCT_SCALE = 0.75f;

		protected EStep currentStep;

		protected BrickPress press;

		protected ProductItemInstance product;

		protected List<FunctionalProduct> products = new List<FunctionalProduct>();

		protected Draggable container;

		public override string TaskName { get; protected set; } = "Use brick press";


		public UseBrickPress(BrickPress _press, ProductItemInstance _product)
		{
			if (_press == null)
			{
				Console.LogError("Press is null!");
				return;
			}
			if (_press.GetState() != 0)
			{
				Console.LogError("Press not ready to begin packaging!");
				return;
			}
			press = _press;
			product = _product;
			PlayerSingleton<PlayerCamera>.Instance.OverrideTransform(press.CameraPosition_Pouring.position, press.CameraPosition_Pouring.rotation, 0.2f);
			PlayerSingleton<PlayerCamera>.Instance.OverrideFOV(70f, 0.2f);
			PlayerSingleton<PlayerInventory>.Instance.SetInventoryEnabled(enabled: false);
			Singleton<InputPromptsCanvas>.Instance.LoadModule("packaging");
			press.Container1.gameObject.SetActive(value: false);
			container = press.CreateFunctionalContainer(product, 0.75f, out products);
			base.CurrentInstruction = "Pour product into mould (0/20)";
			press.StartCoroutine(CheckMould());
			IEnumerator CheckMould()
			{
				while (base.TaskActive)
				{
					this.CheckMould();
					yield return new WaitForSeconds(0.2f);
				}
			}
		}

		public override void Update()
		{
			base.Update();
			if (currentStep == EStep.Pressing && press.Handle.CurrentPosition >= 1f)
			{
				FinishPress();
			}
		}

		public override void StopTask()
		{
			base.StopTask();
			Singleton<InputPromptsCanvas>.Instance.UnloadModule();
			if (container != null)
			{
				Object.Destroy(container.gameObject);
			}
			for (int i = 0; i < products.Count; i++)
			{
				Object.Destroy(products[i].gameObject);
			}
			press.Container1.gameObject.SetActive(value: true);
			PlayerSingleton<PlayerCamera>.Instance.OverrideTransform(press.CameraPosition.position, press.CameraPosition.rotation, 0.2f);
			PlayerSingleton<PlayerCamera>.Instance.OverrideFOV(65f, 0.2f);
			Singleton<BrickPressCanvas>.Instance.SetIsOpen(press, open: true);
			press.Handle.Locked = false;
			press.Handle.SetInteractable(e: false);
			if (currentStep == EStep.Complete)
			{
				press.CompletePress(product);
			}
		}

		private void CheckMould()
		{
			if (currentStep == EStep.Pouring)
			{
				List<FunctionalProduct> productInMould = press.GetProductInMould();
				base.CurrentInstruction = "Pour product into mould (" + productInMould.Count + "/20)";
				if (productInMould.Count >= 20)
				{
					BeginPress();
				}
			}
		}

		private void BeginPress()
		{
			currentStep = EStep.Pressing;
			press.Handle.SetInteractable(e: true);
			container.ClickableEnabled = false;
			container.Rb.AddForce((press.transform.right + press.transform.up) * 2f, ForceMode.VelocityChange);
			base.CurrentInstruction = "Rotate handle quickly to press product";
			PlayerSingleton<PlayerCamera>.Instance.OverrideFOV(70f, 0.3f);
			PlayerSingleton<PlayerCamera>.Instance.OverrideTransform(press.CameraPosition_Raising.position, press.CameraPosition_Raising.rotation, 0.3f);
		}

		private void FinishPress()
		{
			press.SlamSound.Play();
			currentStep = EStep.Complete;
			press.Handle.Locked = true;
			press.Handle.SetInteractable(e: false);
			PlayerSingleton<PlayerCamera>.Instance.OverrideFOV(60f, 0.1f);
			PlayerSingleton<PlayerCamera>.Instance.StartCameraShake(0.25f, 0.2f);
			press.StartCoroutine(Wait());
			IEnumerator Wait()
			{
				yield return new WaitForSeconds(0.8f);
				StopTask();
			}
		}
	}
}
