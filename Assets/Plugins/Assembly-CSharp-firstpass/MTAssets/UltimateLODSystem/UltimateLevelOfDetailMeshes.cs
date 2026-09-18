using UnityEngine;

namespace MTAssets.UltimateLODSystem
{
	[AddComponentMenu("")]
	public class UltimateLevelOfDetailMeshes : MonoBehaviour
	{
		[HideInInspector]
		public UltimateLevelOfDetail responsibleUlod;

		[HideInInspector]
		public int idOfOriginalMeshItemOfThisInResponsibleUlod = -1;

		public UltimateLevelOfDetail GetResponsibleUlodComponent()
		{
			return responsibleUlod;
		}

		public int GetQuantityOfLods()
		{
			return responsibleUlod.levelsOfDetailToGenerate;
		}

		public void SetMeshOfThisLodGroup(int level, Mesh newMesh)
		{
			if (level < 0 || level > 8)
			{
				Debug.LogError("It was not possible to define a new mesh in this LOD group, the level informed is invalid.");
				return;
			}
			responsibleUlod.currentScannedMeshesList[idOfOriginalMeshItemOfThisInResponsibleUlod].allMeshLods[level] = newMesh;
			responsibleUlod.ForceThisComponentToUpdateLodsRender();
		}

		public Mesh GetMeshOfThisLodGroup(int level)
		{
			if (level < 0 || level > 8)
			{
				Debug.LogError("It was not possible to get mesh of desired level, the level informed is invalid.");
				return null;
			}
			return responsibleUlod.currentScannedMeshesList[idOfOriginalMeshItemOfThisInResponsibleUlod].allMeshLods[level];
		}

		public bool isMaterialChangesEnabledForThisMesh()
		{
			if (responsibleUlod.enableMaterialsChanges)
			{
				return true;
			}
			_ = responsibleUlod.enableMaterialsChanges;
			return false;
		}

		public void SetMaterialArrayOfThisLodGroup(int level, Material[] newMaterialArray)
		{
			if (!isMaterialChangesEnabledForThisMesh())
			{
				Debug.LogError("It is not possible to supply or obtain a material array for an LOD of this mesh. Material change is disabled for this mesh and the Ultimate Level Of Detail component that manages it.");
				return;
			}
			if (level < 0 || level > 8)
			{
				Debug.LogError("It was not possible to define a new material array in this LOD group, the level informed is invalid.");
				return;
			}
			responsibleUlod.currentScannedMeshesList[idOfOriginalMeshItemOfThisInResponsibleUlod].allMeshLodsMaterials[level].materialArray = newMaterialArray;
			responsibleUlod.ForceThisComponentToUpdateLodsRender();
		}

		public Material[] GetMaterialArrayOfThisLodGroup(int level)
		{
			if (!isMaterialChangesEnabledForThisMesh())
			{
				Debug.LogError("It is not possible to supply or obtain a material array for an LOD of this mesh. Material change is disabled for this mesh and the Ultimate Level Of Detail component that manages it.");
				return null;
			}
			if (level < 0 || level > 8)
			{
				Debug.LogError("It was not possible to get mesh of desired level, the level informed is invalid.");
				return null;
			}
			return responsibleUlod.currentScannedMeshesList[idOfOriginalMeshItemOfThisInResponsibleUlod].allMeshLodsMaterials[level].materialArray;
		}
	}
}
