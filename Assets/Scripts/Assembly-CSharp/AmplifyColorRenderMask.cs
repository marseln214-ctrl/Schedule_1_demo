using AmplifyColor;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.XR;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
[RequireComponent(typeof(AmplifyColorEffect))]
[AddComponentMenu("Image Effects/Amplify Color Render Mask")]
public class AmplifyColorRenderMask : MonoBehaviour
{
	[FormerlySerializedAs("clearColor")]
	public Color ClearColor = Color.black;

	[FormerlySerializedAs("renderLayers")]
	public RenderLayer[] RenderLayers = new RenderLayer[0];

	[FormerlySerializedAs("debug")]
	public bool DebugMask;

	private Camera referenceCamera;

	private Camera maskCamera;

	private AmplifyColorEffect colorEffect;

	private int width;

	private int height;

	private RenderTexture maskTexture;

	private Shader colorMaskShader;

	private bool singlePassStereo;

	private void OnEnable()
	{
		if (maskCamera == null)
		{
			GameObject gameObject = new GameObject("Mask Camera", typeof(Camera))
			{
				hideFlags = HideFlags.HideAndDontSave
			};
			gameObject.transform.parent = base.gameObject.transform;
			maskCamera = gameObject.GetComponent<Camera>();
		}
		referenceCamera = GetComponent<Camera>();
		colorEffect = GetComponent<AmplifyColorEffect>();
		colorMaskShader = Shader.Find("Hidden/RenderMask");
	}

	private void OnDisable()
	{
		DestroyCamera();
		DestroyRenderTextures();
	}

	private void DestroyCamera()
	{
		if (maskCamera != null)
		{
			Object.DestroyImmediate(maskCamera.gameObject);
			maskCamera = null;
		}
	}

	private void DestroyRenderTextures()
	{
		if (maskTexture != null)
		{
			RenderTexture.active = null;
			Object.DestroyImmediate(maskTexture);
			maskTexture = null;
		}
	}

	private void UpdateRenderTextures(bool singlePassStereo)
	{
		int num = referenceCamera.pixelWidth;
		int num2 = referenceCamera.pixelHeight;
		if (maskTexture == null || width != num || height != num2 || !maskTexture.IsCreated() || this.singlePassStereo != singlePassStereo)
		{
			width = num;
			height = num2;
			DestroyRenderTextures();
			if (XRSettings.enabled)
			{
				num = XRSettings.eyeTextureWidth * ((!singlePassStereo) ? 1 : 2);
				num2 = XRSettings.eyeTextureHeight;
			}
			if (maskTexture == null)
			{
				maskTexture = new RenderTexture(num, num2, 24, RenderTextureFormat.Default, RenderTextureReadWrite.sRGB)
				{
					hideFlags = HideFlags.HideAndDontSave,
					name = "MaskTexture"
				};
				maskTexture.name = "AmplifyColorMaskTexture";
				bool allowMSAA = maskCamera.allowMSAA;
				maskTexture.antiAliasing = ((!allowMSAA || QualitySettings.antiAliasing <= 0) ? 1 : QualitySettings.antiAliasing);
			}
			maskTexture.Create();
			this.singlePassStereo = singlePassStereo;
		}
		if (colorEffect != null)
		{
			colorEffect.MaskTexture = maskTexture;
		}
	}

	private void UpdateCameraProperties()
	{
		maskCamera.CopyFrom(referenceCamera);
		maskCamera.targetTexture = maskTexture;
		maskCamera.clearFlags = CameraClearFlags.Nothing;
		maskCamera.renderingPath = RenderingPath.VertexLit;
		maskCamera.pixelRect = new Rect(0f, 0f, width, height);
		maskCamera.depthTextureMode = DepthTextureMode.None;
		maskCamera.allowHDR = false;
		maskCamera.enabled = false;
	}

	private void OnPreRender()
	{
		if (!(maskCamera != null))
		{
			return;
		}
		RenderBuffer activeColorBuffer = Graphics.activeColorBuffer;
		RenderBuffer activeDepthBuffer = Graphics.activeDepthBuffer;
		bool flag = false;
		if (referenceCamera.stereoEnabled)
		{
			flag = XRSettings.eyeTextureDesc.vrUsage == VRTextureUsage.TwoEyes;
			maskCamera.SetStereoViewMatrix(Camera.StereoscopicEye.Left, referenceCamera.GetStereoViewMatrix(Camera.StereoscopicEye.Left));
			maskCamera.SetStereoViewMatrix(Camera.StereoscopicEye.Right, referenceCamera.GetStereoViewMatrix(Camera.StereoscopicEye.Right));
			maskCamera.SetStereoProjectionMatrix(Camera.StereoscopicEye.Left, referenceCamera.GetStereoProjectionMatrix(Camera.StereoscopicEye.Left));
			maskCamera.SetStereoProjectionMatrix(Camera.StereoscopicEye.Right, referenceCamera.GetStereoProjectionMatrix(Camera.StereoscopicEye.Right));
		}
		UpdateRenderTextures(flag);
		UpdateCameraProperties();
		Graphics.SetRenderTarget(maskTexture);
		GL.Clear(clearDepth: true, clearColor: true, ClearColor);
		if (flag)
		{
			maskCamera.worldToCameraMatrix = referenceCamera.GetStereoViewMatrix(Camera.StereoscopicEye.Left);
			maskCamera.projectionMatrix = referenceCamera.GetStereoProjectionMatrix(Camera.StereoscopicEye.Left);
			maskCamera.rect = new Rect(0f, 0f, 0.5f, 1f);
		}
		RenderLayer[] renderLayers = RenderLayers;
		for (int i = 0; i < renderLayers.Length; i++)
		{
			RenderLayer renderLayer = renderLayers[i];
			Shader.SetGlobalColor("_COLORMASK_Color", renderLayer.color);
			maskCamera.cullingMask = renderLayer.mask;
			maskCamera.RenderWithShader(colorMaskShader, "RenderType");
		}
		if (flag)
		{
			maskCamera.worldToCameraMatrix = referenceCamera.GetStereoViewMatrix(Camera.StereoscopicEye.Right);
			maskCamera.projectionMatrix = referenceCamera.GetStereoProjectionMatrix(Camera.StereoscopicEye.Right);
			maskCamera.rect = new Rect(0.5f, 0f, 0.5f, 1f);
			renderLayers = RenderLayers;
			for (int i = 0; i < renderLayers.Length; i++)
			{
				RenderLayer renderLayer2 = renderLayers[i];
				Shader.SetGlobalColor("_COLORMASK_Color", renderLayer2.color);
				maskCamera.cullingMask = renderLayer2.mask;
				maskCamera.RenderWithShader(colorMaskShader, "RenderType");
			}
		}
		Graphics.SetRenderTarget(activeColorBuffer, activeDepthBuffer);
	}
}
