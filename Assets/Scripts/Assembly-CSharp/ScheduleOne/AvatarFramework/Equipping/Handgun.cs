using System.Collections;
using ScheduleOne.PlayerScripts;
using UnityEngine;

namespace ScheduleOne.AvatarFramework.Equipping
{
	public class Handgun : AvatarRangedWeapon
	{
		[Header("References")]
		public UnityEngine.Animation Anim;

		public ParticleSystem ShellParticles;

		public ParticleSystem SmokeParticles;

		public Transform FlashObject;

		[Header("Prefabs")]
		public GameObject RayPrefab;

		private Coroutine flashRoutine;

		public override void Shoot(Vector3 endPoint)
		{
			base.Shoot(endPoint);
			Anim.Play();
			ShellParticles.Play();
			SmokeParticles.Play();
			Player componentInParent = GetComponentInParent<Player>();
			if (!(componentInParent != null) || !componentInParent.IsOwner)
			{
				if (flashRoutine != null)
				{
					StopCoroutine(flashRoutine);
				}
				flashRoutine = StartCoroutine(Flash(endPoint));
			}
		}

		private IEnumerator Flash(Vector3 endPoint)
		{
			float num = 0.06f;
			FlashObject.localEulerAngles = new Vector3(0f, 0f, Random.Range(0f, 360f));
			FlashObject.gameObject.SetActive(value: true);
			Transform obj = Object.Instantiate(RayPrefab, GameObject.Find("_Temp").transform).transform;
			Object.Destroy(obj.gameObject, num);
			obj.transform.position = (MuzzlePoint.position + endPoint) / 2f;
			obj.transform.LookAt(endPoint);
			obj.transform.localScale = new Vector3(1f, 1f, Vector3.Distance(MuzzlePoint.position, endPoint));
			yield return new WaitForSeconds(num);
			FlashObject.gameObject.SetActive(value: false);
		}
	}
}
