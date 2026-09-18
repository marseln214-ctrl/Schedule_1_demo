using System;
using UnityEngine;

namespace ScheduleOne.AvatarFramework.Emotions
{
	[Serializable]
	public class AvatarEmotionPreset
	{
		public string PresetName = "Preset Name";

		public Texture2D FaceTexture;

		public Eye.EyeLidConfiguration LeftEyeRestingState;

		public Eye.EyeLidConfiguration RightEyeRestingState;

		[Range(-30f, 30f)]
		public float BrowAngleChange_L;

		[Range(-30f, 30f)]
		public float BrowAngleChange_R;

		[Range(-1f, 1f)]
		public float BrowHeightChange_L;

		[Range(-1f, 1f)]
		public float BrowHeightChange_R;

		public static AvatarEmotionPreset Lerp(AvatarEmotionPreset start, AvatarEmotionPreset end, AvatarEmotionPreset neutralPreset, float lerp)
		{
			AvatarEmotionPreset obj = new AvatarEmotionPreset
			{
				PresetName = "Lerp",
				FaceTexture = ((lerp > 0f) ? end.FaceTexture : start.FaceTexture),
				LeftEyeRestingState = Eye.EyeLidConfiguration.Lerp(start.LeftEyeRestingState, end.LeftEyeRestingState, lerp),
				RightEyeRestingState = Eye.EyeLidConfiguration.Lerp(start.RightEyeRestingState, end.RightEyeRestingState, lerp)
			};
			float browAngleChange_L = start.BrowAngleChange_L;
			float browAngleChange_R = start.BrowAngleChange_R;
			float browHeightChange_L = start.BrowHeightChange_L;
			float browHeightChange_R = start.BrowHeightChange_R;
			float num = end.BrowAngleChange_L;
			float num2 = end.BrowAngleChange_R;
			float num3 = end.BrowHeightChange_L;
			float num4 = end.BrowHeightChange_R;
			if (end.PresetName != "Neutral")
			{
				num += neutralPreset.BrowAngleChange_L;
				num2 += neutralPreset.BrowAngleChange_R;
				num3 += neutralPreset.BrowHeightChange_L;
				num4 += neutralPreset.BrowHeightChange_R;
			}
			obj.BrowAngleChange_L = Mathf.Lerp(browAngleChange_L, num, lerp);
			obj.BrowAngleChange_R = Mathf.Lerp(browAngleChange_R, num2, lerp);
			obj.BrowHeightChange_L = Mathf.Lerp(browHeightChange_L, num3, lerp);
			obj.BrowHeightChange_R = Mathf.Lerp(browHeightChange_R, num4, lerp);
			return obj;
		}
	}
}
