using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FishNet;
using ScheduleOne.Clothing;
using ScheduleOne.DevUtilities;
using ScheduleOne.GameTime;
using ScheduleOne.Networking;
using ScheduleOne.PlayerScripts;
using ScheduleOne.UI;
using ScheduleOne.UI.CharacterCreator;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.AvatarFramework.Customization
{
	public class CharacterCreator : Singleton<CharacterCreator>
	{
		public enum ECategory
		{
			Body = 0,
			Hair = 1,
			Face = 2,
			Eyes = 3,
			Eyebrows = 4,
			Clothing = 5,
			Accessories = 6
		}

		public List<BaseCharacterCreatorField> Fields = new List<BaseCharacterCreatorField>();

		[Header("References")]
		public Transform Container;

		public Transform CameraPosition;

		public Transform RigContainer;

		public Avatar Rig;

		public Canvas Canvas;

		public UnityEngine.Animation CanvasAnimation;

		[Header("Settings")]
		public bool DemoCreator;

		public BasicAvatarSettings DefaultSettings;

		public List<BasicAvatarSettings> Presets;

		public UnityEvent<BasicAvatarSettings> onComplete;

		public UnityEvent<BasicAvatarSettings, List<ClothingInstance>> onCompleteWithClothing;

		private Dictionary<string, ClothingDefinition> lastSelectedClothingDefinitions = new Dictionary<string, ClothingDefinition>();

		private float rigTargetY;

		public bool IsOpen { get; protected set; }

		public BasicAvatarSettings ActiveSettings { get; protected set; }

		protected override void Awake()
		{
			if (!DemoCreator)
			{
				base.gameObject.SetActive(value: false);
				return;
			}
			base.Awake();
			Fields = Canvas.GetComponentsInChildren<BaseCharacterCreatorField>(includeInactive: true).ToList();
		}

		protected override void Start()
		{
			base.Start();
			Container.gameObject.SetActive(value: false);
		}

		private void Update()
		{
			RigContainer.localEulerAngles = Vector3.Lerp(RigContainer.localEulerAngles, new Vector3(0f, rigTargetY, 0f), Time.deltaTime * 5f);
		}

		public void Open(BasicAvatarSettings initialSettings, bool showUI = true)
		{
			IsOpen = true;
			if (showUI)
			{
				ShowUI();
			}
			if (!DemoCreator)
			{
				PlayerSingleton<PlayerCamera>.Instance.OverrideFOV(60f, 0f);
				PlayerSingleton<PlayerCamera>.Instance.OverrideTransform(CameraPosition.position, CameraPosition.rotation, 0f);
			}
			PlayerSingleton<PlayerInventory>.Instance.SetInventoryEnabled(enabled: false);
			PlayerSingleton<PlayerMovement>.Instance.canMove = false;
			Singleton<HUD>.Instance.canvas.enabled = false;
			Container.gameObject.SetActive(value: true);
			if (InstanceFinder.IsServer && !Singleton<Lobby>.Instance.IsInLobby)
			{
				NetworkSingleton<TimeManager>.Instance.TimeProgressionMultiplier = 0f;
			}
			if (initialSettings != null)
			{
				ActiveSettings = Object.Instantiate(initialSettings);
			}
			else
			{
				ActiveSettings = ScriptableObject.CreateInstance<BasicAvatarSettings>();
			}
			Rig.LoadAvatarSettings(ActiveSettings.GetAvatarSettings());
			for (int i = 0; i < Fields.Count; i++)
			{
				Fields[i].ApplyValue();
			}
		}

		public void ShowUI()
		{
			Canvas.enabled = true;
			CanvasAnimation.Play("Character creator fade in");
			PlayerSingleton<PlayerCamera>.Instance.FreeMouse();
			PlayerSingleton<PlayerCamera>.Instance.AddActiveUIElement(base.name);
		}

		public void Close()
		{
			IsOpen = false;
			StartCoroutine(Close());
			IEnumerator Close()
			{
				PlayerSingleton<PlayerCamera>.Instance.LockMouse();
				PlayerSingleton<PlayerCamera>.Instance.RemoveActiveUIElement(base.name);
				rigTargetY = 0f;
				Canvas.enabled = false;
				if (InstanceFinder.IsServer)
				{
					NetworkSingleton<TimeManager>.Instance.TimeProgressionMultiplier = 1f;
				}
				yield break;
			}
		}

		public void DisableStuff()
		{
			Container.gameObject.SetActive(value: false);
		}

		public void Done()
		{
			if (IsOpen)
			{
				List<ClothingInstance> arg = new List<ClothingInstance>();
				if (onComplete != null)
				{
					onComplete.Invoke(ActiveSettings);
				}
				if (onCompleteWithClothing != null)
				{
					onCompleteWithClothing.Invoke(ActiveSettings, arg);
				}
				Close();
			}
		}

		public void SliderChanged(float newVal)
		{
			rigTargetY = newVal * 359f;
		}

		public T SetValue<T>(string fieldName, T value, ClothingDefinition definition)
		{
			if (!lastSelectedClothingDefinitions.ContainsKey(fieldName))
			{
				lastSelectedClothingDefinitions.Add(fieldName, definition);
			}
			else
			{
				lastSelectedClothingDefinitions[fieldName] = definition;
			}
			if (fieldName == "Preset")
			{
				SelectPreset(value as string);
				return default(T);
			}
			ActiveSettings.SetValue(fieldName, value);
			return value;
		}

		public void SelectPreset(string presetName)
		{
			BasicAvatarSettings basicAvatarSettings = Presets.Find((BasicAvatarSettings p) => p.name == presetName);
			if (basicAvatarSettings == null)
			{
				Debug.LogError("Preset not found: " + presetName);
				return;
			}
			ActiveSettings = Object.Instantiate(basicAvatarSettings);
			Rig.LoadAvatarSettings(ActiveSettings.GetAvatarSettings());
			for (int i = 0; i < Fields.Count; i++)
			{
				Fields[i].ApplyValue();
			}
		}

		public void RefreshCategory(ECategory category)
		{
			AvatarSettings avatarSettings = ActiveSettings.GetAvatarSettings();
			switch (category)
			{
			case ECategory.Body:
				Rig.ApplyBodySettings(avatarSettings);
				Rig.ApplyEyeLidColorSettings(avatarSettings);
				Rig.ApplyBodyLayerSettings(avatarSettings);
				break;
			case ECategory.Hair:
				Rig.ApplyHairSettings(avatarSettings);
				Rig.ApplyHairColorSettings(avatarSettings);
				Rig.ApplyFaceLayerSettings(avatarSettings);
				break;
			case ECategory.Face:
				Rig.ApplyFaceLayerSettings(avatarSettings);
				break;
			case ECategory.Eyes:
				Rig.ApplyEyeBallSettings(avatarSettings);
				Rig.ApplyEyeLidColorSettings(avatarSettings);
				Rig.ApplyEyeLidSettings(avatarSettings);
				break;
			case ECategory.Eyebrows:
				Rig.ApplyEyebrowSettings(avatarSettings);
				break;
			case ECategory.Clothing:
				Rig.ApplyBodyLayerSettings(avatarSettings);
				break;
			case ECategory.Accessories:
				Rig.ApplyAccessorySettings(avatarSettings);
				break;
			}
		}
	}
}
