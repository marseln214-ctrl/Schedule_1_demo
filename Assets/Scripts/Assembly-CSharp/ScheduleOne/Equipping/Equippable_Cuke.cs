using System.Collections;
using ScheduleOne.Audio;
using ScheduleOne.DevUtilities;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Trash;
using UnityEngine;

namespace ScheduleOne.Equipping
{
	public class Equippable_Cuke : Equippable_Viewmodel
	{
		[Header("Settings")]
		public float BaseEnergyGain = 100f;

		public float MinEnergyGain = 2.5f;

		public float ConsecutiveReduction = 0.5f;

		public float HealthGain;

		public float AnimationDuration = 2f;

		public bool ClearDrugEffects;

		[Header("References")]
		public Animation OpenAnim;

		public Animation DrinkAnim;

		public AudioSourceController OpenSound;

		public AudioSourceController SlurpSound;

		public TrashItem TrashPrefab;

		public bool IsDrinking { get; protected set; }

		protected override void Update()
		{
			base.Update();
			if (!IsDrinking && GameInput.GetButtonDown(GameInput.ButtonCode.PrimaryClick) && !GameInput.IsTyping && PlayerSingleton<PlayerCamera>.Instance.activeUIElementCount == 0)
			{
				Drink();
			}
		}

		public void Drink()
		{
			IsDrinking = true;
			StartCoroutine(DrinkRoutine());
			IEnumerator DrinkRoutine()
			{
				OpenAnim.Play();
				DrinkAnim.Play();
				OpenSound.Play();
				SlurpSound.Play();
				yield return new WaitForSeconds(AnimationDuration);
				ApplyEffects();
				TrashPrefab = NetworkSingleton<TrashManager>.Instance.CreateTrashItem(TrashPrefab.ID, PlayerSingleton<PlayerCamera>.Instance.transform.position - PlayerSingleton<PlayerCamera>.Instance.transform.up * 0.3f, PlayerSingleton<PlayerCamera>.Instance.transform.rotation, PlayerSingleton<PlayerMovement>.Instance.Controller.velocity + (PlayerSingleton<PlayerCamera>.Instance.transform.forward + PlayerSingleton<PlayerCamera>.Instance.transform.up * 0.25f) * 4f);
				itemInstance.ChangeQuantity(-1);
				if (itemInstance.Quantity > 0)
				{
					PlayerSingleton<PlayerInventory>.Instance.Reequip();
				}
			}
		}

		public void ApplyEffects()
		{
			float num = Mathf.Pow(ConsecutiveReduction, Player.Local.Energy.EnergyDrinksConsumed);
			float num2 = Mathf.Clamp(BaseEnergyGain * num, MinEnergyGain, BaseEnergyGain);
			Player.Local.Energy.SetEnergy(Player.Local.Energy.CurrentEnergy + num2);
			PlayerSingleton<PlayerMovement>.Instance.SetStamina(PlayerMovement.StaminaReserveMax);
			if (HealthGain > 0f)
			{
				Player.Local.Health.RecoverHealth(HealthGain);
			}
			Player.Local.Energy.IncrementEnergyDrinks();
			if (ClearDrugEffects && Player.Local.ConsumedProduct != null)
			{
				Player.Local.ClearProduct();
			}
		}
	}
}
