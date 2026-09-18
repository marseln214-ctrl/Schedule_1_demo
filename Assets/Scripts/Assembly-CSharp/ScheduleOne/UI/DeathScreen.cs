using System.Collections;
using ScheduleOne.Audio;
using ScheduleOne.DevUtilities;
using ScheduleOne.FX;
using ScheduleOne.Law;
using ScheduleOne.Map;
using ScheduleOne.Persistence;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Property;
using UnityEngine;
using UnityEngine.UI;

namespace ScheduleOne.UI
{
	public class DeathScreen : Singleton<DeathScreen>
	{
		[Header("References")]
		public Canvas canvas;

		public RectTransform Container;

		public CanvasGroup group;

		public Button respawnButton;

		public Button loadSaveButton;

		public Animation Anim;

		public AudioSourceController Sound;

		private bool arrested;

		public bool isOpen { get; protected set; }

		protected override void Awake()
		{
			base.Awake();
			respawnButton.onClick.AddListener(RespawnClicked);
			loadSaveButton.onClick.AddListener(LoadSaveClicked);
			canvas.enabled = false;
			Container.gameObject.SetActive(value: false);
			group.alpha = 0f;
			group.interactable = false;
		}

		private void RespawnClicked()
		{
			if (isOpen)
			{
				isOpen = false;
				StartCoroutine(Routine());
			}
			IEnumerator Routine()
			{
				Singleton<BlackOverlay>.Instance.Open();
				yield return new WaitForSeconds(0.5f);
				Close();
				Singleton<HospitalBillScreen>.Instance.Open();
				_ = Singleton<ScheduleOne.Map.Map>.Instance.MedicalCentre.RespawnPoint;
				Transform transform = NetworkSingleton<GameManager>.Instance.NoHomeRespawnPoint;
				if (NetworkSingleton<CurfewManager>.Instance.IsCurrentlyActive && (Player.Local.LastVisitedProperty != null || ScheduleOne.Property.Property.OwnedProperties.Count > 0))
				{
					transform = ((!(Player.Local.LastVisitedProperty != null)) ? ScheduleOne.Property.Property.OwnedProperties[0].InteriorSpawnPoint : Player.Local.LastVisitedProperty.InteriorSpawnPoint);
				}
				Player.Local.Health.SendRevive(transform.position + Vector3.up * 1f, transform.rotation);
				if (arrested)
				{
					Singleton<ArrestNoticeScreen>.Instance.RecordCrimes();
					Player.Local.Free();
				}
				yield return new WaitForSeconds(2f);
				Singleton<BlackOverlay>.Instance.Close();
			}
		}

		private void LoadSaveClicked()
		{
			Close();
			Singleton<LoadManager>.Instance.ExitToMenu(Singleton<LoadManager>.Instance.ActiveSaveInfo);
		}

		public void Open()
		{
			if (!isOpen)
			{
				isOpen = true;
				arrested = Player.Local.IsArrested;
				PlayerSingleton<PlayerCamera>.Instance.AddActiveUIElement(base.name);
				Sound.Play();
				respawnButton.gameObject.SetActive(CanRespawn());
				loadSaveButton.gameObject.SetActive(!respawnButton.gameObject.activeSelf);
				StartCoroutine(Routine());
			}
			IEnumerator Routine()
			{
				yield return new WaitForSeconds(0.55f);
				Anim.Play();
				canvas.enabled = true;
				Container.gameObject.SetActive(value: true);
				float lerpTime = 0.75f;
				for (float i = 0f; i < lerpTime; i += Time.deltaTime)
				{
					Singleton<PostProcessingManager>.Instance.SetBlur(i / lerpTime);
					yield return new WaitForEndOfFrame();
				}
				Singleton<PostProcessingManager>.Instance.SetBlur(1f);
				PlayerSingleton<PlayerCamera>.Instance.FreeMouse();
				group.interactable = true;
			}
		}

		private bool CanRespawn()
		{
			return Player.PlayerList.Count > 1;
		}

		public void Close()
		{
			isOpen = false;
			PlayerSingleton<PlayerCamera>.Instance.RemoveActiveUIElement(base.name);
			Singleton<PostProcessingManager>.Instance.SetBlur(0f);
			canvas.enabled = false;
			Container.gameObject.SetActive(value: false);
		}
	}
}
