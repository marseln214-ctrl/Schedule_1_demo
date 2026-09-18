using System.Collections;
using System.Collections.Generic;
using EasyButtons;
using FishNet;
using ScheduleOne.AvatarFramework.Customization;
using ScheduleOne.Clothing;
using ScheduleOne.DevUtilities;
using ScheduleOne.GameTime;
using ScheduleOne.Persistence;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Property;
using ScheduleOne.UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ScheduleOne.Intro
{
	public class IntroManager : Singleton<IntroManager>
	{
		public const float SKIP_TIME = 0.5f;

		public int CurrentStep;

		[Header("Settings")]
		public int TimeOfDayOverride = 2000;

		[Header("References")]
		public GameObject Container;

		public Transform PlayerInitialPosition;

		public Transform CameraContainer;

		public Animation Anim;

		public GameObject SkipContainer;

		public Image SkipDial;

		public GameObject[] DisableDuringIntro;

		public RV rv;

		public UnityEvent onIntroDone;

		public UnityEvent onIntroDoneAsServer;

		private float currentSkipTime;

		private bool depressed = true;

		public bool IsPlaying { get; protected set; }

		protected override void Awake()
		{
			base.Awake();
			Container.gameObject.SetActive(value: false);
		}

		private void Update()
		{
			if (!Anim.isPlaying)
			{
				return;
			}
			if ((GameInput.GetButton(GameInput.ButtonCode.Jump) || GameInput.GetButton(GameInput.ButtonCode.Submit) || GameInput.GetButton(GameInput.ButtonCode.PrimaryClick)) && depressed)
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
			NetworkSingleton<TimeManager>.Instance.SetTimeOverridden(overridden: true, TimeOfDayOverride);
			Console.Log("Starting Intro...");
			Container.SetActive(value: true);
			rv.ModelContainer.gameObject.SetActive(value: false);
			PlayerSingleton<PlayerMovement>.Instance.canMove = false;
			Singleton<HUD>.Instance.canvas.enabled = false;
			PlayerSingleton<PlayerCamera>.Instance.LockMouse();
			PlayerSingleton<PlayerCamera>.Instance.OverrideFOV(60f, 0f);
			PlayerSingleton<PlayerCamera>.Instance.OverrideTransform(CameraContainer.position, CameraContainer.rotation, 0f);
			PlayerSingleton<PlayerCamera>.Instance.CameraContainer.transform.SetParent(CameraContainer);
			PlayerSingleton<PlayerInventory>.Instance.SetInventoryEnabled(enabled: false);
			PlayerSingleton<PlayerCamera>.Instance.AddActiveUIElement(base.name);
			GameObject[] disableDuringIntro = DisableDuringIntro;
			for (int i = 0; i < disableDuringIntro.Length; i++)
			{
				disableDuringIntro[i].gameObject.SetActive(value: false);
			}
			StartCoroutine(Wait());
			IEnumerator Wait()
			{
				yield return new WaitUntil(() => Singleton<LoadManager>.Instance.IsGameLoaded);
				Anim.Play();
				yield return new WaitForSeconds(0.1f);
				yield return new WaitUntil(() => !Anim.isPlaying);
				PlayerSingleton<PlayerCamera>.Instance.StopFOVOverride(0f);
				PlayerSingleton<PlayerCamera>.Instance.StopTransformOverride(0f, reenableCameraLook: false, returnToOriginalRotation: false);
				yield return new WaitForSeconds(0.1f);
				Singleton<CharacterCreator>.Instance.Open(Singleton<CharacterCreator>.Instance.DefaultSettings);
				Singleton<CharacterCreator>.Instance.onCompleteWithClothing.AddListener(CharacterCreationDone);
				yield return new WaitForSeconds(0.05f);
				PlayerSingleton<PlayerCamera>.Instance.RemoveActiveUIElement(base.name);
				Container.gameObject.SetActive(value: false);
				rv.ModelContainer.gameObject.SetActive(value: true);
				PlayerSingleton<PlayerMovement>.Instance.Teleport(NetworkSingleton<GameManager>.Instance.SpawnPoint.position);
				base.transform.forward = NetworkSingleton<GameManager>.Instance.SpawnPoint.forward;
				GameObject[] disableDuringIntro2 = DisableDuringIntro;
				for (int j = 0; j < disableDuringIntro2.Length; j++)
				{
					disableDuringIntro2[j].gameObject.SetActive(value: true);
				}
			}
		}

		public void CharacterCreationDone(BasicAvatarSettings avatar, List<ClothingInstance> clothes)
		{
			StartCoroutine(Wait());
			IEnumerator Wait()
			{
				Singleton<BlackOverlay>.Instance.Open();
				yield return new WaitForSeconds(0.5f);
				Player.Local.transform.position = PlayerInitialPosition.position;
				Player.Local.transform.rotation = PlayerInitialPosition.rotation;
				PlayerSingleton<PlayerCamera>.Instance.StopTransformOverride(0f, reenableCameraLook: false);
				PlayerSingleton<PlayerCamera>.Instance.StopFOVOverride(0f);
				Singleton<CharacterCreator>.Instance.DisableStuff();
				yield return new WaitForSeconds(0.5f);
				PlayerSingleton<PlayerCamera>.Instance.RemoveActiveUIElement(base.name);
				PlayerSingleton<PlayerCamera>.Instance.SetCanLook(c: true);
				PlayerSingleton<PlayerMovement>.Instance.canMove = true;
				PlayerSingleton<PlayerInventory>.Instance.SetInventoryEnabled(enabled: true);
				Singleton<HUD>.Instance.canvas.enabled = true;
				Singleton<BlackOverlay>.Instance.Close(1f);
				foreach (ClothingInstance clothe in clothes)
				{
					Player.Local.Clothing.InsertClothing(clothe);
				}
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

		public void PassedStep(int stepIndex)
		{
			CurrentStep = stepIndex;
		}
	}
}
