using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.ObjectScripts
{
	public class PotSoilCover : MonoBehaviour
	{
		public const int TEXTURE_SIZE = 128;

		public const int POUR_RADIUS = 32;

		public const int UPDATES_PER_SECOND = 24;

		public const float COVERAGE_THRESHOLD = 0.5f;

		public const float BASE_COVERAGE = 0.215f;

		public const float SUCCESS_COVERAGE_THRESHOLD = 0.95f;

		public const float DELAY = 0.35f;

		public float CurrentCoverage;

		[Header("Settings")]
		public float Radius;

		[Header("References")]
		public MeshRenderer MeshRenderer;

		public Texture2D PourMask;

		public UnityEvent onSufficientCoverage;

		private bool queued;

		private Vector3 queuedWorldPos = Vector3.zero;

		private Texture2D mainTex;

		private Vector3 relative;

		private Vector2 vector2;

		private Vector2 normalizedOffset;

		private Vector2 originPixel;

		private void Awake()
		{
		}

		private void OnEnable()
		{
			StartCoroutine(CheckQueue());
		}

		public void ConfigureAppearance(Color col, float transparency)
		{
			MeshRenderer.material.SetColor("_MainColor", col);
			MeshRenderer.material.SetFloat("_Transparency", transparency);
		}

		public void Reset()
		{
			Blank();
			CurrentCoverage = 0.215f;
		}

		public void QueuePour(Vector3 worldSpacePosition)
		{
			queued = true;
			queuedWorldPos = worldSpacePosition;
		}

		public float GetNormalizedProgress()
		{
			return (CurrentCoverage - 0.215f) / 0.735f;
		}

		private IEnumerator CheckQueue()
		{
			while (base.gameObject != null)
			{
				if (queued)
				{
					queued = false;
					DelayedApplyPour(queuedWorldPos);
				}
				yield return new WaitForSeconds(1f / 24f);
			}
		}

		private void Blank()
		{
			Texture2D texture2D = new Texture2D(128, 128);
			Color[] array = new Color[16384];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = Color.black;
			}
			texture2D.SetPixels(array);
			texture2D.Apply();
			MeshRenderer.material.mainTexture = texture2D;
			mainTex = texture2D;
		}

		private void DelayedApplyPour(Vector3 worldSpace)
		{
			StartCoroutine(Routine());
			IEnumerator Routine()
			{
				yield return new WaitForSeconds(0.35f);
				ApplyPour(worldSpace);
			}
		}

		private void ApplyPour(Vector3 worldSpace)
		{
			relative = base.transform.InverseTransformPoint(worldSpace);
			vector2 = new Vector2(relative.x, relative.z);
			if (vector2.magnitude > Radius)
			{
				return;
			}
			normalizedOffset = new Vector2(vector2.x / Radius, vector2.y / Radius);
			originPixel = new Vector2(64f * (1f + normalizedOffset.x), 64f * (1f + normalizedOffset.y));
			for (int i = 0; i < 64; i++)
			{
				for (int j = 0; j < 64; j++)
				{
					int num = (int)originPixel.x - 32 + i;
					int num2 = (int)originPixel.y - 32 + j;
					if (num >= 0 && num < 128 && num2 >= 0 && num2 < 128)
					{
						Color pixel = mainTex.GetPixel(num, num2);
						pixel.r += GetPourMaskValue(i, j);
						pixel.g = pixel.r;
						pixel.b = pixel.r;
						pixel.a = 1f;
						mainTex.SetPixel(num, num2, pixel);
					}
				}
			}
			mainTex.Apply();
			float currentCoverage = CurrentCoverage;
			if ((CurrentCoverage = GetCoverage()) >= 0.95f && currentCoverage < 0.95f && onSufficientCoverage != null)
			{
				onSufficientCoverage.Invoke();
			}
		}

		private float GetPourMaskValue(int x, int y)
		{
			return PourMask.GetPixel(x, y).grayscale;
		}

		private float GetCoverage()
		{
			int num = 16384;
			int num2 = 0;
			for (int i = 0; i < 128; i++)
			{
				for (int j = 0; j < 128; j++)
				{
					if (mainTex.GetPixel(i, j).r > 0.5f)
					{
						num2++;
					}
				}
			}
			return Mathf.Clamp01((float)num2 / (float)num + 0.215f);
		}
	}
}
