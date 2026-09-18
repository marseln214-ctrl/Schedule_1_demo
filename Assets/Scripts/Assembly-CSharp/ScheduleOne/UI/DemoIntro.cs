using System.Collections;
using EasyButtons;
using FishNet;
using ScheduleOne.Audio;
using ScheduleOne.AvatarFramework.Customization;
using ScheduleOne.DevUtilities;
using ScheduleOne.Persistence;
using ScheduleOne.PlayerScripts;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ScheduleOne.UI
{
	public class DemoIntro : Singleton<DemoIntro>
	{
		public const float SKIP_TIME = 0.5f;

		public Animation Anim;

		public Transform PlayerInitialPosition;

		public GameObject SkipContainer;

		public Image SkipDial;

		public int SkipEvents = 3;

		public UnityEvent onStart;

		public UnityEvent onStartAsServer;

		public UnityEvent onCutsceneDone;

		public UnityEvent onIntroDone;

		public UnityEvent onIntroDoneAsServer;

		private int CurrentStep;

		public string MusicName;

		private float currentSkipTime;

		private bool depressed = true;

		private bool waitingForCutsceneEnd;

		public bool IsPlaying { get; protected set; }

		private void Update()
		{
			if (waitingForCutsceneEnd && !Anim.isPlaying)
			{
				CutsceneDone();
			}
			if (!Anim.isPlaying)
			{
				return;
			}
			if ((GameInput.GetButton(GameInput.ButtonCode.Jump) || GameInput.GetButton(GameInput.ButtonCode.Submit) || GameInput.GetButton(GameInput.ButtonCode.PrimaryClick)) && depressed && CurrentStep < SkipEvents - 1)
			{
				currentSkipTime += Time.deltaTime;
				if (currentSkipTime >= 0.5f)
				{
					currentSkipTime = 0f;
					if (IsPlaying)
					{
						Debug.Log("Skipping!");
						int num = CurrentStep + 1;
						float time = Anim.clip.events[num].time;
						Anim[Anim.clip.name].time = time;
						CurrentStep = num;
						depressed = false;
					}
				}
				SkipDial.fillAmount = currentSkipTime / 0.5f;
				SkipContainer.SetActive(value: true);
			}
			else
			{
				currentSkipTime = 0f;
				SkipContainer.SetActive(value: false);
				if (!GameInput.GetButton(GameInput.ButtonCode.Jump) && !GameInput.GetButton(GameInput.ButtonCode.Submit) && !GameInput.GetButton(GameInput.ButtonCode.PrimaryClick))
				{
					depressed = true;
				}
			}
		}

		[Button]
		public void Play()
		{
			IsPlaying = true;
			PlayerSingleton<PlayerCamera>.Instance.SetCanLook(c: false);
			PlayerSingleton<PlayerCamera>.Instance.AddActiveUIElement(base.name);
			PlayerSingleton<PlayerMovement>.Instance.canMove = false;
			PlayerSingleton<PlayerInventory>.Instance.SetInventoryEnabled(enabled: false);
			Singleton<HUD>.Instance.canvas.enabled = false;
			Anim.Play();
			Invoke("PlayMusic", 1f);
			if (onStart != null)
			{
				onStart.Invoke();
			}
			waitingForCutsceneEnd = true;
			if (InstanceFinder.IsServer && onStartAsServer != null)
			{
				onStartAsServer.Invoke();
			}
		}

		private void PlayMusic()
		{
			Singleton<MusicPlayer>.Instance.Tracks.Find((MusicTrack t) => t.TrackName == MusicName).GetComponent<AmbientTrack>().ForcePlay();
		}

		public void ShowAvatar()
		{
			Singleton<ScheduleOne.AvatarFramework.Customization.CharacterCreator>.Instance.Open(Singleton<ScheduleOne.AvatarFramework.Customization.CharacterCreator>.Instance.DefaultSettings, showUI: false);
		}

		public void CutsceneDone()
		{
			waitingForCutsceneEnd = false;
			Singleton<ScheduleOne.AvatarFramework.Customization.CharacterCreator>.Instance.ShowUI();
			Singleton<ScheduleOne.AvatarFramework.Customization.CharacterCreator>.Instance.onComplete.AddListener(CharacterCreationDone);
			if (onCutsceneDone != null)
			{
				onCutsceneDone.Invoke();
			}
			IsPlaying = false;
		}

		public void PassedStep(int stepIndex)
		{
			CurrentStep = stepIndex;
		}

		public void CharacterCreationDone(BasicAvatarSettings avatar)
		{
			StartCoroutine(Wait());
			IEnumerator Wait()
			{
				Singleton<BlackOverlay>.Instance.Open();
				Singleton<MusicPlayer>.Instance.Tracks.Find((MusicTrack t) => t.TrackName == MusicName).GetComponent<AmbientTrack>().Stop();
				yield return new WaitForSeconds(0.5f);
				Player.Local.transform.position = PlayerInitialPosition.position;
				Player.Local.transform.rotation = PlayerInitialPosition.rotation;
				PlayerSingleton<PlayerCamera>.Instance.StopTransformOverride(0f, reenableCameraLook: false);
				PlayerSingleton<PlayerCamera>.Instance.StopFOVOverride(0f);
				Singleton<ScheduleOne.AvatarFramework.Customization.CharacterCreator>.Instance.DisableStuff();
				yield return new WaitForSeconds(0.5f);
				PlayerSingleton<PlayerCamera>.Instance.RemoveActiveUIElement(base.name);
				PlayerSingleton<PlayerCamera>.Instance.SetCanLook(c: true);
				PlayerSingleton<PlayerMovement>.Instance.canMove = true;
				PlayerSingleton<PlayerInventory>.Instance.SetInventoryEnabled(enabled: true);
				Singleton<HUD>.Instance.canvas.enabled = true;
				Singleton<BlackOverlay>.Instance.Close(1f);
				if (onIntroDone != null)
				{
					onIntroDone.Invoke();
				}
				if (InstanceFinder.IsServer)
				{
					if (onIntroDoneAsServer != null)
					{
						onIntroDoneAsServer.Invoke();
					}
					Singleton<SaveManager>.Instance.Save();
				}
				else
				{
					Player.Local.RequestSavePlayer();
				}
				base.gameObject.SetActive(value: false);
			}
		}
	}
}
