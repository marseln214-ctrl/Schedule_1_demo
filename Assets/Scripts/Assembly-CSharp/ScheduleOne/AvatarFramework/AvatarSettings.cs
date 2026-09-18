using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace ScheduleOne.AvatarFramework
{
	[Serializable]
	[CreateAssetMenu(fileName = "Avatar Settings", menuName = "ScriptableObjects/Avatar Settings", order = 1)]
	public class AvatarSettings : ScriptableObject
	{
		[Serializable]
		public struct LayerSetting
		{
			public string layerPath;

			public Color layerTint;
		}

		[Serializable]
		public class AccessorySetting
		{
			public string path;

			public Color color;
		}

		public Color SkinColor;

		public float Height;

		public float Gender;

		public float Weight;

		public string HairPath;

		public Color HairColor;

		public float EyebrowScale;

		public float EyebrowThickness;

		public float EyebrowRestingHeight;

		public float EyebrowRestingAngle;

		public Color LeftEyeLidColor;

		public Color RightEyeLidColor;

		public Eye.EyeLidConfiguration LeftEyeRestingState;

		public Eye.EyeLidConfiguration RightEyeRestingState;

		public string EyeballMaterialIdentifier;

		public Color EyeBallTint;

		public float PupilDilation;

		public List<LayerSetting> FaceLayerSettings = new List<LayerSetting>();

		public List<LayerSetting> BodyLayerSettings = new List<LayerSetting>();

		public List<AccessorySetting> AccessorySettings = new List<AccessorySetting>();

		public bool UseCombinedLayer;

		public string CombinedLayerPath;

		public float UpperEyelidRestingPosition => LeftEyeRestingState.topLidOpen;

		public float LowerEyelidRestingPosition => LeftEyeRestingState.bottomLidOpen;

		public string FaceLayer1Path
		{
			get
			{
				if (FaceLayerSettings.Count <= 0)
				{
					return null;
				}
				return FaceLayerSettings[0].layerPath;
			}
		}

		public Color FaceLayer1Color
		{
			get
			{
				if (FaceLayerSettings.Count <= 0)
				{
					return Color.white;
				}
				return FaceLayerSettings[0].layerTint;
			}
		}

		public string FaceLayer2Path
		{
			get
			{
				if (FaceLayerSettings.Count <= 1)
				{
					return null;
				}
				return FaceLayerSettings[1].layerPath;
			}
		}

		public Color FaceLayer2Color
		{
			get
			{
				if (FaceLayerSettings.Count <= 1)
				{
					return Color.white;
				}
				return FaceLayerSettings[1].layerTint;
			}
		}

		public string FaceLayer3Path
		{
			get
			{
				if (FaceLayerSettings.Count <= 2)
				{
					return null;
				}
				return FaceLayerSettings[2].layerPath;
			}
		}

		public Color FaceLayer3Color
		{
			get
			{
				if (FaceLayerSettings.Count <= 2)
				{
					return Color.white;
				}
				return FaceLayerSettings[2].layerTint;
			}
		}

		public string FaceLayer4Path
		{
			get
			{
				if (FaceLayerSettings.Count <= 3)
				{
					return null;
				}
				return FaceLayerSettings[3].layerPath;
			}
		}

		public Color FaceLayer4Color
		{
			get
			{
				if (FaceLayerSettings.Count <= 3)
				{
					return Color.white;
				}
				return FaceLayerSettings[3].layerTint;
			}
		}

		public string FaceLayer5Path
		{
			get
			{
				if (FaceLayerSettings.Count <= 4)
				{
					return null;
				}
				return FaceLayerSettings[4].layerPath;
			}
		}

		public Color FaceLayer5Color
		{
			get
			{
				if (FaceLayerSettings.Count <= 4)
				{
					return Color.white;
				}
				return FaceLayerSettings[4].layerTint;
			}
		}

		public string FaceLayer6Path
		{
			get
			{
				if (FaceLayerSettings.Count <= 5)
				{
					return null;
				}
				return FaceLayerSettings[5].layerPath;
			}
		}

		public Color FaceLayer6Color
		{
			get
			{
				if (FaceLayerSettings.Count <= 5)
				{
					return Color.white;
				}
				return FaceLayerSettings[5].layerTint;
			}
		}

		public string BodyLayer1Path
		{
			get
			{
				if (BodyLayerSettings.Count <= 0)
				{
					return null;
				}
				return BodyLayerSettings[0].layerPath;
			}
		}

		public Color BodyLayer1Color
		{
			get
			{
				if (BodyLayerSettings.Count <= 0)
				{
					return Color.white;
				}
				return BodyLayerSettings[0].layerTint;
			}
		}

		public string BodyLayer2Path
		{
			get
			{
				if (BodyLayerSettings.Count <= 1)
				{
					return null;
				}
				return BodyLayerSettings[1].layerPath;
			}
		}

		public Color BodyLayer2Color
		{
			get
			{
				if (BodyLayerSettings.Count <= 1)
				{
					return Color.white;
				}
				return BodyLayerSettings[1].layerTint;
			}
		}

		public string BodyLayer3Path
		{
			get
			{
				if (BodyLayerSettings.Count <= 2)
				{
					return null;
				}
				return BodyLayerSettings[2].layerPath;
			}
		}

		public Color BodyLayer3Color
		{
			get
			{
				if (BodyLayerSettings.Count <= 2)
				{
					return Color.white;
				}
				return BodyLayerSettings[2].layerTint;
			}
		}

		public string BodyLayer4Path
		{
			get
			{
				if (BodyLayerSettings.Count <= 3)
				{
					return null;
				}
				return BodyLayerSettings[3].layerPath;
			}
		}

		public Color BodyLayer4Color
		{
			get
			{
				if (BodyLayerSettings.Count <= 3)
				{
					return Color.white;
				}
				return BodyLayerSettings[3].layerTint;
			}
		}

		public string BodyLayer5Path
		{
			get
			{
				if (BodyLayerSettings.Count <= 4)
				{
					return null;
				}
				return BodyLayerSettings[4].layerPath;
			}
		}

		public Color BodyLayer5Color
		{
			get
			{
				if (BodyLayerSettings.Count <= 4)
				{
					return Color.white;
				}
				return BodyLayerSettings[4].layerTint;
			}
		}

		public string BodyLayer6Path
		{
			get
			{
				if (BodyLayerSettings.Count <= 5)
				{
					return null;
				}
				return BodyLayerSettings[5].layerPath;
			}
		}

		public Color BodyLayer6Color
		{
			get
			{
				if (BodyLayerSettings.Count <= 5)
				{
					return Color.white;
				}
				return BodyLayerSettings[5].layerTint;
			}
		}

		public string Accessory1Path
		{
			get
			{
				if (AccessorySettings.Count <= 0)
				{
					return null;
				}
				return AccessorySettings[0].path;
			}
		}

		public Color Accessory1Color
		{
			get
			{
				if (AccessorySettings.Count <= 0)
				{
					return Color.white;
				}
				return AccessorySettings[0].color;
			}
		}

		public string Accessory2Path
		{
			get
			{
				if (AccessorySettings.Count <= 1)
				{
					return null;
				}
				return AccessorySettings[1].path;
			}
		}

		public Color Accessory2Color
		{
			get
			{
				if (AccessorySettings.Count <= 1)
				{
					return Color.white;
				}
				return AccessorySettings[1].color;
			}
		}

		public string Accessory3Path
		{
			get
			{
				if (AccessorySettings.Count <= 2)
				{
					return null;
				}
				return AccessorySettings[2].path;
			}
		}

		public Color Accessory3Color
		{
			get
			{
				if (AccessorySettings.Count <= 2)
				{
					return Color.white;
				}
				return AccessorySettings[2].color;
			}
		}

		public string Accessory4Path
		{
			get
			{
				if (AccessorySettings.Count <= 3)
				{
					return null;
				}
				return AccessorySettings[3].path;
			}
		}

		public Color Accessory4Color
		{
			get
			{
				if (AccessorySettings.Count <= 3)
				{
					return Color.white;
				}
				return AccessorySettings[3].color;
			}
		}

		public string Accessory5Path
		{
			get
			{
				if (AccessorySettings.Count <= 4)
				{
					return null;
				}
				return AccessorySettings[4].path;
			}
		}

		public Color Accessory5Color
		{
			get
			{
				if (AccessorySettings.Count <= 4)
				{
					return Color.white;
				}
				return AccessorySettings[4].color;
			}
		}

		public string Accessory6Path
		{
			get
			{
				if (AccessorySettings.Count <= 5)
				{
					return null;
				}
				return AccessorySettings[5].path;
			}
		}

		public Color Accessory6Color
		{
			get
			{
				if (AccessorySettings.Count <= 5)
				{
					return Color.white;
				}
				return AccessorySettings[5].color;
			}
		}

		public string Accessory7Path
		{
			get
			{
				if (AccessorySettings.Count <= 6)
				{
					return null;
				}
				return AccessorySettings[6].path;
			}
		}

		public Color Accessory7Color
		{
			get
			{
				if (AccessorySettings.Count <= 6)
				{
					return Color.white;
				}
				return AccessorySettings[6].color;
			}
		}

		public string Accessory8Path
		{
			get
			{
				if (AccessorySettings.Count <= 7)
				{
					return null;
				}
				return AccessorySettings[7].path;
			}
		}

		public Color Accessory8Color
		{
			get
			{
				if (AccessorySettings.Count <= 7)
				{
					return Color.white;
				}
				return AccessorySettings[7].color;
			}
		}

		public string Accessory9Path
		{
			get
			{
				if (AccessorySettings.Count <= 8)
				{
					return null;
				}
				return AccessorySettings[8].path;
			}
		}

		public Color Accessory9Color
		{
			get
			{
				if (AccessorySettings.Count <= 8)
				{
					return Color.white;
				}
				return AccessorySettings[8].color;
			}
		}

		public object this[string propertyName]
		{
			get
			{
				FieldInfo field = GetType().GetField(propertyName);
				PropertyInfo property = GetType().GetProperty(propertyName);
				if (field != null)
				{
					return field.GetValue(this);
				}
				if (property != null)
				{
					return property.GetValue(this, null);
				}
				return null;
			}
		}

		public virtual string GetJson(bool prettyPrint = true)
		{
			return JsonUtility.ToJson(this, prettyPrint);
		}
	}
}
