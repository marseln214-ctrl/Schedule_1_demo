using System.Collections.Generic;
using System.Reflection;
using EPOOutline;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class URPOutlineFeature : ScriptableRendererFeature
{
	private class SRPOutline : ScriptableRenderPass
	{
		private static List<Outlinable> temporaryOutlinables = new List<Outlinable>();

		public ScriptableRenderer Renderer;

		public bool UseColorTargetForDepth;

		public Outliner Outliner;

		public OutlineParameters Parameters = new OutlineParameters();

		private List<Outliner> outliners = new List<Outliner>();

		private FieldInfo nameId = typeof(RenderTargetIdentifier).GetField("m_NameID", BindingFlags.Instance | BindingFlags.NonPublic);

		public SRPOutline()
		{
			Parameters.CheckInitialization();
		}

		private bool IsDepthTextureAvailable(ScriptableRenderer renderer)
		{
			return renderer.cameraDepthTargetHandle.rt != null;
		}

		private RenderTargetIdentifier GetDepthTarget(ScriptableRenderer renderer)
		{
			return Renderer.cameraDepthTargetHandle;
		}

		private RenderTargetIdentifier GetColorTarget(ScriptableRenderer renderer)
		{
			return renderer.cameraColorTargetHandle;
		}

		public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
		{
			Outliner outliner = Outliner;
			if (outliner == null || !outliner.enabled)
			{
				return;
			}
			Outlinable.GetAllActiveOutlinables(renderingData.cameraData.camera, Parameters.OutlinablesToRender);
			Outliner.UpdateSharedParameters(Parameters, renderingData.cameraData.camera, renderingData.cameraData.isSceneViewCamera);
			RendererFilteringUtility.Filter(renderingData.cameraData.camera, Parameters);
			Parameters.TargetWidth = renderingData.cameraData.cameraTargetDescriptor.width;
			Parameters.TargetHeight = renderingData.cameraData.cameraTargetDescriptor.height;
			Parameters.Antialiasing = renderingData.cameraData.cameraTargetDescriptor.msaaSamples;
			Parameters.Target = RenderTargetUtility.ComposeTarget(Parameters, Renderer.cameraColorTarget);
			Parameters.DepthTarget = RenderTargetUtility.ComposeTarget(Parameters, (!IsDepthTextureAvailable(Renderer)) ? GetColorTarget(Renderer) : GetDepthTarget(Renderer));
			Parameters.Buffer.Clear();
			if (Outliner.RenderingStrategy == OutlineRenderingStrategy.Default)
			{
				OutlineEffect.SetupOutline(Parameters);
				Parameters.BlitMesh = null;
				Parameters.MeshPool.ReleaseAllMeshes();
			}
			else
			{
				temporaryOutlinables.Clear();
				temporaryOutlinables.AddRange(Parameters.OutlinablesToRender);
				Parameters.OutlinablesToRender.Clear();
				Parameters.OutlinablesToRender.Add(null);
				foreach (Outlinable temporaryOutlinable in temporaryOutlinables)
				{
					Parameters.OutlinablesToRender[0] = temporaryOutlinable;
					OutlineEffect.SetupOutline(Parameters);
					Parameters.BlitMesh = null;
				}
				Parameters.MeshPool.ReleaseAllMeshes();
			}
			context.ExecuteCommandBuffer(Parameters.Buffer);
		}
	}

	private class Pool
	{
		private Stack<SRPOutline> outlines = new Stack<SRPOutline>();

		private List<SRPOutline> createdOutlines = new List<SRPOutline>();

		public SRPOutline Get()
		{
			if (outlines.Count == 0)
			{
				outlines.Push(new SRPOutline());
				createdOutlines.Add(outlines.Peek());
			}
			return outlines.Pop();
		}

		public void ReleaseAll()
		{
			outlines.Clear();
			foreach (SRPOutline createdOutline in createdOutlines)
			{
				outlines.Push(createdOutline);
			}
		}
	}

	private GameObject lastSelectedCamera;

	private Pool outlinePool = new Pool();

	private List<Outliner> outliners = new List<Outliner>();

	private bool GetOutlinersToRenderWith(RenderingData renderingData, List<Outliner> outliners)
	{
		outliners.Clear();
		GameObject gameObject = renderingData.cameraData.camera.gameObject;
		gameObject.GetComponents(outliners);
		if (outliners.Count == 0)
		{
			return false;
		}
		bool num = outliners.Count > 0;
		if (num)
		{
			lastSelectedCamera = gameObject;
		}
		return num;
	}

	public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
	{
		if (!GetOutlinersToRenderWith(renderingData, outliners))
		{
			return;
		}
		UniversalAdditionalCameraData universalAdditionalCameraData = renderingData.cameraData.camera.GetUniversalAdditionalCameraData();
		int num = 0;
		if (universalAdditionalCameraData != null)
		{
			List<Camera> list = ((universalAdditionalCameraData.renderType == CameraRenderType.Overlay) ? null : universalAdditionalCameraData.cameraStack);
			if (list != null)
			{
				foreach (Camera item in list)
				{
					if (item != null && item.isActiveAndEnabled)
					{
						num++;
					}
				}
			}
		}
		foreach (Outliner outliner in outliners)
		{
			SRPOutline sRPOutline = outlinePool.Get();
			sRPOutline.Outliner = outliner;
			sRPOutline.Renderer = renderer;
			sRPOutline.renderPassEvent = ((outliner.RenderStage == RenderStage.AfterTransparents) ? RenderPassEvent.AfterRenderingTransparents : RenderPassEvent.AfterRenderingOpaques);
			renderer.EnqueuePass(sRPOutline);
		}
		outlinePool.ReleaseAll();
	}

	public override void Create()
	{
	}
}
