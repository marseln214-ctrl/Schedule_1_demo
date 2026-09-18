using ScheduleOne.DevUtilities;
using TMPro;
using UnityEngine;

namespace ScheduleOne.AvatarFramework.Customization
{
	public class CustomizationManager : Singleton<CustomizationManager>
	{
		public delegate void AvatarSettingsChanged(AvatarSettings settings);

		[SerializeField]
		private AvatarSettings ActiveSettings;

		public Avatar TemplateAvatar;

		public TMP_InputField SaveInputField;

		public TMP_InputField LoadInputField;

		public AvatarSettingsChanged OnAvatarSettingsChanged;

		public AvatarSettings DefaultSettings;

		private bool isEditingOriginal;

		protected override void Start()
		{
			base.Start();
			LoadSettings(Object.Instantiate(DefaultSettings));
		}

		public void CreateSettings(string name)
		{
		}

		public void CreateSettings()
		{
			if (SaveInputField.text == "")
			{
				Console.LogWarning("No name entered for settings file.");
			}
			else
			{
				CreateSettings(SaveInputField.text);
			}
		}

		public void LoadSettings(AvatarSettings loadedSettings)
		{
			if (loadedSettings == null)
			{
				Console.LogWarning("Settings are null!");
				return;
			}
			ActiveSettings = loadedSettings;
			Debug.Log("Settings loaded: " + ActiveSettings.name);
			TemplateAvatar.LoadAvatarSettings(ActiveSettings);
			if (OnAvatarSettingsChanged != null)
			{
				OnAvatarSettingsChanged(ActiveSettings);
			}
		}

		public void LoadSettings(string settingsName, bool editOriginal = false)
		{
			isEditingOriginal = editOriginal;
			AvatarSettings avatarSettings = null;
			if (editOriginal)
			{
				avatarSettings = Resources.Load<AvatarSettings>("CharacterSettings/" + settingsName);
				SaveInputField.SetTextWithoutNotify(settingsName);
			}
			else
			{
				avatarSettings = Object.Instantiate(Resources.Load<AvatarSettings>("CharacterSettings/" + settingsName));
			}
			LoadSettings(avatarSettings);
		}

		private void ApplyDefaultSettings(AvatarSettings settings)
		{
			settings.SkinColor = new Color32(150, 120, 95, byte.MaxValue);
			settings.Height = 0.98f;
			settings.Gender = 0f;
			settings.Weight = 0.4f;
			settings.EyebrowScale = 1f;
			settings.EyebrowThickness = 1f;
			settings.EyebrowRestingHeight = 0f;
			settings.EyebrowRestingAngle = 0f;
			settings.LeftEyeLidColor = new Color32(150, 120, 95, byte.MaxValue);
			settings.RightEyeLidColor = new Color32(150, 120, 95, byte.MaxValue);
			settings.LeftEyeRestingState = new Eye.EyeLidConfiguration
			{
				bottomLidOpen = 0.5f,
				topLidOpen = 0.5f
			};
			settings.RightEyeRestingState = new Eye.EyeLidConfiguration
			{
				bottomLidOpen = 0.5f,
				topLidOpen = 0.5f
			};
			settings.EyeballMaterialIdentifier = "Default";
			settings.EyeBallTint = Color.white;
			settings.PupilDilation = 1f;
			settings.HairPath = string.Empty;
			settings.HairColor = Color.black;
		}

		public void LoadSettings()
		{
			isEditingOriginal = true;
			Debug.Log("Loading!: " + LoadInputField.text);
			LoadSettings(LoadInputField.text, LoadInputField.text != "Default");
		}

		public void GenderChanged(float genderScale)
		{
			ActiveSettings.Gender = genderScale;
			TemplateAvatar.ApplyBodySettings(ActiveSettings);
		}

		public void WeightChanged(float weightScale)
		{
			ActiveSettings.Weight = weightScale;
			TemplateAvatar.ApplyBodySettings(ActiveSettings);
		}

		public void HeightChanged(float height)
		{
			ActiveSettings.Height = height;
			TemplateAvatar.ApplyBodySettings(ActiveSettings);
		}

		public void SkinColorChanged(Color col)
		{
			ActiveSettings.SkinColor = col;
			TemplateAvatar.ApplyBodySettings(ActiveSettings);
			if (Input.GetKey(KeyCode.LeftControl))
			{
				ActiveSettings.LeftEyeLidColor = col;
				ActiveSettings.RightEyeLidColor = col;
			}
			TemplateAvatar.ApplyEyeLidColorSettings(ActiveSettings);
		}

		public void HairChanged(Accessory newHair)
		{
			ActiveSettings.HairPath = ((newHair != null) ? newHair.AssetPath : string.Empty);
			TemplateAvatar.ApplyHairSettings(ActiveSettings);
		}

		public void HairColorChanged(Color newCol)
		{
			ActiveSettings.HairColor = newCol;
			TemplateAvatar.ApplyHairColorSettings(ActiveSettings);
		}

		public void EyeBallTintChanged(Color col)
		{
			ActiveSettings.EyeBallTint = col;
			TemplateAvatar.ApplyEyeBallSettings(ActiveSettings);
		}

		public void UpperEyeLidRestingPositionChanged(float newVal)
		{
			ActiveSettings.LeftEyeRestingState.topLidOpen = newVal;
			ActiveSettings.RightEyeRestingState.topLidOpen = newVal;
			TemplateAvatar.ApplyEyeLidSettings(ActiveSettings);
		}

		public void LowerEyeLidRestingPositionChanged(float newVal)
		{
			ActiveSettings.LeftEyeRestingState.bottomLidOpen = newVal;
			ActiveSettings.RightEyeRestingState.bottomLidOpen = newVal;
			TemplateAvatar.ApplyEyeLidSettings(ActiveSettings);
		}

		public void EyebrowScaleChanged(float newVal)
		{
			ActiveSettings.EyebrowScale = newVal;
			TemplateAvatar.ApplyEyebrowSettings(ActiveSettings);
		}

		public void EyebrowThicknessChanged(float newVal)
		{
			ActiveSettings.EyebrowThickness = newVal;
			TemplateAvatar.ApplyEyebrowSettings(ActiveSettings);
		}

		public void EyebrowRestingHeightChanged(float newVal)
		{
			ActiveSettings.EyebrowRestingHeight = newVal;
			TemplateAvatar.ApplyEyebrowSettings(ActiveSettings);
		}

		public void EyebrowRestingAngleChanged(float newVal)
		{
			ActiveSettings.EyebrowRestingAngle = newVal;
			TemplateAvatar.ApplyEyebrowSettings(ActiveSettings);
		}

		public void PupilDilationChanged(float dilation)
		{
			ActiveSettings.PupilDilation = dilation;
			TemplateAvatar.ApplyEyeBallSettings(ActiveSettings);
		}

		public void FaceLayerChanged(FaceLayer layer, int index)
		{
			string layerPath = ((layer != null) ? layer.AssetPath : string.Empty);
			Color layerTint = ActiveSettings.FaceLayerSettings[index].layerTint;
			ActiveSettings.FaceLayerSettings[index] = new AvatarSettings.LayerSetting
			{
				layerPath = layerPath,
				layerTint = layerTint
			};
			TemplateAvatar.ApplyFaceLayerSettings(ActiveSettings);
		}

		public void FaceLayerColorChanged(Color col, int index)
		{
			string layerPath = ActiveSettings.FaceLayerSettings[index].layerPath;
			ActiveSettings.FaceLayerSettings[index] = new AvatarSettings.LayerSetting
			{
				layerPath = layerPath,
				layerTint = col
			};
			TemplateAvatar.ApplyFaceLayerSettings(ActiveSettings);
		}

		public void BodyLayerChanged(AvatarLayer layer, int index)
		{
			string layerPath = ((layer != null) ? layer.AssetPath : string.Empty);
			Color layerTint = ActiveSettings.BodyLayerSettings[index].layerTint;
			ActiveSettings.BodyLayerSettings[index] = new AvatarSettings.LayerSetting
			{
				layerPath = layerPath,
				layerTint = layerTint
			};
			TemplateAvatar.ApplyBodyLayerSettings(ActiveSettings);
		}

		public void BodyLayerColorChanged(Color col, int index)
		{
			string layerPath = ActiveSettings.BodyLayerSettings[index].layerPath;
			ActiveSettings.BodyLayerSettings[index] = new AvatarSettings.LayerSetting
			{
				layerPath = layerPath,
				layerTint = col
			};
			TemplateAvatar.ApplyBodyLayerSettings(ActiveSettings);
		}

		public void AccessoryChanged(Accessory acc, int index)
		{
			Debug.Log("Accessory changed: " + acc?.AssetPath);
			string path = ((acc != null) ? acc.AssetPath : string.Empty);
			while (ActiveSettings.AccessorySettings.Count <= index)
			{
				ActiveSettings.AccessorySettings.Add(new AvatarSettings.AccessorySetting());
			}
			Color color = ActiveSettings.AccessorySettings[index].color;
			ActiveSettings.AccessorySettings[index] = new AvatarSettings.AccessorySetting
			{
				path = path,
				color = color
			};
			TemplateAvatar.ApplyAccessorySettings(ActiveSettings);
		}

		public void AccessoryColorChanged(Color col, int index)
		{
			string path = ActiveSettings.AccessorySettings[index].path;
			ActiveSettings.AccessorySettings[index] = new AvatarSettings.AccessorySetting
			{
				path = path,
				color = col
			};
			TemplateAvatar.ApplyAccessorySettings(ActiveSettings);
		}
	}
}
