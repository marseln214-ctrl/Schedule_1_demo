using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace EPOOutline
{
	public static class CameraUtility
	{
		public static int GetMSAA(Camera camera)
		{
			if (camera.targetTexture != null)
			{
				return camera.targetTexture.antiAliasing;
			}
			int result = Mathf.Max(GetRenderPipelineMSAA(), 1);
			if (!camera.allowMSAA)
			{
				result = 1;
			}
			if (camera.actualRenderingPath != RenderingPath.Forward && camera.actualRenderingPath != 0)
			{
				result = 1;
			}
			return result;
		}

		private static int GetRenderPipelineMSAA()
		{
			if (PipelineFetcher.CurrentAsset is UniversalRenderPipelineAsset)
			{
				return (PipelineFetcher.CurrentAsset as UniversalRenderPipelineAsset).msaaSampleCount;
			}
			return QualitySettings.antiAliasing;
		}
	}
}
