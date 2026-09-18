using UnityEngine;

namespace ScheduleOne.AvatarFramework
{
	public class EyebrowController : MonoBehaviour
	{
		private static float eyebrowHeightMultiplier = 0.3f;

		[Header("References")]
		public Eyebrow leftBrow;

		public Eyebrow rightBrow;

		public void ApplySettings(AvatarSettings settings)
		{
			SetLeftBrowRestingHeight(settings.EyebrowRestingHeight);
			SetRightBrowRestingHeight(settings.EyebrowRestingHeight);
			leftBrow.SetScale(settings.EyebrowScale);
			rightBrow.SetScale(settings.EyebrowScale);
			leftBrow.SetThickness(settings.EyebrowThickness);
			rightBrow.SetThickness(settings.EyebrowThickness);
			leftBrow.SetRestingAngle(settings.EyebrowRestingAngle);
			rightBrow.SetRestingAngle(settings.EyebrowRestingAngle);
			leftBrow.SetColor(settings.HairColor);
			rightBrow.SetColor(settings.HairColor);
		}

		public void SetLeftBrowRestingHeight(float normalizedHeight)
		{
			normalizedHeight = Mathf.Clamp(normalizedHeight, -1.1f, 1.5f);
			leftBrow.transform.localPosition = new Vector3(0f, normalizedHeight * eyebrowHeightMultiplier, 0f);
		}

		public void SetRightBrowRestingHeight(float normalizedHeight)
		{
			normalizedHeight = Mathf.Clamp(normalizedHeight, -1.1f, 1.5f);
			rightBrow.transform.localPosition = new Vector3(0f, normalizedHeight * eyebrowHeightMultiplier, 0f);
		}
	}
}
