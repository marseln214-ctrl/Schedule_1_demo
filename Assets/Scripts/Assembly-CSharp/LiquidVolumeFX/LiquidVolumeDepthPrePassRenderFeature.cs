using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace LiquidVolumeFX
{
	public class LiquidVolumeDepthPrePassRenderFeature : ScriptableRendererFeature
	{
		private static class ShaderParams
		{
			public const string RTBackBufferName = "_VLBackBufferTexture";

			public static int RTBackBuffer = Shader.PropertyToID("_VLBackBufferTexture");

			public const string RTFrontBufferName = "_VLFrontBufferTexture";

			public static int RTFrontBuffer = Shader.PropertyToID("_VLFrontBufferTexture");

			public static int FlaskThickness = Shader.PropertyToID("_FlaskThickness");

			public static int ForcedInvisible = Shader.PropertyToID("_LVForcedInvisible");

			public const string SKW_FP_RENDER_TEXTURE = "LIQUID_VOLUME_FP_RENDER_TEXTURES";
		}

		private enum Pass
		{
			BackBuffer = 0,
			FrontBuffer = 1
		}

		private class DepthPass : ScriptableRenderPass
		{
			private class PassData
			{
				public Camera cam;

				public CommandBuffer cmd;

				public DepthPass depthPass;

				public Material mat;

				public RTHandle source;

				public RTHandle depth;

				public RenderTextureDescriptor cameraTargetDescriptor;
			}

			private const string profilerTag = "LiquidVolumeDepthPrePass";

			private Material mat;

			private int targetNameId;

			private RTHandle targetRT;

			private int passId;

			private List<LiquidVolume> lvRenderers;

			public ScriptableRenderer renderer;

			public bool interleavedRendering;

			private static Vector3 currentCameraPosition;

			private readonly PassData passData = new PassData();

			public DepthPass(Material mat, Pass pass, RenderPassEvent renderPassEvent)
			{
				base.renderPassEvent = renderPassEvent;
				this.mat = mat;
				passData.depthPass = this;
				switch (pass)
				{
				case Pass.BackBuffer:
				{
					targetNameId = ShaderParams.RTBackBuffer;
					RenderTargetIdentifier tex2 = new RenderTargetIdentifier(targetNameId, 0, CubemapFace.Unknown, -1);
					targetRT = RTHandles.Alloc(tex2, "_VLBackBufferTexture");
					passId = 0;
					lvRenderers = lvBackRenderers;
					break;
				}
				case Pass.FrontBuffer:
				{
					targetNameId = ShaderParams.RTFrontBuffer;
					RenderTargetIdentifier tex = new RenderTargetIdentifier(targetNameId, 0, CubemapFace.Unknown, -1);
					targetRT = RTHandles.Alloc(tex, "_VLFrontBufferTexture");
					passId = 1;
					lvRenderers = lvFrontRenderers;
					break;
				}
				}
			}

			public void Setup(LiquidVolumeDepthPrePassRenderFeature feature, ScriptableRenderer renderer)
			{
				this.renderer = renderer;
				interleavedRendering = feature.interleavedRendering;
			}

			private int SortByDistanceToCamera(LiquidVolume lv1, LiquidVolume lv2)
			{
				bool flag = lv1 == null;
				bool flag2 = lv2 == null;
				if (flag && flag2)
				{
					return 0;
				}
				if (flag2)
				{
					return 1;
				}
				if (flag)
				{
					return -1;
				}
				float num = Vector3.Distance(lv1.transform.position, currentCameraPosition);
				float num2 = Vector3.Distance(lv2.transform.position, currentCameraPosition);
				if (num < num2)
				{
					return 1;
				}
				if (num > num2)
				{
					return -1;
				}
				return 0;
			}

			public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
			{
				cameraTextureDescriptor.colorFormat = (LiquidVolume.useFPRenderTextures ? RenderTextureFormat.RHalf : RenderTextureFormat.ARGB32);
				cameraTextureDescriptor.sRGB = false;
				cameraTextureDescriptor.depthBufferBits = 16;
				cameraTextureDescriptor.msaaSamples = 1;
				cmd.GetTemporaryRT(targetNameId, cameraTextureDescriptor);
				if (!interleavedRendering)
				{
					ConfigureTarget(targetRT);
				}
				ConfigureInput(ScriptableRenderPassInput.Depth);
			}

			public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
			{
				if (lvRenderers != null)
				{
					CommandBuffer commandBuffer = CommandBufferPool.Get("LiquidVolumeDepthPrePass");
					commandBuffer.Clear();
					passData.cam = renderingData.cameraData.camera;
					passData.cmd = commandBuffer;
					passData.mat = mat;
					passData.cameraTargetDescriptor = renderingData.cameraData.cameraTargetDescriptor;
					passData.source = renderer.cameraColorTargetHandle;
					passData.depth = renderer.cameraDepthTargetHandle;
					ExecutePass(passData);
					context.ExecuteCommandBuffer(commandBuffer);
					CommandBufferPool.Release(commandBuffer);
				}
			}

			private static void ExecutePass(PassData passData)
			{
				CommandBuffer cmd = passData.cmd;
				cmd.SetGlobalFloat(ShaderParams.ForcedInvisible, 0f);
				Camera cam = passData.cam;
				DepthPass depthPass = passData.depthPass;
				RenderTextureDescriptor cameraTargetDescriptor = passData.cameraTargetDescriptor;
				cameraTargetDescriptor.colorFormat = (LiquidVolume.useFPRenderTextures ? RenderTextureFormat.RHalf : RenderTextureFormat.ARGB32);
				cameraTargetDescriptor.sRGB = false;
				cameraTargetDescriptor.depthBufferBits = 16;
				cameraTargetDescriptor.msaaSamples = 1;
				cmd.GetTemporaryRT(depthPass.targetNameId, cameraTargetDescriptor);
				int count = depthPass.lvRenderers.Count;
				if (depthPass.interleavedRendering)
				{
					RenderTargetIdentifier rt = new RenderTargetIdentifier(depthPass.targetNameId, 0, CubemapFace.Unknown, -1);
					currentCameraPosition = cam.transform.position;
					depthPass.lvRenderers.Sort(depthPass.SortByDistanceToCamera);
					for (int i = 0; i < count; i++)
					{
						LiquidVolume liquidVolume = depthPass.lvRenderers[i];
						if (!(liquidVolume != null) || !liquidVolume.isActiveAndEnabled)
						{
							continue;
						}
						if (liquidVolume.topology == TOPOLOGY.Irregular)
						{
							cmd.SetRenderTarget(rt, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
							if (LiquidVolume.useFPRenderTextures)
							{
								cmd.ClearRenderTarget(clearDepth: true, clearColor: true, new Color(cam.farClipPlane, 0f, 0f, 0f), 1f);
								cmd.EnableShaderKeyword("LIQUID_VOLUME_FP_RENDER_TEXTURES");
							}
							else
							{
								cmd.ClearRenderTarget(clearDepth: true, clearColor: true, new Color(84f / 85f, 0.4470558f, 0.75f, 0f), 1f);
								cmd.DisableShaderKeyword("LIQUID_VOLUME_FP_RENDER_TEXTURES");
							}
							cmd.SetGlobalFloat(ShaderParams.FlaskThickness, 1f - liquidVolume.flaskThickness);
							cmd.DrawRenderer(liquidVolume.mr, passData.mat, (liquidVolume.subMeshIndex >= 0) ? liquidVolume.subMeshIndex : 0, depthPass.passId);
						}
						RenderTargetIdentifier color = new RenderTargetIdentifier(passData.source, 0, CubemapFace.Unknown, -1);
						RenderTargetIdentifier depth = new RenderTargetIdentifier(passData.depth, 0, CubemapFace.Unknown, -1);
						cmd.SetRenderTarget(color, depth);
						cmd.DrawRenderer(liquidVolume.mr, liquidVolume.liqMat, (liquidVolume.subMeshIndex >= 0) ? liquidVolume.subMeshIndex : 0, 1);
					}
					cmd.SetGlobalFloat(ShaderParams.ForcedInvisible, 1f);
					return;
				}
				RenderTargetIdentifier renderTargetIdentifier = new RenderTargetIdentifier(depthPass.targetNameId, 0, CubemapFace.Unknown, -1);
				cmd.SetRenderTarget(renderTargetIdentifier);
				cmd.SetGlobalTexture(depthPass.targetNameId, renderTargetIdentifier);
				if (LiquidVolume.useFPRenderTextures)
				{
					cmd.ClearRenderTarget(clearDepth: true, clearColor: true, new Color(cam.farClipPlane, 0f, 0f, 0f), 1f);
					cmd.EnableShaderKeyword("LIQUID_VOLUME_FP_RENDER_TEXTURES");
				}
				else
				{
					cmd.ClearRenderTarget(clearDepth: true, clearColor: true, new Color(84f / 85f, 0.4470558f, 0.75f, 0f), 1f);
					cmd.DisableShaderKeyword("LIQUID_VOLUME_FP_RENDER_TEXTURES");
				}
				for (int j = 0; j < count; j++)
				{
					LiquidVolume liquidVolume2 = depthPass.lvRenderers[j];
					if (liquidVolume2 != null && liquidVolume2.isActiveAndEnabled)
					{
						cmd.SetGlobalFloat(ShaderParams.FlaskThickness, 1f - liquidVolume2.flaskThickness);
						cmd.DrawRenderer(liquidVolume2.mr, passData.mat, (liquidVolume2.subMeshIndex >= 0) ? liquidVolume2.subMeshIndex : 0, depthPass.passId);
					}
				}
			}

			public void CleanUp()
			{
				RTHandles.Release(targetRT);
			}
		}

		public static readonly List<LiquidVolume> lvBackRenderers = new List<LiquidVolume>();

		public static readonly List<LiquidVolume> lvFrontRenderers = new List<LiquidVolume>();

		[SerializeField]
		[HideInInspector]
		private Shader shader;

		public static bool installed;

		private Material mat;

		private DepthPass backPass;

		private DepthPass frontPass;

		[Tooltip("Renders each irregular liquid volume completely before rendering the next one.")]
		public bool interleavedRendering;

		public RenderPassEvent renderPassEvent = RenderPassEvent.BeforeRenderingTransparents;

		public static void AddLiquidToBackRenderers(LiquidVolume lv)
		{
			if (!(lv == null) && lv.topology == TOPOLOGY.Irregular && !lvBackRenderers.Contains(lv))
			{
				lvBackRenderers.Add(lv);
			}
		}

		public static void RemoveLiquidFromBackRenderers(LiquidVolume lv)
		{
			if (!(lv == null) && lvBackRenderers.Contains(lv))
			{
				lvBackRenderers.Remove(lv);
			}
		}

		public static void AddLiquidToFrontRenderers(LiquidVolume lv)
		{
			if (!(lv == null) && lv.topology == TOPOLOGY.Irregular && !lvFrontRenderers.Contains(lv))
			{
				lvFrontRenderers.Add(lv);
			}
		}

		public static void RemoveLiquidFromFrontRenderers(LiquidVolume lv)
		{
			if (!(lv == null) && lvFrontRenderers.Contains(lv))
			{
				lvFrontRenderers.Remove(lv);
			}
		}

		private void OnDestroy()
		{
			Shader.SetGlobalFloat(ShaderParams.ForcedInvisible, 0f);
			CoreUtils.Destroy(mat);
			if (backPass != null)
			{
				backPass.CleanUp();
			}
			if (frontPass != null)
			{
				frontPass.CleanUp();
			}
		}

		public override void Create()
		{
			base.name = "Liquid Volume Depth PrePass";
			shader = Shader.Find("LiquidVolume/DepthPrePass");
			if (!(shader == null))
			{
				mat = CoreUtils.CreateEngineMaterial(shader);
				backPass = new DepthPass(mat, Pass.BackBuffer, renderPassEvent);
				frontPass = new DepthPass(mat, Pass.FrontBuffer, renderPassEvent);
			}
		}

		public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
		{
			installed = true;
			if (backPass != null && lvBackRenderers.Count > 0)
			{
				backPass.Setup(this, renderer);
				renderer.EnqueuePass(backPass);
			}
			if (frontPass != null && lvFrontRenderers.Count > 0)
			{
				frontPass.Setup(this, renderer);
				frontPass.renderer = renderer;
				renderer.EnqueuePass(frontPass);
			}
		}
	}
}
