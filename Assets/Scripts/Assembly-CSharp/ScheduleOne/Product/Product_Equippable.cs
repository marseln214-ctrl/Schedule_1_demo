using System.Collections;
using System.Linq;
using ScheduleOne.DevUtilities;
using ScheduleOne.Equipping;
using ScheduleOne.ItemFramework;
using ScheduleOne.Packaging;
using ScheduleOne.PlayerScripts;
using ScheduleOne.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.Product
{
	public class Product_Equippable : Equippable_Viewmodel
	{
		[Header("References")]
		public FilledPackagingVisuals Visuals;

		public Transform ModelContainer;

		[Header("Settings")]
		public bool Consumable = true;

		public string ConsumeDescription = "Smoke";

		public float ConsumeTime = 1.5f;

		public float EffectsApplyDelay = 2f;

		public string ConsumeAnimationBool = string.Empty;

		public string ConsumeEquippableAssetPath = string.Empty;

		[Header("Events")]
		public UnityEvent onConsumeInputStart;

		public UnityEvent onConsumeInputComplete;

		public UnityEvent onConsumeInputCancel;

		private float consumeTime;

		private bool consumingInProgress;

		private Vector3 defaultModelPosition = Vector3.zero;

		private int productAmount = 1;

		private Coroutine consumeRoutine;

		public override void Equip(ItemInstance item)
		{
			base.Equip(item);
			ProductItemInstance productItemInstance = item as ProductItemInstance;
			productAmount = productItemInstance.Amount;
			if (Consumable && productAmount == 1)
			{
				Singleton<InputPromptsCanvas>.Instance.LoadModule("consumable");
				Singleton<InputPromptsCanvas>.Instance.currentModule.gameObject.GetComponentsInChildren<Transform>().FirstOrDefault((Transform c) => c.gameObject.name == "Label").GetComponent<TextMeshProUGUI>()
					.text = "(Hold) " + ConsumeDescription;
			}
			productItemInstance.SetupPackagingVisuals(Visuals);
			if (ModelContainer == null)
			{
				Console.LogWarning("Model container not set for equippable product: " + item.Name);
				ModelContainer = base.transform.GetChild(0);
			}
			defaultModelPosition = ModelContainer.localPosition;
		}

		public override void Unequip()
		{
			if (Consumable)
			{
				Singleton<InputPromptsCanvas>.Instance.UnloadModule();
			}
			if (base.transform.IsChildOf(Player.Local.transform))
			{
				Player.Local.SendAnimationBool(ConsumeAnimationBool, val: false);
			}
			if (consumingInProgress)
			{
				StopCoroutine(consumeRoutine);
			}
			base.Unequip();
		}

		protected override void Update()
		{
			base.Update();
			Vector3 b = defaultModelPosition;
			if (Consumable && !consumingInProgress && GameInput.GetButton(GameInput.ButtonCode.PrimaryClick) && productAmount == 1 && !Singleton<PauseMenu>.Instance.IsPaused)
			{
				if (consumeTime == 0f && onConsumeInputStart != null)
				{
					onConsumeInputStart.Invoke();
				}
				consumeTime += Time.deltaTime;
				Singleton<HUD>.Instance.ShowRadialIndicator(consumeTime / ConsumeTime);
				if (consumeTime >= ConsumeTime)
				{
					Consume();
					if (onConsumeInputComplete != null)
					{
						onConsumeInputComplete.Invoke();
					}
				}
			}
			else
			{
				if (consumeTime > 0f && onConsumeInputCancel != null && !consumingInProgress)
				{
					onConsumeInputCancel.Invoke();
					if (base.transform.IsChildOf(Player.Local.transform))
					{
						Player.Local.SendAnimationBool(ConsumeAnimationBool, val: false);
					}
				}
				consumeTime = 0f;
			}
			if (consumeTime > 0f || consumingInProgress)
			{
				b = defaultModelPosition - ModelContainer.transform.parent.InverseTransformDirection(PlayerSingleton<PlayerCamera>.Instance.transform.up) * 0.25f;
			}
			ModelContainer.transform.localPosition = Vector3.Lerp(ModelContainer.transform.localPosition, b, Time.deltaTime * 6f);
		}

		protected virtual void Consume()
		{
			consumingInProgress = true;
			if (base.transform.IsChildOf(Player.Local.transform))
			{
				Player.Local.SendAnimationBool(ConsumeAnimationBool, val: true);
				if (ConsumeEquippableAssetPath != string.Empty)
				{
					Player.Local.SendEquippable_Networked(ConsumeEquippableAssetPath);
				}
			}
			consumeRoutine = StartCoroutine(ConsumeRoutine());
			IEnumerator ConsumeRoutine()
			{
				yield return new WaitForSeconds(EffectsApplyDelay);
				consumingInProgress = false;
				ApplyEffects();
				itemInstance.ChangeQuantity(-1);
			}
		}

		protected virtual void ApplyEffects()
		{
			Player.Local.ConsumeProduct(itemInstance as ProductItemInstance);
		}
	}
}
