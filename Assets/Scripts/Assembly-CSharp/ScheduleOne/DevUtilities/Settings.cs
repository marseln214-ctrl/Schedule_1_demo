using System;
using ScheduleOne.Audio;
using ScheduleOne.Networking;
using ScheduleOne.PlayerScripts;
using ScheduleOne.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

namespace ScheduleOne.DevUtilities
{
	public class Settings : PersistentSingleton<Settings>
	{
		public enum UnitType
		{
			Metric = 0,
			Imperial = 1
		}

		public const float MinYPos = -20f;

		public DisplaySettings DisplaySettings;

		public DisplaySettings UnappliedDisplaySettings;

		public GraphicsSettings GraphicsSettings = new GraphicsSettings();

		public AudioSettings AudioSettings = new AudioSettings();

		public InputSettings InputSettings = new InputSettings();

		public InputActionAsset InputActions;

		public GameInput GameInput;

		public ScriptableRendererFeature SSAO;

		public ScriptableRendererFeature GodRays;

		[Header("Camera")]
		public float LookSensitivity = 1f;

		public bool InvertMouse;

		public float CameraFOV = 75f;

		public InputSettings.EActionMode SprintMode = InputSettings.EActionMode.Hold;

		[Range(0f, 1f)]
		public float CameraBobIntensity = 1f;

		private InputActionMap playerControls;

		public Action onDisplayChanged;

		public Action onInputsApplied;

		public UnitType unitType { get; protected set; }

		public bool PausingFreezesTime
		{
			get
			{
				if (Player.PlayerList.Count <= 1)
				{
					return !Singleton<Lobby>.Instance.IsInLobby;
				}
				return false;
			}
		}

		protected override void Awake()
		{
			base.Awake();
			playerControls = InputActions.FindActionMap("Generic");
			DisplaySettings = ReadDisplaySettings();
			UnappliedDisplaySettings = ReadDisplaySettings();
			GraphicsSettings = ReadGraphicsSettings();
			AudioSettings = ReadAudioSettings();
			InputSettings = ReadInputSettings();
			string[] commandLineArgs = Environment.GetCommandLineArgs();
			for (int i = 0; i < commandLineArgs.Length; i++)
			{
				if (commandLineArgs[i] == "-beta")
				{
					GameManager.IS_BETA = true;
				}
			}
		}

		protected override void Start()
		{
			base.Start();
			ApplyDisplaySettings(DisplaySettings);
			ApplyGraphicsSettings(GraphicsSettings);
			ApplyAudioSettings(AudioSettings);
			ApplyInputSettings(InputSettings);
		}

		public void ApplyDisplaySettings(DisplaySettings settings)
		{
			Resolution[] array = DisplaySettings.GetResolutions().ToArray();
			Resolution resolution = array[Mathf.Clamp(settings.ResolutionIndex, 0, array.Length - 1)];
			FullScreenMode fullScreenMode = FullScreenMode.Windowed;
			switch (settings.DisplayMode)
			{
			case DisplaySettings.EDisplayMode.Windowed:
				fullScreenMode = FullScreenMode.Windowed;
				break;
			case DisplaySettings.EDisplayMode.FullscreenWindow:
				fullScreenMode = FullScreenMode.FullScreenWindow;
				break;
			case DisplaySettings.EDisplayMode.ExclusiveFullscreen:
				fullScreenMode = FullScreenMode.ExclusiveFullScreen;
				break;
			}
			Screen.fullScreenMode = fullScreenMode;
			Screen.SetResolution(resolution.width, resolution.height, settings.DisplayMode == DisplaySettings.EDisplayMode.ExclusiveFullscreen || settings.DisplayMode == DisplaySettings.EDisplayMode.FullscreenWindow);
			QualitySettings.vSyncCount = (settings.VSync ? 1 : 0);
			Application.targetFrameRate = settings.TargetFPS;
			CanvasScaler.SetScaleFactor(settings.UIScale);
			Singleton<Settings>.Instance.CameraBobIntensity = settings.CameraBobbing;
		}

		public void ReloadGraphicsSettings()
		{
			ApplyGraphicsSettings(GraphicsSettings);
		}

		public void ApplyGraphicsSettings(GraphicsSettings settings)
		{
			QualitySettings.SetQualityLevel((int)settings.GraphicsQuality);
			PlayerCamera.SetAntialiasingMode(settings.AntiAliasingMode);
			CameraFOV = settings.FOV;
			//SSAO.SetActive(settings.SSAO);
			GodRays.SetActive(settings.GodRays);
		}

		public void ReloadAudioSettings()
		{
			ApplyAudioSettings(AudioSettings);
		}

		public void ApplyAudioSettings(AudioSettings settings)
		{
			Singleton<AudioManager>.Instance.SetMasterVolume(settings.MasterVolume);
			Singleton<AudioManager>.Instance.SetVolume(EAudioType.Ambient, settings.AmbientVolume);
			Singleton<AudioManager>.Instance.SetVolume(EAudioType.Music, settings.MusicVolume);
			Singleton<AudioManager>.Instance.SetVolume(EAudioType.FX, settings.SFXVolume);
			Singleton<AudioManager>.Instance.SetVolume(EAudioType.UI, settings.UIVolume);
			Singleton<AudioManager>.Instance.SetVolume(EAudioType.Voice, settings.DialogueVolume);
			Singleton<AudioManager>.Instance.SetVolume(EAudioType.Footsteps, settings.FootstepsVolume);
		}

		public void ReloadInputSettings()
		{
			ApplyInputSettings(InputSettings);
		}

		public void ApplyInputSettings(InputSettings settings)
		{
			InputSettings = settings;
			LookSensitivity = settings.MouseSensitivity;
			InvertMouse = settings.InvertMouse;
			SprintMode = settings.SprintMode;
			InputActions.Disable();
			InputActions.LoadBindingOverridesFromJson(settings.BindingOverrides);
			InputActions.Enable();
			GameInput.PlayerInput.actions = InputActions;
			onInputsApplied?.Invoke();
		}

		public void WriteDisplaySettings(DisplaySettings settings)
		{
			DisplaySettings = settings;
			UnappliedDisplaySettings = settings;
			PlayerPrefs.SetInt("ResolutionIndex", settings.ResolutionIndex);
			PlayerPrefs.SetInt("DisplayMode", (int)settings.DisplayMode);
			PlayerPrefs.SetInt("VSync", settings.VSync ? 1 : 0);
			PlayerPrefs.SetInt("TargetFPS", settings.TargetFPS);
			PlayerPrefs.SetFloat("UIScale", settings.UIScale);
			PlayerPrefs.SetFloat("CameraBobbing", settings.CameraBobbing);
		}

		public DisplaySettings ReadDisplaySettings()
		{
			DisplaySettings result = default(DisplaySettings);
			result.ResolutionIndex = PlayerPrefs.GetInt("ResolutionIndex", Screen.resolutions.Length - 1);
			result.DisplayMode = (DisplaySettings.EDisplayMode)PlayerPrefs.GetInt("DisplayMode", 2);
			result.VSync = PlayerPrefs.GetInt("VSync", 1) == 1;
			result.TargetFPS = PlayerPrefs.GetInt("TargetFPS", 90);
			result.UIScale = PlayerPrefs.GetFloat("UIScale", 1f);
			result.CameraBobbing = PlayerPrefs.GetFloat("CameraBobbing", 0.7f);
			return result;
		}

		public void WriteGraphicsSettings(GraphicsSettings settings)
		{
			GraphicsSettings = settings;
			PlayerPrefs.SetInt("QualityLevel", (int)settings.GraphicsQuality);
			PlayerPrefs.SetInt("AntiAliasing", (int)settings.AntiAliasingMode);
			PlayerPrefs.SetFloat("FOV", settings.FOV);
			PlayerPrefs.SetInt("SSAO", settings.SSAO ? 1 : 0);
			PlayerPrefs.SetInt("GodRays", settings.GodRays ? 1 : 0);
		}

		public GraphicsSettings ReadGraphicsSettings()
		{
			return new GraphicsSettings
			{
				GraphicsQuality = (GraphicsSettings.EGraphicsQuality)PlayerPrefs.GetInt("QualityLevel", 2),
				AntiAliasingMode = (GraphicsSettings.EAntiAliasingMode)PlayerPrefs.GetInt("AntiAliasing", 2),
				FOV = PlayerPrefs.GetFloat("FOV", 80f),
				SSAO = (PlayerPrefs.GetInt("SSAO", 1) == 1),
				GodRays = (PlayerPrefs.GetInt("GodRays", 1) == 1)
			};
		}

		public void WriteAudioSettings(AudioSettings settings)
		{
			AudioSettings = settings;
			PlayerPrefs.SetFloat("MasterVolume", settings.MasterVolume);
			PlayerPrefs.SetFloat("AmbientVolume", settings.AmbientVolume);
			PlayerPrefs.SetFloat("MusicVolume", settings.MusicVolume);
			PlayerPrefs.SetFloat("SFXVolume", settings.SFXVolume);
			PlayerPrefs.SetFloat("UIVolume", settings.UIVolume);
			PlayerPrefs.SetFloat("DialogueVolume", settings.DialogueVolume);
			PlayerPrefs.SetFloat("FootstepsVolume", settings.FootstepsVolume);
		}

		public AudioSettings ReadAudioSettings()
		{
			return new AudioSettings
			{
				MasterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f),
				AmbientVolume = PlayerPrefs.GetFloat("AmbientVolume", 1f),
				MusicVolume = PlayerPrefs.GetFloat("MusicVolume", 1f),
				SFXVolume = PlayerPrefs.GetFloat("SFXVolume", 1f),
				UIVolume = PlayerPrefs.GetFloat("UIVolume", 1f),
				DialogueVolume = PlayerPrefs.GetFloat("DialogueVolume", 1f),
				FootstepsVolume = PlayerPrefs.GetFloat("FootstepsVolume", 1f)
			};
		}

		public void WriteInputSettings(InputSettings settings)
		{
			InputSettings = settings;
			PlayerPrefs.SetFloat("MouseSensitivity", settings.MouseSensitivity);
			PlayerPrefs.SetInt("InvertMouse", settings.InvertMouse ? 1 : 0);
			PlayerPrefs.SetInt("SprintMode", (int)settings.SprintMode);
			string value = GameInput.PlayerInput.actions.SaveBindingOverridesAsJson();
			PlayerPrefs.SetString("BindingOverrides", value);
		}

		public InputSettings ReadInputSettings()
		{
			return new InputSettings
			{
				MouseSensitivity = PlayerPrefs.GetFloat("MouseSensitivity", 1f),
				InvertMouse = (PlayerPrefs.GetInt("InvertMouse", 0) == 1),
				SprintMode = (InputSettings.EActionMode)PlayerPrefs.GetInt("SprintMode", 0),
				BindingOverrides = PlayerPrefs.GetString("BindingOverrides", GameInput.PlayerInput.actions.SaveBindingOverridesAsJson())
			};
		}

		public string GetActionControlPath(string actionName)
		{
			InputAction inputAction = playerControls.FindAction(actionName);
			if (inputAction == null)
			{
				Console.LogError("Could not find action with name '" + actionName + "'");
				return string.Empty;
			}
			return inputAction.controls[0].path;
		}
	}
}
