using System;
using System.Collections;
using ScheduleOne.DevUtilities;
using ScheduleOne.PlayerScripts;
using UnityEngine;
using UnityEngine.UI;

namespace ScheduleOne.UI
{
	public class StaminaBar : MonoBehaviour
	{
		public const float StaminaShowTime = 1.5f;

		public const float StaminaFadeTime = 0.5f;

		[Header("References")]
		public Slider[] Sliders;

		public CanvasGroup Group;

		private Coroutine routine;

		private void Awake()
		{
			Player.onLocalPlayerSpawned = (Action)Delegate.Combine(Player.onLocalPlayerSpawned, new Action(PlayerSpawned));
			Group.alpha = 0f;
		}

		private void PlayerSpawned()
		{
			PlayerMovement instance = PlayerSingleton<PlayerMovement>.Instance;
			instance.onStaminaReserveChanged = (Action<float>)Delegate.Combine(instance.onStaminaReserveChanged, new Action<float>(UpdateStaminaBar));
		}

		private void UpdateStaminaBar(float change)
		{
			Slider[] sliders = Sliders;
			for (int i = 0; i < sliders.Length; i++)
			{
				sliders[i].value = PlayerSingleton<PlayerMovement>.Instance.CurrentStaminaReserve / PlayerMovement.StaminaReserveMax;
			}
			Group.alpha = 1f;
			if (routine != null)
			{
				StopCoroutine(routine);
			}
			routine = StartCoroutine(Routine());
			IEnumerator Routine()
			{
				yield return new WaitForSeconds(1.5f);
				for (float j = 0f; j < 0.5f; j += Time.deltaTime)
				{
					Group.alpha = Mathf.Lerp(1f, 0f, j / 0.5f);
					yield return new WaitForEndOfFrame();
				}
				Group.alpha = 0f;
				routine = null;
			}
		}
	}
}
