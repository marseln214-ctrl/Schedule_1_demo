using System.Collections;
using System.Collections.Generic;
using ScheduleOne.Audio;
using ScheduleOne.DevUtilities;
using ScheduleOne.ItemFramework;
using ScheduleOne.PlayerTasks;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.ObjectScripts.Soil
{
	public class PourableSoil : Pourable
	{
		public const float TEAR_ANGLE = 10f;

		public const float HIGHLIGHT_CYCLE_TIME = 5f;

		public bool IsOpen;

		public SoilDefinition SoilDefinition;

		[Header("References")]
		public Transform SoilBag;

		public Transform[] Bones;

		public List<Collider> TopColliders;

		public MeshRenderer[] Highlights;

		public Transform TopParent;

		public AudioSourceController SnipSound;

		public SkinnedMeshRenderer TopMesh;

		public UnityEvent onOpened;

		private Vector3 highlightScale = Vector3.zero;

		private float timeSinceStart;

		public int currentCut { get; protected set; }

		protected override void Awake()
		{
			base.Awake();
			highlightScale = Highlights[0].transform.localScale;
			UpdateHighlights();
			ClickableEnabled = false;
		}

		protected override void Update()
		{
			base.Update();
			timeSinceStart += Time.deltaTime;
			UpdateHighlights();
		}

		private void UpdateHighlights()
		{
			if (Highlights[0] == null)
			{
				return;
			}
			for (int i = 0; i < Highlights.Length; i++)
			{
				if (IsOpen || i < currentCut)
				{
					Highlights[i].gameObject.SetActive(value: false);
					continue;
				}
				float num = (float)i / (float)Highlights.Length;
				float num2 = Mathf.Sin(Mathf.Clamp(timeSinceStart * 5f - num, 0f, float.MaxValue)) + 1f;
				Highlights[i].transform.localScale = new Vector3(highlightScale.x * num2, highlightScale.y, highlightScale.z * num2);
			}
		}

		protected override void PourAmount(float amount)
		{
			base.PourAmount(amount);
			SoilBag.localScale = new Vector3(1f, Mathf.Lerp(0.45f, 1f, currentQuantity / StartQuantity), 1f);
			if (IsPourPointOverPot())
			{
				if (TargetPot.SoilID != SoilDefinition.ID)
				{
					TargetPot.SetSoilID(SoilDefinition.ID);
					TargetPot.SetSoilUses(SoilDefinition.Uses);
				}
				TargetPot.SetSoilState(Pot.ESoilState.Flat);
				TargetPot.AddSoil(amount);
			}
			if (TargetPot.SoilLevel >= TargetPot.SoilCapacity)
			{
				Singleton<TaskManager>.Instance.currentTask.Success();
			}
		}

		protected override bool CanPour()
		{
			if (!IsOpen)
			{
				return false;
			}
			return base.CanPour();
		}

		public void Cut()
		{
			TopColliders[currentCut].enabled = false;
			LerpCut(currentCut);
			if (currentCut == Bones.Length - 1)
			{
				FinishCut();
			}
			SnipSound.AudioSource.pitch = 0.9f + (float)currentCut * 0.05f;
			SnipSound.PlayOneShot();
			currentCut++;
		}

		private void FinishCut()
		{
			IsOpen = true;
			Rigidbody rigidbody = TopParent.gameObject.AddComponent<Rigidbody>();
			TopParent.transform.SetParent(null);
			rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
			rigidbody.AddRelativeForce(Vector3.forward * 1.5f, ForceMode.VelocityChange);
			rigidbody.AddRelativeForce(Vector3.up * 0.3f, ForceMode.VelocityChange);
			rigidbody.AddTorque(Vector3.up * 1.5f, ForceMode.VelocityChange);
			ClickableEnabled = true;
			if (onOpened != null)
			{
				onOpened.Invoke();
			}
			Object.Destroy(TopParent.gameObject, 3f);
			Object.Destroy(TopMesh.gameObject, 3f);
		}

		private void LerpCut(int cutIndex)
		{
			Transform bone = Bones[cutIndex];
			Quaternion startRot = bone.localRotation;
			Quaternion endRot = bone.localRotation * Quaternion.Euler(0f, 0f, 10f);
			StartCoroutine(Routine());
			IEnumerator Routine()
			{
				float lerpTime = 0.1f;
				for (float i = 0f; i < lerpTime; i += Time.deltaTime)
				{
					bone.localRotation = Quaternion.Lerp(startRot, endRot, i / lerpTime);
					yield return new WaitForEndOfFrame();
				}
				bone.localRotation = endRot;
			}
		}
	}
}
