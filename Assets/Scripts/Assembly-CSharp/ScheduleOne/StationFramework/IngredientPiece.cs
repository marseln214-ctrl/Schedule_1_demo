using System.Collections;
using ScheduleOne.PlayerTasks;
using UnityEngine;

namespace ScheduleOne.StationFramework
{
	[RequireComponent(typeof(Draggable))]
	public class IngredientPiece : MonoBehaviour
	{
		public const float LIQUID_FRICTION = 100f;

		public LiquidContainer CurrentLiquidContainer;

		[Header("References")]
		public Transform ModelContainer;

		public ParticleSystem DissolveParticles;

		[Header("Settings")]
		public bool DetectLiquid = true;

		public bool DisableInteractionInLiquid = true;

		[Range(0f, 2f)]
		public float LiquidFrictionMultiplier = 1f;

		private Draggable draggable;

		private float defaultDrag;

		private Coroutine dissolveParticleRoutine;

		public float CurrentDissolveAmount { get; private set; }

		private void Start()
		{
			InvokeRepeating("CheckLiquid", 0f, 0.05f);
			draggable = GetComponent<Draggable>();
			defaultDrag = draggable.NormalRBDrag;
		}

		private void Update()
		{
			if (DisableInteractionInLiquid && CurrentLiquidContainer != null)
			{
				draggable.ClickableEnabled = false;
			}
		}

		private void FixedUpdate()
		{
			UpdateDrag();
		}

		private void UpdateDrag()
		{
			if (CurrentLiquidContainer != null)
			{
				Vector3 vector = -draggable.Rb.velocity.normalized;
				float num = CurrentLiquidContainer.Viscosity * draggable.Rb.velocity.magnitude * 100f * LiquidFrictionMultiplier;
				draggable.Rb.AddForce(vector * num, ForceMode.Acceleration);
			}
		}

		private void CheckLiquid()
		{
			CurrentLiquidContainer = null;
			if (!DetectLiquid)
			{
				return;
			}
			Collider[] array = Physics.OverlapSphere(base.transform.position, 0.001f, 1 << LayerMask.NameToLayer("Task"), QueryTriggerInteraction.Collide);
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i].isTrigger && array[i].TryGetComponent<LiquidVolumeCollider>(out var component))
				{
					CurrentLiquidContainer = component.LiquidContainer;
					break;
				}
			}
		}

		public void DissolveAmount(float amount, bool showParticles = true)
		{
			if (CurrentDissolveAmount >= 1f)
			{
				return;
			}
			CurrentDissolveAmount = Mathf.Clamp01(CurrentDissolveAmount + amount);
			ModelContainer.transform.localScale = Vector3.one * (1f - CurrentDissolveAmount);
			if (showParticles)
			{
				if (!DissolveParticles.isPlaying)
				{
					DissolveParticles.Play();
				}
				if (dissolveParticleRoutine != null)
				{
					StopCoroutine(dissolveParticleRoutine);
				}
				dissolveParticleRoutine = StartCoroutine(DissolveParticlesRoutine());
			}
			IEnumerator DissolveParticlesRoutine()
			{
				yield return new WaitForSeconds(0.2f);
				DissolveParticles.Stop();
				dissolveParticleRoutine = null;
			}
		}
	}
}
