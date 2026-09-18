using System;
using System.Collections.Generic;
using UnityEngine;

namespace FluffyUnderware.Curvy.Generator
{
	[Serializable]
	public class CGMeshResourceCollection : ICGResourceCollection
	{
		public List<CGMeshResource> Items = new List<CGMeshResource>();

		public int Count => Items.Count;

		public Component[] ItemsArray => Items.ToArray();
	}
}
