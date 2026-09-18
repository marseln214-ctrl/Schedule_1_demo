using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ardenfall.Utilities
{
	[Serializable]
	public class MaterialOverrides
	{
		[Serializable]
		public class TextureProperty
		{
			public string propertyName;

			public Texture2D propertyValue;
		}

		[Serializable]
		public class FloatProperty
		{
			public string propertyName;

			public float propertyValue;
		}

		[Serializable]
		public class IntProperty
		{
			public string propertyName;

			public int propertyValue;
		}

		[Serializable]
		public class VectorProperty
		{
			public string propertyName;

			public Vector4 propertyValue;
		}

		[Serializable]
		public class ColorProperty
		{
			public string propertyName;

			public Color propertyValue;
		}

		public List<TextureProperty> textureOverrides;

		public List<FloatProperty> floatOverrides;

		public List<IntProperty> intOverrides;

		public List<VectorProperty> vectorOverrides;

		public List<ColorProperty> colorOverrides;

		public void OverrideMaterial(Material material)
		{
			foreach (TextureProperty textureOverride in textureOverrides)
			{
				material.SetTexture(textureOverride.propertyName, textureOverride.propertyValue);
			}
			foreach (FloatProperty floatOverride in floatOverrides)
			{
				material.SetFloat(floatOverride.propertyName, floatOverride.propertyValue);
			}
			foreach (IntProperty intOverride in intOverrides)
			{
				material.SetInt(intOverride.propertyName, intOverride.propertyValue);
			}
			foreach (VectorProperty vectorOverride in vectorOverrides)
			{
				material.SetVector(vectorOverride.propertyName, vectorOverride.propertyValue);
			}
			foreach (ColorProperty colorOverride in colorOverrides)
			{
				material.SetColor(colorOverride.propertyName, colorOverride.propertyValue);
			}
		}
	}
}
