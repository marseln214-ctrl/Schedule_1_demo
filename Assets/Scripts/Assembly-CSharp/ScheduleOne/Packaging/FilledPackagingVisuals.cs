using System;
using UnityEngine;

namespace ScheduleOne.Packaging
{
	public class FilledPackagingVisuals : MonoBehaviour
	{
		[Serializable]
		public class MeshIndexPair
		{
			public MeshRenderer Mesh;

			public int MaterialIndex;
		}

		[Serializable]
		public class BaseVisuals
		{
			public Transform Container;
		}

		[Serializable]
		public class WeedVisuals : BaseVisuals
		{
			public MeshIndexPair[] MainMeshes;

			public MeshIndexPair[] SecondaryMeshes;

			public MeshIndexPair[] LeafMeshes;

			public MeshIndexPair[] StemMeshes;
		}

		[Serializable]
		public class MethVisuals : BaseVisuals
		{
			public MeshRenderer[] CrystalMeshes;
		}

		[Serializable]
		public class CocaineVisuals : BaseVisuals
		{
			public MeshRenderer[] RockMeshes;
		}

		public WeedVisuals weedVisuals;

		public MethVisuals methVisuals;

		public CocaineVisuals cocaineVisuals;

		public void ResetVisuals()
		{
			if (weedVisuals.Container != null)
			{
				weedVisuals.Container.gameObject.SetActive(value: false);
			}
			if (methVisuals.Container != null)
			{
				methVisuals.Container.gameObject.SetActive(value: false);
			}
			if (cocaineVisuals.Container != null)
			{
				cocaineVisuals.Container.gameObject.SetActive(value: false);
			}
		}
	}
}
