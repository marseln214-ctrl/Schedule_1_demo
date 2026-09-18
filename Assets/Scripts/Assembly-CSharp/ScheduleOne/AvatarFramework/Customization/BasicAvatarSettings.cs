using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace ScheduleOne.AvatarFramework.Customization
{
	[Serializable]
	[CreateAssetMenu(fileName = "BasicAvatarSettings", menuName = "ScriptableObjects/BasicAvatarSettings", order = 1)]
	public class BasicAvatarSettings : ScriptableObject
	{
		public const float GENDER_MULTIPLIER = 0.7f;

		public const string MaleUnderwearPath = "Avatar/Layers/Bottom/MaleUnderwear";

		public const string FemaleUnderwearPath = "Avatar/Layers/Bottom/FemaleUnderwear";

		public int Gender;

		public float Weight;

		public Color SkinColor;

		public string HairStyle;

		public Color HairColor;

		public string Mouth;

		public string FacialHair;

		public string FacialDetails;

		public float FacialDetailsIntensity;

		public Color EyeballColor;

		public float UpperEyeLidRestingPosition;

		public float LowerEyeLidRestingPosition;

		public float PupilDilation = 1f;

		public float EyebrowScale;

		public float EyebrowThickness;

		public float EyebrowRestingHeight;

		public float EyebrowRestingAngle;

		public string Top;

		public Color TopColor;

		public string Bottom;

		public Color BottomColor;

		public string Shoes;

		public Color ShoesColor;

		public string Headwear;

		public Color HeadwearColor;

		public string Eyewear;

		public Color EyewearColor;

		public List<string> Tattoos = new List<string>();

		public T SetValue<T>(string fieldName, T value)
		{
			GetType().GetField(fieldName).SetValue(this, value);
			return value;
		}

		public T GetValue<T>(string fieldName)
		{
			FieldInfo field = GetType().GetField(fieldName);
			if (field == null)
			{
				return default(T);
			}
			return (T)field.GetValue(this);
		}

		public AvatarSettings GetAvatarSettings()
		{
			AvatarSettings avatarSettings = ScriptableObject.CreateInstance<AvatarSettings>();
			avatarSettings.Gender = (float)Gender * 0.7f;
			avatarSettings.Weight = Weight;
			avatarSettings.Height = 1f;
			avatarSettings.SkinColor = SkinColor;
			avatarSettings.HairPath = HairStyle;
			avatarSettings.HairColor = HairColor;
			avatarSettings.FaceLayerSettings.Add(new AvatarSettings.LayerSetting
			{
				layerPath = Mouth,
				layerTint = Color.black
			});
			avatarSettings.FaceLayerSettings.Add(new AvatarSettings.LayerSetting
			{
				layerPath = FacialHair,
				layerTint = Color.white
			});
			avatarSettings.FaceLayerSettings.Add(new AvatarSettings.LayerSetting
			{
				layerPath = FacialDetails,
				layerTint = new Color(0f, 0f, 0f, FacialDetailsIntensity)
			});
			avatarSettings.FaceLayerSettings.Add(new AvatarSettings.LayerSetting
			{
				layerPath = "Avatar/Layers/Face/EyeShadow",
				layerTint = new Color(0f, 0f, 0f, 0.7f)
			});
			avatarSettings.EyeBallTint = EyeballColor;
			avatarSettings.LeftEyeLidColor = SkinColor;
			avatarSettings.RightEyeLidColor = SkinColor;
			avatarSettings.EyeballMaterialIdentifier = "Default";
			avatarSettings.PupilDilation = PupilDilation;
			Eye.EyeLidConfiguration leftEyeRestingState = default(Eye.EyeLidConfiguration);
			leftEyeRestingState.topLidOpen = UpperEyeLidRestingPosition;
			leftEyeRestingState.bottomLidOpen = LowerEyeLidRestingPosition;
			avatarSettings.RightEyeRestingState = (avatarSettings.LeftEyeRestingState = leftEyeRestingState);
			avatarSettings.EyebrowScale = EyebrowScale;
			avatarSettings.EyebrowThickness = EyebrowThickness;
			avatarSettings.EyebrowRestingHeight = EyebrowRestingHeight;
			avatarSettings.EyebrowRestingAngle = EyebrowRestingAngle;
			avatarSettings.BodyLayerSettings.Add(new AvatarSettings.LayerSetting
			{
				layerPath = "Avatar/Layers/Top/Nipples",
				layerTint = new Color32(212, 181, 142, byte.MaxValue)
			});
			string layerPath = (((float)Gender <= 0.5f) ? "Avatar/Layers/Bottom/MaleUnderwear" : "Avatar/Layers/Bottom/FemaleUnderwear");
			avatarSettings.BodyLayerSettings.Add(new AvatarSettings.LayerSetting
			{
				layerPath = layerPath,
				layerTint = Color.white
			});
			if (!string.IsNullOrEmpty(Top))
			{
				avatarSettings.BodyLayerSettings.Add(new AvatarSettings.LayerSetting
				{
					layerPath = Top,
					layerTint = TopColor
				});
			}
			if (!string.IsNullOrEmpty(Bottom))
			{
				avatarSettings.BodyLayerSettings.Add(new AvatarSettings.LayerSetting
				{
					layerPath = Bottom,
					layerTint = BottomColor
				});
			}
			if (!string.IsNullOrEmpty(Shoes))
			{
				avatarSettings.AccessorySettings.Add(new AvatarSettings.AccessorySetting
				{
					path = Shoes,
					color = ShoesColor
				});
			}
			if (!string.IsNullOrEmpty(Headwear))
			{
				avatarSettings.AccessorySettings.Add(new AvatarSettings.AccessorySetting
				{
					path = Headwear,
					color = HeadwearColor
				});
			}
			if (!string.IsNullOrEmpty(Eyewear))
			{
				avatarSettings.AccessorySettings.Add(new AvatarSettings.AccessorySetting
				{
					path = Eyewear,
					color = EyewearColor
				});
			}
			return avatarSettings;
		}

		public virtual string GetJson(bool prettyPrint = true)
		{
			return JsonUtility.ToJson(this, prettyPrint);
		}
	}
}
