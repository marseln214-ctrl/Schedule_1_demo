using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ardenfall.Utilities
{
	[CreateAssetMenu(menuName = "Ardenfall/Foliage/Billboard Render Settings")]
	public class BillboardRenderSettings : ScriptableObject
	{
		[Serializable]
		public class BillboardTexture
		{
			public string textureId = "_MainTex";

			public bool powerOfTwo = true;

			public bool alphaIsTransparency = true;

			public List<BakePass> bakePasses;

			public TextureFormat GetFormat()
			{
				Vector4 vector = default(Vector4);
				foreach (BakePass bakePass in bakePasses)
				{
					if (bakePass.r)
					{
						vector.x += 1f;
					}
					if (bakePass.g)
					{
						vector.y += 1f;
					}
					if (bakePass.b)
					{
						vector.z += 1f;
					}
					if (bakePass.a)
					{
						vector.w += 1f;
					}
				}
				if (vector.x > 1f || vector.y > 1f || vector.z > 1f || vector.w > 1f)
				{
					Debug.LogError("Multiple bake passes in the same texture channel detected");
				}
				if (vector.w >= 1f)
				{
					return TextureFormat.RGBA32;
				}
				if (vector.z >= 1f)
				{
					return TextureFormat.RGB24;
				}
				if (vector.y >= 1f)
				{
					return TextureFormat.RG16;
				}
				return TextureFormat.R8;
			}
		}

		[Serializable]
		public class BakePass
		{
			public Shader customShader;

			public MaterialOverrides materialOverrides;

			public bool r = true;

			public bool g = true;

			public bool b = true;

			public bool a = true;
		}

		public List<BillboardTexture> billboardTextures;

		public Shader billboardShader;
	}
}
