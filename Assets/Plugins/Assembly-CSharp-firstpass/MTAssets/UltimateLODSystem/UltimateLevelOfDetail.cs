using System;
using System.Collections;
using System.Collections.Generic;
using MTAssets.UltimateLODSystem.MeshSimplifier;
using UnityEngine;
using UnityEngine.Events;

namespace MTAssets.UltimateLODSystem
{
	[AddComponentMenu("MT Assets/Ultimate LOD System/Ultimate Level Of Detail")]
	[ExecuteInEditMode]
	public class UltimateLevelOfDetail : MonoBehaviour
	{
		public enum ScanMeshesMode
		{
			ScanInChildrenGameObjectsOnly = 0,
			ScanInThisGameObjectOnly = 1
		}

		public enum ForceOfSimplification
		{
			Normal = 0,
			Strong = 1,
			VeryStrong = 2,
			ExtremelyStrong = 3,
			Destroyer = 4
		}

		public enum CullingMode
		{
			Disabled = 0,
			CullingMeshes = 1,
			CullingRenderer = 2
		}

		public enum CameraDetectionMode
		{
			CurrentCamera = 0,
			MainCamera = 1,
			CustomCamera = 2
		}

		[Serializable]
		public class ScannedMeshItem
		{
			[Serializable]
			public class MeshMaterials
			{
				public Material[] materialArray = new Material[0];
			}

			public GameObject originalGameObject;

			public SkinnedMeshRenderer originalSkinnedMeshRenderer;

			public MeshFilter originalMeshFilter;

			public MeshRenderer originalMeshRenderer;

			public Mesh[] allMeshLods = new Mesh[9];

			public string[] allMeshLodsPaths = new string[9];

			public bool canChangeMaterialsOnThisMeshLods;

			public MeshMaterials[] allMeshLodsMaterials = new MeshMaterials[9];

			public UltimateLevelOfDetailMeshes originalMeshLodsManager;

			public Mesh beforeCullingData_lastMeshOfThis;

			public bool beforeCullingData_isForcedToRenderizationOff;

			public void InitializeAllMeshLodsMaterialsArray()
			{
				for (int i = 0; i < 9; i++)
				{
					if (allMeshLodsMaterials[i] != null)
					{
						return;
					}
				}
				for (int j = 0; j < 9; j++)
				{
					allMeshLodsMaterials[j] = new MeshMaterials();
				}
			}
		}

		private WaitForEndOfFrame WAIT_FOR_END_OF_FRAME = new WaitForEndOfFrame();

		private Camera cacheOfMainCamera;

		private GameObject cacheOfUlodData;

		private RuntimeInstancesDetector cacheOfUlodDataRuntimeInstancesDetector;

		private float lastDistanceFromMainCamera = -1f;

		private int currentLodAccordingToDistance = -1;

		private float currentDistanceFromMainCamera;

		private float currentRealDistanceFromMainCamera;

		private bool forcedToDisableLodsOfThisComponent;

		private int forcedToShowOnlyALodIndependentOfDistance = -1;

		[HideInInspector]
		public List<ScannedMeshItem> currentScannedMeshesList = new List<ScannedMeshItem>();

		[HideInInspector]
		public ScanMeshesMode modeOfMeshesScanning;

		[HideInInspector]
		public bool scanInactiveGameObjects;

		[HideInInspector]
		public List<GameObject> gameObjectsToIgnore = new List<GameObject>();

		[HideInInspector]
		public int levelsOfDetailToGenerate = 3;

		[HideInInspector]
		public float[] percentOfVerticesForEachLod = new float[9] { 100f, 75f, 65f, 45f, 35f, 25f, 15f, 10f, 5f };

		[HideInInspector]
		public bool saveGeneratedLodsInAssets = true;

		[HideInInspector]
		public bool skinnedAnimsCompatibilityMode = true;

		[HideInInspector]
		public bool preventArtifacts = true;

		[HideInInspector]
		public bool optimizeResultingMeshes;

		[HideInInspector]
		public bool enableLightmapsSupport;

		[HideInInspector]
		public bool enableMaterialsChanges;

		[HideInInspector]
		public ForceOfSimplification forceOfSimplification;

		[HideInInspector]
		public CullingMode cullingMode = CullingMode.CullingMeshes;

		[HideInInspector]
		[SerializeField]
		private Transform _customPivotToSimulateLods;

		[HideInInspector]
		public CameraDetectionMode cameraDetectionMode;

		[HideInInspector]
		public bool useCacheForMainCameraInDetection = true;

		[HideInInspector]
		public Camera customCameraForSimulationOfLods;

		[HideInInspector]
		public float[] minDistanceOfViewForEachLod = new float[9] { 0f, 30f, 70f, 120f, 150f, 180f, 200f, 220f, 250f };

		[HideInInspector]
		public float minDistanceOfViewForCull = 270f;

		public UnityEvent onDoneScan;

		public UnityEvent onUndoScan;

		[HideInInspector]
		public bool forceChangeLodsOfSkinnedInEditor;

		[HideInInspector]
		public bool drawGizmoOnThisPivot;

		[HideInInspector]
		public Color colorOfGizmo = Color.blue;

		[HideInInspector]
		public float sizeOfGizmo = 0.2f;

		[HideInInspector]
		public bool forceShowHiddenSettings;

		public Transform customPivotToSimulateLods
		{
			get
			{
				return _customPivotToSimulateLods;
			}
			set
			{
				if (value == null)
				{
					_customPivotToSimulateLods = null;
				}
				else if (value.IsChildOf(base.gameObject.transform))
				{
					_customPivotToSimulateLods = value;
				}
				else
				{
					Debug.LogError("We were unable to define a custom pivot. Make sure that the GameObject that will be the new personalized pivot is the child of the desired ULOD component.");
				}
			}
		}

		private void ValidateAllParameters(bool isGoingToScan)
		{
			float num = 0f;
			for (int i = 0; i <= levelsOfDetailToGenerate; i++)
			{
				num = minDistanceOfViewForEachLod[i];
			}
			if (forceOfSimplification != 0)
			{
				preventArtifacts = false;
			}
			float[] array = new float[9] { 0f, 1f, 5f, 10f, 15f, 20f, 25f, 30f, 35f };
			for (int j = 0; j < 9; j++)
			{
				if (minDistanceOfViewForEachLod[j] < array[j])
				{
					minDistanceOfViewForEachLod[j] = array[j];
				}
			}
			if (minDistanceOfViewForCull <= num)
			{
				minDistanceOfViewForCull = num + 10f;
			}
			if (!isGoingToScan)
			{
				return;
			}
			for (int k = 0; k < 9; k++)
			{
				if (k != 0 && levelsOfDetailToGenerate >= k && percentOfVerticesForEachLod[k] > percentOfVerticesForEachLod[k - 1])
				{
					percentOfVerticesForEachLod[k] = percentOfVerticesForEachLod[k - 1] - 1f;
					if (percentOfVerticesForEachLod[k] <= 0f)
					{
						percentOfVerticesForEachLod[k] = percentOfVerticesForEachLod[k - 1];
					}
				}
			}
			for (int num2 = 8; num2 >= 0; num2--)
			{
				if (num2 != 0 && levelsOfDetailToGenerate >= num2 && minDistanceOfViewForEachLod[num2 - 1] >= minDistanceOfViewForEachLod[num2])
				{
					minDistanceOfViewForEachLod[num2 - 1] = minDistanceOfViewForEachLod[num2] - 1f;
					if (minDistanceOfViewForEachLod[num2 - 1] <= 0f)
					{
						minDistanceOfViewForEachLod[num2 - 1] = 1f;
					}
				}
			}
			if (minDistanceOfViewForCull <= num)
			{
				minDistanceOfViewForCull = num + 10f;
			}
		}

		private void CreateHierarchyOfFoldersIfNotExists()
		{
		}

		private string SaveGeneratedLodInAssets(string lodNumber, long ticks, Mesh generatedLodMesh)
		{
			return "";
		}

		private Mesh GetGeneratedLodForThisMesh(Mesh originalMesh, float percentOfVertices, bool isSkinnedMesh)
		{
			float num = 1E-05f;
			MTAssets.UltimateLODSystem.MeshSimplifier.MeshSimplifier meshSimplifier = new MTAssets.UltimateLODSystem.MeshSimplifier.MeshSimplifier();
			SimplificationOptions simplificationOptions = default(SimplificationOptions);
			switch (forceOfSimplification)
			{
			case ForceOfSimplification.Normal:
				simplificationOptions.Agressiveness = 7.0;
				num = 1f;
				break;
			case ForceOfSimplification.Strong:
				simplificationOptions.Agressiveness = 8.5;
				num = 0.8f;
				break;
			case ForceOfSimplification.VeryStrong:
				simplificationOptions.Agressiveness = 10.0;
				num = 0.6f;
				break;
			case ForceOfSimplification.ExtremelyStrong:
				simplificationOptions.Agressiveness = 12.0;
				num = 0.4f;
				break;
			case ForceOfSimplification.Destroyer:
				simplificationOptions.Agressiveness = 14.0;
				num = 0.2f;
				break;
			}
			if (preventArtifacts)
			{
				simplificationOptions.EnableSmartLink = true;
			}
			if (!preventArtifacts)
			{
				simplificationOptions.EnableSmartLink = false;
			}
			simplificationOptions.MaxIterationCount = 100;
			simplificationOptions.PreserveBorderEdges = false;
			simplificationOptions.PreserveSurfaceCurvature = false;
			simplificationOptions.PreserveUVFoldoverEdges = false;
			simplificationOptions.PreserveUVSeamEdges = false;
			simplificationOptions.VertexLinkDistance = double.Epsilon;
			meshSimplifier.SimplificationOptions = simplificationOptions;
			meshSimplifier.Initialize(originalMesh);
			meshSimplifier.SimplifyMesh(percentOfVertices / 100f * num);
			Mesh mesh = meshSimplifier.ToMesh();
			if (optimizeResultingMeshes)
			{
				mesh.Optimize();
			}
			if (isSkinnedMesh && skinnedAnimsCompatibilityMode)
			{
				mesh.bindposes = originalMesh.bindposes;
			}
			return mesh;
		}

		private Material[] GetCopyOfExistentArrayOfMaterials(Material[] sourceArray)
		{
			Material[] array = new Material[sourceArray.Length];
			for (int i = 0; i < sourceArray.Length; i++)
			{
				array[i] = sourceArray[i];
			}
			return array;
		}

		private void ScanForMeshesAndGenerateAllLodGroups_StartProcessing(bool showProgressBar)
		{
			StartCoroutine(ScanForMeshesAndGenerateAllLodGroups_AsyncProcessing(showProgressBar));
		}

		private IEnumerator ScanForMeshesAndGenerateAllLodGroups_AsyncProcessing(bool showProgressBar)
		{
			ValidateAllParameters(isGoingToScan: true);
			List<SkinnedMeshRenderer> list = new List<SkinnedMeshRenderer>();
			List<MeshFilter> meshFiltersFound = new List<MeshFilter>();
			bool flag = true;
			if (modeOfMeshesScanning == ScanMeshesMode.ScanInChildrenGameObjectsOnly)
			{
				SkinnedMeshRenderer[] componentsInChildren = GetComponentsInChildren<SkinnedMeshRenderer>(scanInactiveGameObjects);
				foreach (SkinnedMeshRenderer skinnedMeshRenderer in componentsInChildren)
				{
					if (skinnedMeshRenderer != null && !gameObjectsToIgnore.Contains(skinnedMeshRenderer.gameObject) && skinnedMeshRenderer.gameObject.GetComponent<UltimateLevelOfDetail>() == null && skinnedMeshRenderer.sharedMesh != null)
					{
						list.Add(skinnedMeshRenderer);
					}
				}
				MeshFilter[] componentsInChildren2 = GetComponentsInChildren<MeshFilter>(scanInactiveGameObjects);
				foreach (MeshFilter meshFilter in componentsInChildren2)
				{
					if (meshFilter != null && !gameObjectsToIgnore.Contains(meshFilter.gameObject) && meshFilter.gameObject.GetComponent<UltimateLevelOfDetail>() == null && meshFilter.sharedMesh != null)
					{
						meshFiltersFound.Add(meshFilter);
					}
				}
			}
			if (modeOfMeshesScanning == ScanMeshesMode.ScanInThisGameObjectOnly)
			{
				SkinnedMeshRenderer component = GetComponent<SkinnedMeshRenderer>();
				if (component != null)
				{
					list.Add(component);
				}
				MeshFilter component2 = GetComponent<MeshFilter>();
				if (component2 != null)
				{
					meshFiltersFound.Add(component2);
				}
			}
			if (list.Count == 0 && meshFiltersFound.Count == 0)
			{
				Debug.Log("No mesh was found in GameObject \"" + base.gameObject.name + "\" or its child GameObjects. Please check the GameObjects hierarchy and the settings for this component and try again.");
				flag = false;
			}
			if (!flag)
			{
				yield break;
			}
			List<ScannedMeshItem> scannedMeshItems = new List<ScannedMeshItem>();
			CreateHierarchyOfFoldersIfNotExists();
			_ = list.Count;
			_ = meshFiltersFound.Count;
			_ = list.Count;
			GetNumberOfLodsGenerated();
			_ = meshFiltersFound.Count;
			GetNumberOfLodsGenerated();
			float currentMesh = 0f;
			float currentLod = 0f;
			_ = enableLightmapsSupport;
			foreach (SkinnedMeshRenderer smr in list)
			{
				long ticks2 = DateTime.UtcNow.Ticks + (long)currentMesh;
				currentMesh += 1f;
				ScannedMeshItem thisScannedMeshItem2 = new ScannedMeshItem
				{
					originalGameObject = smr.gameObject,
					originalSkinnedMeshRenderer = smr
				};
				thisScannedMeshItem2.allMeshLods[0] = smr.sharedMesh;
				thisScannedMeshItem2.InitializeAllMeshLodsMaterialsArray();
				thisScannedMeshItem2.canChangeMaterialsOnThisMeshLods = enableMaterialsChanges;
				thisScannedMeshItem2.allMeshLodsMaterials[0].materialArray = GetCopyOfExistentArrayOfMaterials(smr.sharedMaterials);
				thisScannedMeshItem2.originalMeshLodsManager = thisScannedMeshItem2.originalGameObject.AddComponent<UltimateLevelOfDetailMeshes>();
				thisScannedMeshItem2.originalMeshLodsManager.responsibleUlod = this;
				thisScannedMeshItem2.originalMeshLodsManager.idOfOriginalMeshItemOfThisInResponsibleUlod = (int)currentMesh - 1;
				for (int j = 1; j <= levelsOfDetailToGenerate; j++)
				{
					if (levelsOfDetailToGenerate >= j)
					{
						if (Application.isPlaying)
						{
							yield return WAIT_FOR_END_OF_FRAME;
							yield return WAIT_FOR_END_OF_FRAME;
						}
						thisScannedMeshItem2.allMeshLods[j] = GetGeneratedLodForThisMesh(smr.sharedMesh, percentOfVerticesForEachLod[j], isSkinnedMesh: true);
						thisScannedMeshItem2.allMeshLodsPaths[j] = SaveGeneratedLodInAssets(j.ToString(), ticks2, thisScannedMeshItem2.allMeshLods[j]);
						thisScannedMeshItem2.allMeshLodsMaterials[j].materialArray = GetCopyOfExistentArrayOfMaterials(smr.sharedMaterials);
						currentLod += 1f;
					}
				}
				scannedMeshItems.Add(thisScannedMeshItem2);
			}
			foreach (MeshFilter mf in meshFiltersFound)
			{
				long ticks2 = DateTime.UtcNow.Ticks + (long)currentMesh;
				currentMesh += 1f;
				ScannedMeshItem thisScannedMeshItem2 = new ScannedMeshItem
				{
					originalGameObject = mf.gameObject,
					originalMeshFilter = mf,
					originalMeshRenderer = mf.GetComponent<MeshRenderer>()
				};
				thisScannedMeshItem2.allMeshLods[0] = mf.sharedMesh;
				thisScannedMeshItem2.InitializeAllMeshLodsMaterialsArray();
				thisScannedMeshItem2.canChangeMaterialsOnThisMeshLods = enableMaterialsChanges;
				thisScannedMeshItem2.allMeshLodsMaterials[0].materialArray = GetCopyOfExistentArrayOfMaterials(thisScannedMeshItem2.originalMeshRenderer.sharedMaterials);
				thisScannedMeshItem2.originalMeshLodsManager = thisScannedMeshItem2.originalGameObject.AddComponent<UltimateLevelOfDetailMeshes>();
				thisScannedMeshItem2.originalMeshLodsManager.responsibleUlod = this;
				thisScannedMeshItem2.originalMeshLodsManager.idOfOriginalMeshItemOfThisInResponsibleUlod = (int)currentMesh - 1;
				for (int j = 1; j <= levelsOfDetailToGenerate; j++)
				{
					if (levelsOfDetailToGenerate >= j)
					{
						if (Application.isPlaying)
						{
							yield return WAIT_FOR_END_OF_FRAME;
							yield return WAIT_FOR_END_OF_FRAME;
						}
						thisScannedMeshItem2.allMeshLods[j] = GetGeneratedLodForThisMesh(mf.sharedMesh, percentOfVerticesForEachLod[j], isSkinnedMesh: true);
						thisScannedMeshItem2.allMeshLodsPaths[j] = SaveGeneratedLodInAssets(j.ToString(), ticks2, thisScannedMeshItem2.allMeshLods[j]);
						thisScannedMeshItem2.allMeshLodsMaterials[j].materialArray = GetCopyOfExistentArrayOfMaterials(thisScannedMeshItem2.originalMeshRenderer.sharedMaterials);
						currentLod += 1f;
					}
				}
				scannedMeshItems.Add(thisScannedMeshItem2);
			}
			currentScannedMeshesList = scannedMeshItems;
			if (Application.isPlaying && onDoneScan != null)
			{
				onDoneScan.Invoke();
			}
			Debug.Log("All meshes present in GameObject \"" + base.gameObject.name + "\" or its child GameObjects have been scanned and have groups of LODs created automatically based on the parameters you have defined. Now just move the camera away and watch the magic of optimization happen!");
			if (Application.isPlaying)
			{
				OnEnable();
			}
			if (!Application.isPlaying)
			{
				yield return null;
			}
		}

		private void UndoAllMeshesScannedAndAllLodGroups(bool showProgressBar, bool deleteAllGeneratedMeshes, bool runMonoIl2CppGc, bool runUnityGc)
		{
			foreach (ScannedMeshItem currentScannedMeshes in currentScannedMeshesList)
			{
				if (saveGeneratedLodsInAssets)
				{
				}
				if (currentScannedMeshes.originalSkinnedMeshRenderer != null)
				{
					currentScannedMeshes.originalSkinnedMeshRenderer.sharedMesh = currentScannedMeshes.allMeshLods[0];
					if (currentScannedMeshes.canChangeMaterialsOnThisMeshLods)
					{
						currentScannedMeshes.originalSkinnedMeshRenderer.sharedMaterials = currentScannedMeshes.allMeshLodsMaterials[0].materialArray;
					}
				}
				if (currentScannedMeshes.originalMeshFilter != null && currentScannedMeshes.originalMeshRenderer != null)
				{
					currentScannedMeshes.originalMeshFilter.sharedMesh = currentScannedMeshes.allMeshLods[0];
					if (currentScannedMeshes.canChangeMaterialsOnThisMeshLods)
					{
						currentScannedMeshes.originalMeshRenderer.sharedMaterials = currentScannedMeshes.allMeshLodsMaterials[0].materialArray;
					}
				}
				if (currentScannedMeshes.originalMeshLodsManager != null)
				{
					UnityEngine.Object.Destroy(currentScannedMeshes.originalMeshLodsManager);
				}
			}
			currentScannedMeshesList.Clear();
			if (runMonoIl2CppGc)
			{
				GC.Collect();
			}
			if (runUnityGc)
			{
				Resources.UnloadUnusedAssets();
			}
			lastDistanceFromMainCamera = -1f;
			currentLodAccordingToDistance = -1;
			if (Application.isPlaying && onUndoScan != null)
			{
				onUndoScan.Invoke();
			}
			Debug.Log("All scanned meshes in GameObject \"" + base.gameObject.name + "\" were restored to the original meshes. The scan was undone.");
		}

		private bool isLodsSimulationEnabledInThisSceneForEditorSceneViewMode()
		{
			return true;
		}

		private Camera GetLastActiveSceneViewCamera()
		{
			return null;
		}

		private void CullThisLodMeshOfRenderer(ScannedMeshItem meshItem)
		{
			if (cullingMode == CullingMode.Disabled)
			{
				return;
			}
			if (cullingMode == CullingMode.CullingMeshes)
			{
				if (meshItem.originalSkinnedMeshRenderer != null && meshItem.originalSkinnedMeshRenderer.sharedMesh != null)
				{
					meshItem.beforeCullingData_lastMeshOfThis = meshItem.originalSkinnedMeshRenderer.sharedMesh;
					meshItem.originalSkinnedMeshRenderer.sharedMesh = null;
				}
				if (meshItem.originalMeshFilter != null && meshItem.originalMeshFilter.sharedMesh != null)
				{
					meshItem.beforeCullingData_lastMeshOfThis = meshItem.originalMeshFilter.sharedMesh;
					meshItem.originalMeshFilter.sharedMesh = null;
				}
			}
			if (cullingMode == CullingMode.CullingRenderer)
			{
				if (meshItem.originalSkinnedMeshRenderer != null)
				{
					meshItem.beforeCullingData_isForcedToRenderizationOff = meshItem.originalSkinnedMeshRenderer.forceRenderingOff;
					meshItem.originalSkinnedMeshRenderer.forceRenderingOff = true;
				}
				if (meshItem.originalMeshRenderer != null)
				{
					meshItem.beforeCullingData_isForcedToRenderizationOff = meshItem.originalMeshRenderer.forceRenderingOff;
					meshItem.originalMeshRenderer.forceRenderingOff = true;
				}
			}
		}

		private void UncullThisLodMeshOfRenderer(ScannedMeshItem meshItem)
		{
			if (cullingMode == CullingMode.Disabled)
			{
				return;
			}
			if (cullingMode == CullingMode.CullingMeshes)
			{
				if (meshItem.originalSkinnedMeshRenderer != null && meshItem.originalSkinnedMeshRenderer.sharedMesh == null)
				{
					meshItem.originalSkinnedMeshRenderer.sharedMesh = meshItem.beforeCullingData_lastMeshOfThis;
				}
				if (meshItem.originalMeshFilter != null && meshItem.originalMeshFilter.sharedMesh == null)
				{
					meshItem.originalMeshFilter.sharedMesh = meshItem.beforeCullingData_lastMeshOfThis;
				}
			}
			if (cullingMode == CullingMode.CullingRenderer)
			{
				if (meshItem.originalSkinnedMeshRenderer != null)
				{
					meshItem.originalSkinnedMeshRenderer.forceRenderingOff = meshItem.beforeCullingData_isForcedToRenderizationOff;
				}
				if (meshItem.originalMeshRenderer != null)
				{
					meshItem.originalMeshRenderer.forceRenderingOff = meshItem.beforeCullingData_isForcedToRenderizationOff;
				}
			}
		}

		private void ChangeLodMeshAndMaterialsOfRenderer(ScannedMeshItem meshItem, int lodLevel)
		{
			if (meshItem.originalSkinnedMeshRenderer != null)
			{
				bool flag = false;
				if (!Application.isEditor)
				{
					flag = true;
				}
				if (Application.isEditor && forceChangeLodsOfSkinnedInEditor)
				{
					flag = true;
				}
				if (flag)
				{
					for (int i = 0; i < 9; i++)
					{
						if (i == lodLevel)
						{
							meshItem.originalSkinnedMeshRenderer.sharedMesh = meshItem.allMeshLods[i];
							if (meshItem.canChangeMaterialsOnThisMeshLods)
							{
								meshItem.originalSkinnedMeshRenderer.sharedMaterials = meshItem.allMeshLodsMaterials[i].materialArray;
							}
						}
					}
					meshItem.originalSkinnedMeshRenderer.enabled = false;
					meshItem.originalSkinnedMeshRenderer.enabled = true;
				}
			}
			if (!(meshItem.originalMeshFilter != null) || !(meshItem.originalMeshRenderer != null))
			{
				return;
			}
			for (int j = 0; j < 9; j++)
			{
				if (j == lodLevel)
				{
					meshItem.originalMeshFilter.sharedMesh = meshItem.allMeshLods[j];
					if (meshItem.canChangeMaterialsOnThisMeshLods)
					{
						meshItem.originalMeshRenderer.sharedMaterials = meshItem.allMeshLodsMaterials[j].materialArray;
					}
				}
			}
		}

		private void CalculateCorrectLodForDistanceBeforeChange(float distance)
		{
			if (currentScannedMeshesList.Count == 0 || lastDistanceFromMainCamera == distance)
			{
				return;
			}
			int num = -1;
			if (distance < minDistanceOfViewForEachLod[1])
			{
				num = 0;
			}
			if (distance >= minDistanceOfViewForEachLod[1])
			{
				num = 1;
			}
			if (levelsOfDetailToGenerate >= 2 && distance >= minDistanceOfViewForEachLod[2])
			{
				num = 2;
			}
			if (levelsOfDetailToGenerate >= 3 && distance >= minDistanceOfViewForEachLod[3])
			{
				num = 3;
			}
			if (levelsOfDetailToGenerate >= 4 && distance >= minDistanceOfViewForEachLod[4])
			{
				num = 4;
			}
			if (levelsOfDetailToGenerate >= 5 && distance >= minDistanceOfViewForEachLod[5])
			{
				num = 5;
			}
			if (levelsOfDetailToGenerate >= 6 && distance >= minDistanceOfViewForEachLod[6])
			{
				num = 6;
			}
			if (levelsOfDetailToGenerate >= 7 && distance >= minDistanceOfViewForEachLod[7])
			{
				num = 7;
			}
			if (levelsOfDetailToGenerate >= 8 && distance >= minDistanceOfViewForEachLod[8])
			{
				num = 8;
			}
			if (cullingMode != 0 && distance >= minDistanceOfViewForCull)
			{
				num = 9;
			}
			if (num >= 0 && num < 9 && currentLodAccordingToDistance != num)
			{
				foreach (ScannedMeshItem currentScannedMeshes in currentScannedMeshesList)
				{
					UncullThisLodMeshOfRenderer(currentScannedMeshes);
					ChangeLodMeshAndMaterialsOfRenderer(currentScannedMeshes, num);
					currentLodAccordingToDistance = num;
				}
			}
			if (num == 9 && currentLodAccordingToDistance != 9)
			{
				foreach (ScannedMeshItem currentScannedMeshes2 in currentScannedMeshesList)
				{
					CullThisLodMeshOfRenderer(currentScannedMeshes2);
					currentLodAccordingToDistance = 9;
				}
			}
			lastDistanceFromMainCamera = distance;
		}

		public void OnRenderObject()
		{
			if (!UltimateLevelOfDetailGlobal.isGlobalULodSystemEnabled())
			{
				CalculateCorrectLodForDistanceBeforeChange(0f);
				return;
			}
			if (forcedToDisableLodsOfThisComponent)
			{
				CalculateCorrectLodForDistanceBeforeChange(0f);
				return;
			}
			if (forcedToShowOnlyALodIndependentOfDistance != -1)
			{
				if (forcedToShowOnlyALodIndependentOfDistance == 0)
				{
					CalculateCorrectLodForDistanceBeforeChange(0f);
				}
				if (forcedToShowOnlyALodIndependentOfDistance > 0)
				{
					CalculateCorrectLodForDistanceBeforeChange(minDistanceOfViewForEachLod[forcedToShowOnlyALodIndependentOfDistance]);
				}
				return;
			}
			if (!isLodsSimulationEnabledInThisSceneForEditorSceneViewMode())
			{
				CalculateCorrectLodForDistanceBeforeChange(0f);
				return;
			}
			Camera lastActiveSceneViewCamera = GetLastActiveSceneViewCamera();
			Camera camera = null;
			if (cameraDetectionMode == CameraDetectionMode.MainCamera && Application.isPlaying)
			{
				if (!useCacheForMainCameraInDetection)
				{
					camera = Camera.main;
				}
				if (useCacheForMainCameraInDetection)
				{
					if (cacheOfMainCamera != null && (!cacheOfMainCamera.enabled || !cacheOfMainCamera.gameObject.activeSelf || !cacheOfMainCamera.gameObject.activeInHierarchy))
					{
						cacheOfMainCamera = null;
					}
					if (cacheOfMainCamera == null)
					{
						cacheOfMainCamera = Camera.main;
					}
					if (cacheOfMainCamera != null)
					{
						camera = cacheOfMainCamera;
					}
				}
				if (camera == null)
				{
					Debug.LogWarning("It was not possible to find a main camera to calculate LODs. Please make sure that the main camera in your scene has the \"MainCamera\" tag defined in the GameObject in which it is located.");
				}
			}
			if (cameraDetectionMode == CameraDetectionMode.CurrentCamera && Application.isPlaying)
			{
				camera = UltimateLevelOfDetailGlobal.currentCameraThatIsOnTopOfScreenInThisScene;
				if (camera == null)
				{
					Debug.LogWarning("It was not possible to find a current camera at the moment, it seems that there are no cameras in the scene, or Unity was unable to make references. Please try to switch to \"Main Camera\" mode.");
				}
			}
			if (cameraDetectionMode == CameraDetectionMode.CustomCamera && Application.isPlaying)
			{
				camera = customCameraForSimulationOfLods;
				if (camera == null)
				{
					Debug.LogWarning("No custom camera for calculating distance and simulating LODs has been provided in \"" + base.gameObject.name + "\".");
				}
			}
			if (Application.isEditor && !Application.isPlaying)
			{
				camera = lastActiveSceneViewCamera;
				if (camera == null)
				{
					Debug.LogWarning("It was not possible to find a camera that is currently viewing a scene. Make sure the scene view window is active and in focus.");
				}
			}
			if (camera != null)
			{
				if (_customPivotToSimulateLods == null)
				{
					currentDistanceFromMainCamera = Vector3.Distance(base.gameObject.transform.position, camera.transform.position) * UltimateLevelOfDetailGlobal.GetGlobalLodDistanceMultiplier();
					currentRealDistanceFromMainCamera = Vector3.Distance(base.gameObject.transform.position, camera.transform.position);
				}
				if (_customPivotToSimulateLods != null)
				{
					currentDistanceFromMainCamera = Vector3.Distance(_customPivotToSimulateLods.position, camera.transform.position) * UltimateLevelOfDetailGlobal.GetGlobalLodDistanceMultiplier();
					currentRealDistanceFromMainCamera = Vector3.Distance(_customPivotToSimulateLods.position, camera.transform.position);
				}
			}
			if (camera == null)
			{
				currentDistanceFromMainCamera = 0f;
				currentRealDistanceFromMainCamera = 0f;
			}
			CalculateCorrectLodForDistanceBeforeChange(currentDistanceFromMainCamera);
		}

		public void Awake()
		{
			CalculateCorrectLodForDistanceBeforeChange(0f);
			GameObject gameObject = GameObject.Find("Ultimate LOD Data");
			if (gameObject != null && Application.isPlaying)
			{
				gameObject.GetComponent<RuntimeInstancesDetector>().instancesOfUlodInThisScene.Add(this);
				cacheOfUlodData = gameObject;
				cacheOfUlodDataRuntimeInstancesDetector = gameObject.GetComponent<RuntimeInstancesDetector>();
			}
			if (gameObject == null && Application.isPlaying)
			{
				gameObject = new GameObject("Ultimate LOD Data");
				gameObject.transform.position = new Vector3(0f, 0f, 0f);
				gameObject.AddComponent<RuntimeCameraDetector>();
				gameObject.AddComponent<RuntimeInstancesDetector>().instancesOfUlodInThisScene.Add(this);
				cacheOfUlodData = gameObject;
				cacheOfUlodDataRuntimeInstancesDetector = gameObject.GetComponent<RuntimeInstancesDetector>();
			}
		}

		private IEnumerator OnRenderObject_HookEmulationForHDRP()
		{
			WaitForEndOfFrame waitForEndOfFrame = new WaitForEndOfFrame();
			while (base.enabled)
			{
				yield return waitForEndOfFrame;
				OnRenderObject();
			}
		}

		public void OnEnable()
		{
			if (Application.isPlaying && currentScannedMeshesList.Count != 0)
			{
				bool flag = false;
				if (CurrentRenderPipeline.haveAnotherSrpPackages && string.Equals(CurrentRenderPipeline.packageDetected, "HDRP"))
				{
					flag = true;
				}
				if (flag)
				{
					StartCoroutine(OnRenderObject_HookEmulationForHDRP());
				}
			}
		}

		public int GetCurrentLodLevel()
		{
			if (currentLodAccordingToDistance != 9)
			{
				return currentLodAccordingToDistance;
			}
			if (currentLodAccordingToDistance == 9)
			{
				return GetNumberOfLodsGenerated() - 1;
			}
			return 0;
		}

		public float GetCurrentCameraDistance()
		{
			return currentDistanceFromMainCamera;
		}

		public float GetCurrentRealCameraDistance()
		{
			return currentRealDistanceFromMainCamera;
		}

		public int GetNumberOfLodsGenerated()
		{
			int num = 1;
			if (levelsOfDetailToGenerate >= 2)
			{
				num++;
			}
			if (levelsOfDetailToGenerate >= 3)
			{
				num++;
			}
			if (levelsOfDetailToGenerate >= 4)
			{
				num++;
			}
			if (levelsOfDetailToGenerate >= 5)
			{
				num++;
			}
			if (levelsOfDetailToGenerate >= 6)
			{
				num++;
			}
			if (levelsOfDetailToGenerate >= 7)
			{
				num++;
			}
			if (levelsOfDetailToGenerate >= 8)
			{
				num++;
			}
			return num;
		}

		public bool isScannedMeshesCurrentCulled()
		{
			if (currentLodAccordingToDistance == 9)
			{
				return true;
			}
			_ = currentLodAccordingToDistance;
			_ = 9;
			return false;
		}

		public UltimateLevelOfDetailMeshes[] GetListOfAllMeshesScanned()
		{
			List<UltimateLevelOfDetailMeshes> list = new List<UltimateLevelOfDetailMeshes>();
			foreach (ScannedMeshItem currentScannedMeshes in currentScannedMeshesList)
			{
				list.Add(currentScannedMeshes.originalMeshLodsManager);
			}
			return list.ToArray();
		}

		public void ForceShowLod(bool force, int level)
		{
			if (force)
			{
				if (level < 0)
				{
					Debug.LogError("It was not possible to force a LOD on the ULOD component of \"" + base.gameObject.name + "\". The level provided is less than zero.");
					return;
				}
				if (level > levelsOfDetailToGenerate)
				{
					Debug.LogError("It was not possible to force a LOD on the ULOD component of \"" + base.gameObject.name + "\". The level provided is greater than the number of levels of detail that this component has generated.");
					return;
				}
				forcedToShowOnlyALodIndependentOfDistance = level;
			}
			if (!force)
			{
				forcedToShowOnlyALodIndependentOfDistance = -1;
			}
		}

		public bool isThisComponentForcedToShowLod()
		{
			if (forcedToShowOnlyALodIndependentOfDistance != -1)
			{
				return true;
			}
			return false;
		}

		public void ForceDisableLodChangesInThisComponent(bool force)
		{
			forcedToDisableLodsOfThisComponent = force;
		}

		public bool isThisComponentForcedToDisableLodChanges()
		{
			return forcedToDisableLodsOfThisComponent;
		}

		public void ForceThisComponentToUpdateLodsRender()
		{
			lastDistanceFromMainCamera += UnityEngine.Random.Range(0.1f, 1f);
			currentLodAccordingToDistance = -1;
			OnRenderObject();
		}

		public bool isMeshesCurrentScannedAndLodsWorkingInThisComponent()
		{
			if (currentScannedMeshesList.Count == 0)
			{
				return false;
			}
			if (currentScannedMeshesList.Count > 0)
			{
				return true;
			}
			return false;
		}

		public void ScanAllMeshesAndGenerateLodsGroups()
		{
			if (isMeshesCurrentScannedAndLodsWorkingInThisComponent())
			{
				Debug.LogError("It was not possible to start scanning meshes to generate LODs. Component in " + base.gameObject.name + " already has an active scan. It is necessary to undo the current scan before starting a new one.");
			}
			else if (!isMeshesCurrentScannedAndLodsWorkingInThisComponent())
			{
				ScanForMeshesAndGenerateAllLodGroups_StartProcessing(showProgressBar: false);
			}
		}

		public void UndoCurrentScanWorkingAndDeleteGeneratedMeshes(bool runMonoIl2CppGc, bool runUnityGc)
		{
			if (!isMeshesCurrentScannedAndLodsWorkingInThisComponent())
			{
				Debug.LogError("It was not possible to undo the LODs scan existing in the " + base.gameObject.name + " component. There is no scan done yet, it is necessary to perform one before.");
			}
			else if (isMeshesCurrentScannedAndLodsWorkingInThisComponent())
			{
				UndoAllMeshesScannedAndAllLodGroups(showProgressBar: false, deleteAllGeneratedMeshes: true, runMonoIl2CppGc, runUnityGc);
			}
		}

		public UltimateLevelOfDetail[] GetListOfAllUlodsInThisScene()
		{
			if (!Application.isPlaying)
			{
				Debug.LogError("It is only possible to obtain the list of ULODs in this scene, if the application is being executed.");
				return null;
			}
			return cacheOfUlodDataRuntimeInstancesDetector.instancesOfUlodInThisScene.ToArray();
		}

		public UltimateLevelOfDetailOptimizer[] GetListOfAllUlodsOptimizerInThisScene()
		{
			if (!Application.isPlaying)
			{
				Debug.LogError("It is only possible to obtain the list of ULODs Optimizers in this scene, if the application is being executed.");
				return null;
			}
			return cacheOfUlodData.GetComponent<RuntimeInstancesDetector>().instancesOfUlodOptimizerInThisScene.ToArray();
		}

		public void SetNewCustomCameraForThisAndAllUlodsInThisScene(Camera newCustomCamera)
		{
			if (!Application.isPlaying)
			{
				Debug.LogError("It is not possible to define a custom camera for all ULODs components in this scene. This method is only usable at run time.");
				return;
			}
			customCameraForSimulationOfLods = newCustomCamera;
			foreach (UltimateLevelOfDetail item in cacheOfUlodDataRuntimeInstancesDetector.instancesOfUlodInThisScene)
			{
				item.customCameraForSimulationOfLods = newCustomCamera;
			}
		}

		public void ConvertThisToDefaultUnityLods()
		{
			foreach (ScannedMeshItem currentScannedMeshes in currentScannedMeshesList)
			{
				LODGroup lODGroup = null;
				List<LOD> list = new List<LOD>();
				Renderer[] array = new Renderer[1];
				if (currentScannedMeshes.originalSkinnedMeshRenderer != null)
				{
					lODGroup = currentScannedMeshes.originalSkinnedMeshRenderer.gameObject.AddComponent<LODGroup>();
					array[0] = currentScannedMeshes.originalSkinnedMeshRenderer;
				}
				if (currentScannedMeshes.originalMeshRenderer != null && currentScannedMeshes.originalMeshFilter != null)
				{
					lODGroup = currentScannedMeshes.originalMeshRenderer.gameObject.AddComponent<LODGroup>();
					array[0] = currentScannedMeshes.originalMeshRenderer;
				}
				lODGroup.fadeMode = LODFadeMode.CrossFade;
				lODGroup.animateCrossFading = true;
				list.Add(new LOD(0.7f, array));
				for (int i = 0; i <= levelsOfDetailToGenerate; i++)
				{
					if (i != 0)
					{
						GameObject gameObject = new GameObject("LOD " + i + " (Generated By ULOD)");
						Renderer[] array2 = new Renderer[1];
						if (currentScannedMeshes.originalSkinnedMeshRenderer != null)
						{
							gameObject.transform.SetParent(currentScannedMeshes.originalSkinnedMeshRenderer.transform);
							gameObject.transform.localPosition = Vector3.zero;
							gameObject.transform.localEulerAngles = Vector3.zero;
							gameObject.transform.localScale = Vector3.one;
							SkinnedMeshRenderer skinnedMeshRenderer = gameObject.AddComponent<SkinnedMeshRenderer>();
							skinnedMeshRenderer.sharedMesh = currentScannedMeshes.allMeshLods[i];
							skinnedMeshRenderer.bones = currentScannedMeshes.originalSkinnedMeshRenderer.bones;
							skinnedMeshRenderer.rootBone = currentScannedMeshes.originalSkinnedMeshRenderer.rootBone;
							skinnedMeshRenderer.updateWhenOffscreen = currentScannedMeshes.originalSkinnedMeshRenderer.updateWhenOffscreen;
							skinnedMeshRenderer.receiveShadows = currentScannedMeshes.originalSkinnedMeshRenderer.receiveShadows;
							skinnedMeshRenderer.shadowCastingMode = currentScannedMeshes.originalSkinnedMeshRenderer.shadowCastingMode;
							skinnedMeshRenderer.skinnedMotionVectors = currentScannedMeshes.originalSkinnedMeshRenderer.skinnedMotionVectors;
							skinnedMeshRenderer.allowOcclusionWhenDynamic = currentScannedMeshes.originalSkinnedMeshRenderer.allowOcclusionWhenDynamic;
							skinnedMeshRenderer.materials = currentScannedMeshes.allMeshLodsMaterials[i].materialArray;
							array2[0] = skinnedMeshRenderer;
						}
						if (currentScannedMeshes.originalMeshRenderer != null && currentScannedMeshes.originalMeshFilter != null)
						{
							gameObject.transform.SetParent(currentScannedMeshes.originalMeshRenderer.transform);
							gameObject.transform.localPosition = Vector3.zero;
							gameObject.transform.localEulerAngles = Vector3.zero;
							gameObject.transform.localScale = Vector3.one;
							gameObject.AddComponent<MeshFilter>().mesh = currentScannedMeshes.allMeshLods[i];
							MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
							meshRenderer.materials = currentScannedMeshes.allMeshLodsMaterials[i].materialArray;
							meshRenderer.shadowCastingMode = currentScannedMeshes.originalMeshRenderer.shadowCastingMode;
							meshRenderer.receiveShadows = currentScannedMeshes.originalMeshRenderer.receiveShadows;
							meshRenderer.motionVectorGenerationMode = currentScannedMeshes.originalMeshRenderer.motionVectorGenerationMode;
							meshRenderer.allowOcclusionWhenDynamic = currentScannedMeshes.originalMeshRenderer.allowOcclusionWhenDynamic;
							array2[0] = meshRenderer;
						}
						float num = 0.3f / (float)levelsOfDetailToGenerate;
						float screenRelativeTransitionHeight = 0.3f - num * 0.97f * (float)i;
						list.Add(new LOD(screenRelativeTransitionHeight, array2));
					}
				}
				lODGroup.SetLODs(list.ToArray());
				lODGroup.RecalculateBounds();
			}
			string text = base.gameObject.name;
			UndoAllMeshesScannedAndAllLodGroups(showProgressBar: false, deleteAllGeneratedMeshes: false, runMonoIl2CppGc: true, runUnityGc: true);
			if (!Application.isPlaying)
			{
				UnityEngine.Object.DestroyImmediate(this);
			}
			if (Application.isPlaying)
			{
				UnityEngine.Object.Destroy(this);
			}
			Debug.Log("The Ultimate Level Of Detail component in \"" + text + "\" has been removed and all scanned meshes are now managed by Unity's standard \"LOD Group\" components.");
		}
	}
}
