using System;
using System.Collections.Generic;
using UnityEngine;

namespace AdvancedPeopleSystem
{
	[Serializable]
	public class ClothesAnchor
	{
		public CharacterElementType partType;

		public List<SkinnedMeshRenderer> skinnedMesh;

		public ClothesAnchor()
		{
			skinnedMesh = new List<SkinnedMeshRenderer>();
		}
	}
}
