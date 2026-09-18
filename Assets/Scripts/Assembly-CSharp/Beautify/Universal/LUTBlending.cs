using UnityEngine;

namespace Beautify.Universal
{
	[ExecuteInEditMode]
	public class LUTBlending : MonoBehaviour
	{
		private static class ShaderParams
		{
			public static int LUT2 = Shader.PropertyToID("_LUT2");

			public static int Phase = Shader.PropertyToID("_Phase");
		}

		public Texture2D LUT1;

		public Texture2D LUT2;

		[Range(0f, 1f)]
		public float LUT1Intensity = 1f;

		[Range(0f, 1f)]
		public float LUT2Intensity = 1f;

		[Range(0f, 1f)]
		public float phase;

		public Shader lerpShader;

		private float oldPhase = -1f;

		private RenderTexture rt;

		private Material lerpMat;

		private void OnEnable()
		{
			UpdateBeautifyLUT();
		}

		private void OnValidate()
		{
			oldPhase = -1f;
			UpdateBeautifyLUT();
		}

		private void OnDestroy()
		{
			if (rt != null)
			{
				rt.Release();
			}
		}

		private void LateUpdate()
		{
			UpdateBeautifyLUT();
		}

		private void UpdateBeautifyLUT()
		{
			if (oldPhase != phase && !(LUT1 == null) && !(LUT2 == null) && !(lerpShader == null))
			{
				oldPhase = phase;
				if (rt == null)
				{
					rt = new RenderTexture(LUT1.width, LUT1.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
					rt.filterMode = FilterMode.Point;
				}
				if (lerpMat == null)
				{
					lerpMat = new Material(lerpShader);
				}
				lerpMat.SetTexture(ShaderParams.LUT2, LUT2);
				lerpMat.SetFloat(ShaderParams.Phase, phase);
				Graphics.Blit(LUT1, rt, lerpMat);
				//BeautifySettings.settings.lut.Override(x: true);
				float x = Mathf.Lerp(LUT1Intensity, LUT2Intensity, phase);
				//BeautifySettings.settings.lutIntensity.Override(x);
				//BeautifySettings.settings.lutTexture.Override(rt);
			}
		}
	}
}
