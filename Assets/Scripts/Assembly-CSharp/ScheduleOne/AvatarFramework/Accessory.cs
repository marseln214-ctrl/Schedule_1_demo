using UnityEngine;

namespace ScheduleOne.AvatarFramework
{
	public class Accessory : MonoBehaviour
	{
		[Header("Settings")]
		public string Name;

		public string AssetPath;

		public bool ReduceFootSize;

		[Range(0f, 1f)]
		public float FootSizeReduction = 1f;

		public bool ShouldBlockHair;

		public bool ColorAllMeshes = true;

		[Header("References")]
		public MeshRenderer[] meshesToColor;

		public SkinnedMeshRenderer[] skinnedMeshesToColor;

		public SkinnedMeshRenderer[] skinnedMeshesToBind;

		public SkinnedMeshRenderer[] shapeKeyMeshRends;

		private void Awake()
		{
			for (int i = 0; i < skinnedMeshesToBind.Length; i++)
			{
				skinnedMeshesToBind[i].updateWhenOffscreen = true;
			}
		}

		public void ApplyColor(Color col)
		{
			MeshRenderer[] array = meshesToColor;
			foreach (MeshRenderer meshRenderer in array)
			{
				for (int j = 0; j < meshRenderer.materials.Length; j++)
				{
					meshRenderer.materials[j].color = col;
					if (!ColorAllMeshes)
					{
						break;
					}
				}
			}
			SkinnedMeshRenderer[] array2 = skinnedMeshesToColor;
			foreach (SkinnedMeshRenderer skinnedMeshRenderer in array2)
			{
				for (int k = 0; k < skinnedMeshRenderer.materials.Length; k++)
				{
					skinnedMeshRenderer.materials[k].color = col;
					if (!ColorAllMeshes)
					{
						break;
					}
				}
			}
		}

		public void ApplyShapeKeys(float gender, float weight)
		{
			SkinnedMeshRenderer[] array = shapeKeyMeshRends;
			foreach (SkinnedMeshRenderer skinnedMeshRenderer in array)
			{
				if (skinnedMeshRenderer.sharedMesh.blendShapeCount >= 2)
				{
					skinnedMeshRenderer.SetBlendShapeWeight(0, gender);
					skinnedMeshRenderer.SetBlendShapeWeight(1, weight);
				}
			}
		}

		public void BindBones(Transform[] bones)
		{
			SkinnedMeshRenderer[] array = skinnedMeshesToBind;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].bones = bones;
			}
		}
	}
}
