using System.Collections;
using ScheduleOne.Audio;
using UnityEngine;

namespace ScheduleOne.AvatarFramework.Equipping
{
	public class Taser : AvatarRangedWeapon
	{
		public const float TaseDuration = 2f;

		public const float TaseMoveSpeedMultiplier = 0.5f;

		[Header("References")]
		public GameObject FlashObject;

		public AudioSourceController ChargeSound;

		[Header("Prefabs")]
		public GameObject RayPrefab;

		private Coroutine flashRoutine;

		public override void Equip(Avatar _avatar)
		{
			base.Equip(_avatar);
			FlashObject.gameObject.SetActive(value: false);
		}

		public override void Shoot(Vector3 endPoint)
		{
			base.Shoot(endPoint);
			if (flashRoutine != null)
			{
				StopCoroutine(flashRoutine);
			}
			ChargeSound.Stop();
			flashRoutine = StartCoroutine(Flash(endPoint));
		}

		public override void SetIsRaised(bool raised)
		{
			base.SetIsRaised(raised);
			if (base.IsRaised)
			{
				ChargeSound.Play();
			}
			else
			{
				ChargeSound.Stop();
			}
		}

		private IEnumerator Flash(Vector3 endPoint)
		{
			float t = 0.2f;
			FlashObject.gameObject.SetActive(value: true);
			Transform obj = Object.Instantiate(RayPrefab, GameObject.Find("_Temp").transform).transform;
			Object.Destroy(obj.gameObject, t);
			obj.transform.position = (MuzzlePoint.position + endPoint) / 2f;
			obj.transform.LookAt(endPoint);
			obj.transform.localScale = new Vector3(1f, 1f, Vector3.Distance(MuzzlePoint.position, endPoint));
			yield return new WaitForSeconds(0.2f);
			FlashObject.gameObject.SetActive(value: false);
		}
	}
}
