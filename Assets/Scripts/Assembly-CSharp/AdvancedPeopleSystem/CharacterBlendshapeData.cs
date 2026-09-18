using System;
using UnityEngine;

namespace AdvancedPeopleSystem
{
	[Serializable]
	public class CharacterBlendshapeData
	{
		public string blendshapeName;

		public CharacterBlendShapeType type;

		public CharacterBlendShapeGroup group;

		[HideInInspector]
		public float value;

		public CharacterBlendshapeData(string name, CharacterBlendShapeType t, CharacterBlendShapeGroup g, float value = 0f)
		{
			blendshapeName = name;
			type = t;
			group = g;
			this.value = value;
		}

		public CharacterBlendshapeData()
		{
		}
	}
}
