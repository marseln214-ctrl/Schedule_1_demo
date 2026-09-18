using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FluffyUnderware.Curvy.Pools;
using FluffyUnderware.DevTools;
using FluffyUnderware.DevTools.Extensions;
using FluffyUnderware.DevTools.Threading;
using JetBrains.Annotations;
using ToolBuddy.Pooling.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace FluffyUnderware.Curvy.Generator.Modules
{
	[ModuleInfo("Create/Mesh", ModuleName = "Create Mesh")]
	[HelpURL("https://curvyeditor.com/doclink/cgcreatemesh")]
	public class CreateMesh : ResourceExportingModule, ISerializationCallbackReceiver
	{
		private const string DefaultTag = "Untagged";

		[HideInInspector]
		[InputSlotInfo(new Type[] { typeof(CGVMesh) }, Array = true, Name = "VMesh")]
		public CGModuleInputSlot InVMeshArray = new CGModuleInputSlot();

		[HideInInspector]
		[InputSlotInfo(new Type[] { typeof(CGSpots) }, Array = true, Name = "Spots", Optional = true)]
		public CGModuleInputSlot InSpots = new CGModuleInputSlot();

		[SerializeField]
		[CGResourceCollectionManager("Mesh", ShowCount = true)]
		private CGMeshResourceCollection m_MeshResources = new CGMeshResourceCollection();

		[Tab("General")]
		[Tooltip("Merge meshes")]
		[FieldCondition("CanUpdate", true, false, ActionAttribute.ActionEnum.Show, null, ActionAttribute.ActionPositionEnum.Below, Action = ActionAttribute.ActionEnum.Enable)]
		[SerializeField]
		private bool m_Combine;

		[SerializeField]
		[Tooltip("Warning: this operation is Editor only (not available in builds) and CPU intensive.\nWhen combining multiple meshes, the UV2s are by default kept as is. Use this option to recompute them by uwrapping the combined mesh.")]
		[FieldCondition("m_Combine", true, false, ActionAttribute.ActionEnum.Show, null, ActionAttribute.ActionPositionEnum.Below, Action = ActionAttribute.ActionEnum.Show)]
		private bool unwrapUV2;

		[Tooltip("When Combine is true, combine only meshes sharing the same index\nIs used only if Spots are provided")]
		[SerializeField]
		private bool m_GroupMeshes;

		[SerializeField]
		[Tooltip("If true, the generated mesh will have normals")]
		private bool includeNormals = true;

		[SerializeField]
		[Tooltip("If true, the generated mesh will have tangents")]
		private bool includeTangents;

		[SerializeField]
		[HideInInspector]
		[FieldCondition("CanUpdate", true, false, ActionAttribute.ActionEnum.Show, null, ActionAttribute.ActionPositionEnum.Below, Action = ActionAttribute.ActionEnum.Enable)]
		private CGYesNoAuto m_AddNormals = CGYesNoAuto.Auto;

		[SerializeField]
		[HideInInspector]
		[FieldCondition("CanUpdate", true, false, ActionAttribute.ActionEnum.Show, null, ActionAttribute.ActionPositionEnum.Below, Action = ActionAttribute.ActionEnum.Enable)]
		private CGYesNoAuto m_AddTangents = CGYesNoAuto.No;

		[SerializeField]
		[HideInInspector]
		[FieldCondition("CanUpdate", true, false, ActionAttribute.ActionEnum.Show, null, ActionAttribute.ActionPositionEnum.Below, Action = ActionAttribute.ActionEnum.Enable)]
		private bool m_AddUV2 = true;

		[SerializeField]
		[Tooltip("If enabled, meshes will have the Static flag set, and will not be generated/updated in Play Mode")]
		[FieldCondition("CanModifyStaticFlag", true, false, ActionAttribute.ActionEnum.Show, null, ActionAttribute.ActionPositionEnum.Below, Action = ActionAttribute.ActionEnum.Enable)]
		private bool m_MakeStatic;

		[SerializeField]
		[Tooltip("The Layer of the created game object")]
		[FieldCondition("CanUpdate", true, false, ActionAttribute.ActionEnum.Show, null, ActionAttribute.ActionPositionEnum.Below, Action = ActionAttribute.ActionEnum.Enable)]
		[Layer("", "")]
		private int m_Layer;

		[SerializeField]
		[Tooltip("The Tag of the created game object")]
		[FieldCondition("CanUpdate", true, false, ActionAttribute.ActionEnum.Show, null, ActionAttribute.ActionPositionEnum.Below, Action = ActionAttribute.ActionEnum.Enable)]
		[Tag("", "")]
		private string m_Tag = "Untagged";

		[Tab("Renderer")]
		[SerializeField]
		[FieldCondition("CanUpdate", true, false, ActionAttribute.ActionEnum.Show, null, ActionAttribute.ActionPositionEnum.Below, Action = ActionAttribute.ActionEnum.Enable)]
		private bool m_RendererEnabled = true;

		[SerializeField]
		[FieldCondition("CanUpdate", true, false, ActionAttribute.ActionEnum.Show, null, ActionAttribute.ActionPositionEnum.Below, Action = ActionAttribute.ActionEnum.Enable)]
		private ShadowCastingMode m_CastShadows = ShadowCastingMode.On;

		[SerializeField]
		[FieldCondition("CanUpdate", true, false, ActionAttribute.ActionEnum.Show, null, ActionAttribute.ActionPositionEnum.Below, Action = ActionAttribute.ActionEnum.Enable)]
		private bool m_ReceiveShadows = true;

		[SerializeField]
		[FieldCondition("CanUpdate", true, false, ActionAttribute.ActionEnum.Show, null, ActionAttribute.ActionPositionEnum.Below, Action = ActionAttribute.ActionEnum.Enable)]
		private LightProbeUsage m_LightProbeUsage = LightProbeUsage.BlendProbes;

		[HideInInspector]
		[SerializeField]
		[FieldCondition("CanUpdate", true, false, ActionAttribute.ActionEnum.Show, null, ActionAttribute.ActionPositionEnum.Below, Action = ActionAttribute.ActionEnum.Enable)]
		private bool m_UseLightProbes = true;

		[SerializeField]
		[FieldCondition("CanUpdate", true, false, ActionAttribute.ActionEnum.Show, null, ActionAttribute.ActionPositionEnum.Below, Action = ActionAttribute.ActionEnum.Enable)]
		private ReflectionProbeUsage m_ReflectionProbes = ReflectionProbeUsage.BlendProbes;

		[SerializeField]
		[FieldCondition("CanUpdate", true, false, ActionAttribute.ActionEnum.Show, null, ActionAttribute.ActionPositionEnum.Below, Action = ActionAttribute.ActionEnum.Enable)]
		private Transform m_AnchorOverride;

		[Tab("Collider")]
		[SerializeField]
		[FieldCondition("CanUpdate", true, false, ActionAttribute.ActionEnum.Show, null, ActionAttribute.ActionPositionEnum.Below, Action = ActionAttribute.ActionEnum.Enable)]
		private CGColliderEnum m_Collider = CGColliderEnum.Mesh;

		[FieldCondition("m_Collider", CGColliderEnum.Mesh, false, ActionAttribute.ActionEnum.Show, null, ActionAttribute.ActionPositionEnum.Below)]
		[SerializeField]
		[FieldCondition("CanUpdate", true, false, ActionAttribute.ActionEnum.Show, null, ActionAttribute.ActionPositionEnum.Below, Action = ActionAttribute.ActionEnum.Enable)]
		private bool m_Convex;

		[SerializeField]
		[FieldCondition("EnableIsTrigger", true, false, ActionAttribute.ActionEnum.Show, null, ActionAttribute.ActionPositionEnum.Below, Action = ActionAttribute.ActionEnum.Enable)]
		private bool m_IsTrigger;

		[Tooltip("Options used to enable or disable certain features in Collider mesh cooking. See Unity's MeshCollider.cookingOptions for more details")]
		[FieldCondition("m_Collider", CGColliderEnum.Mesh, false, ActionAttribute.ActionEnum.Show, null, ActionAttribute.ActionPositionEnum.Below)]
		[SerializeField]
		[EnumFlag("", "")]
		[FieldCondition("CanUpdate", true, false, ActionAttribute.ActionEnum.Show, null, ActionAttribute.ActionPositionEnum.Below, Action = ActionAttribute.ActionEnum.Enable)]
		private MeshColliderCookingOptions m_CookingOptions = MeshColliderCookingOptions.CookForFasterSimulation | MeshColliderCookingOptions.EnableMeshCleaning | MeshColliderCookingOptions.WeldColocatedVertices | MeshColliderCookingOptions.UseFastMidphase;

		[Label("Auto Update", "")]
		[SerializeField]
		private bool m_AutoUpdateColliders = true;

		[SerializeField]
		private PhysicMaterial m_Material;

		private readonly CGSpotComparer cgSpotComparer = new CGSpotComparer();

		public bool Combine
		{
			get
			{
				return m_Combine;
			}
			set
			{
				if (m_Combine != value)
				{
					m_Combine = value;
					base.Dirty = true;
				}
			}
		}

		public bool UnwrapUV2
		{
			get
			{
				return unwrapUV2;
			}
			set
			{
				if (value)
				{
					DTLog.LogWarning("[Curvy] UV2 Unwrapping is not available outside of the editor", this);
				}
				if (unwrapUV2 != value)
				{
					unwrapUV2 = value;
					base.Dirty = true;
				}
			}
		}

		public bool GroupMeshes
		{
			get
			{
				return m_GroupMeshes;
			}
			set
			{
				if (m_GroupMeshes != value)
				{
					m_GroupMeshes = value;
					base.Dirty = true;
				}
			}
		}

		public bool IncludeNormals
		{
			get
			{
				return includeNormals;
			}
			set
			{
				if (includeNormals != value)
				{
					includeNormals = value;
					base.Dirty = true;
				}
			}
		}

		public bool IncludeTangents
		{
			get
			{
				return includeTangents;
			}
			set
			{
				if (includeTangents != value)
				{
					includeTangents = value;
					base.Dirty = true;
				}
			}
		}

		[UsedImplicitly]
		[Obsolete("Use IncludeNormals instead")]
		public CGYesNoAuto AddNormals
		{
			get
			{
				return m_AddNormals;
			}
			set
			{
				if (m_AddNormals != value)
				{
					m_AddNormals = value;
					base.Dirty = true;
				}
			}
		}

		[UsedImplicitly]
		[Obsolete("Use IncludeTangents instead")]
		public CGYesNoAuto AddTangents
		{
			get
			{
				return m_AddTangents;
			}
			set
			{
				if (m_AddTangents != value)
				{
					m_AddTangents = value;
					base.Dirty = true;
				}
			}
		}

		[UsedImplicitly]
		[Obsolete("UV2 is now always added")]
		public bool AddUV2
		{
			get
			{
				return m_AddUV2;
			}
			set
			{
				if (m_AddUV2 != value)
				{
					m_AddUV2 = value;
					base.Dirty = true;
				}
			}
		}

		public int Layer
		{
			get
			{
				return m_Layer;
			}
			set
			{
				int num = Mathf.Clamp(value, 0, 32);
				if (m_Layer != num)
				{
					m_Layer = num;
					base.Dirty = true;
				}
			}
		}

		public string Tag
		{
			get
			{
				return m_Tag;
			}
			set
			{
				if (m_Tag != value)
				{
					m_Tag = value;
					base.Dirty = true;
				}
			}
		}

		public bool MakeStatic
		{
			get
			{
				return m_MakeStatic;
			}
			set
			{
				if (m_MakeStatic != value)
				{
					m_MakeStatic = value;
					base.Dirty = true;
				}
			}
		}

		public bool RendererEnabled
		{
			get
			{
				return m_RendererEnabled;
			}
			set
			{
				if (m_RendererEnabled != value)
				{
					m_RendererEnabled = value;
					base.Dirty = true;
				}
			}
		}

		public ShadowCastingMode CastShadows
		{
			get
			{
				return m_CastShadows;
			}
			set
			{
				if (m_CastShadows != value)
				{
					m_CastShadows = value;
					base.Dirty = true;
				}
			}
		}

		public bool ReceiveShadows
		{
			get
			{
				return m_ReceiveShadows;
			}
			set
			{
				if (m_ReceiveShadows != value)
				{
					m_ReceiveShadows = value;
					base.Dirty = true;
				}
			}
		}

		public bool UseLightProbes
		{
			get
			{
				return m_UseLightProbes;
			}
			set
			{
				if (m_UseLightProbes != value)
				{
					m_UseLightProbes = value;
					base.Dirty = true;
				}
			}
		}

		public LightProbeUsage LightProbeUsage
		{
			get
			{
				return m_LightProbeUsage;
			}
			set
			{
				if (m_LightProbeUsage != value)
				{
					m_LightProbeUsage = value;
					base.Dirty = true;
				}
			}
		}

		public ReflectionProbeUsage ReflectionProbes
		{
			get
			{
				return m_ReflectionProbes;
			}
			set
			{
				if (m_ReflectionProbes != value)
				{
					m_ReflectionProbes = value;
					base.Dirty = true;
				}
			}
		}

		public Transform AnchorOverride
		{
			get
			{
				return m_AnchorOverride;
			}
			set
			{
				if (m_AnchorOverride != value)
				{
					m_AnchorOverride = value;
					base.Dirty = true;
				}
			}
		}

		public CGColliderEnum Collider
		{
			get
			{
				return m_Collider;
			}
			set
			{
				if (m_Collider != value)
				{
					m_Collider = value;
					base.Dirty = true;
				}
			}
		}

		public bool AutoUpdateColliders
		{
			get
			{
				return m_AutoUpdateColliders;
			}
			set
			{
				if (m_AutoUpdateColliders != value)
				{
					m_AutoUpdateColliders = value;
					base.Dirty = true;
				}
			}
		}

		public bool Convex
		{
			get
			{
				return m_Convex;
			}
			set
			{
				if (m_Convex != value)
				{
					m_Convex = value;
					base.Dirty = true;
				}
			}
		}

		public bool IsTrigger
		{
			get
			{
				return m_IsTrigger;
			}
			set
			{
				if (m_IsTrigger != value)
				{
					m_IsTrigger = value;
					base.Dirty = true;
				}
			}
		}

		public MeshColliderCookingOptions CookingOptions
		{
			get
			{
				return m_CookingOptions;
			}
			set
			{
				if (m_CookingOptions != value)
				{
					m_CookingOptions = value;
					base.Dirty = true;
				}
			}
		}

		public PhysicMaterial Material
		{
			get
			{
				return m_Material;
			}
			set
			{
				if (m_Material != value)
				{
					m_Material = value;
					base.Dirty = true;
				}
			}
		}

		[Obsolete("Member is set to become editor only. Contact support if you need it outside of Editor")]
		public CGMeshResourceCollection Meshes => m_MeshResources;

		[Obsolete("Member is set to become editor only. Contact support if you need it outside of Editor")]
		public int MeshCount => m_MeshResources.Count;

		[Obsolete("Member is set to become editor only. Contact support if you need it outside of Editor")]
		public int VertexCount { get; private set; }

		private bool CanGroupMeshes => InSpots.IsLinked;

		private bool CanModifyStaticFlag => false;

		private bool CanUpdate
		{
			get
			{
				if (Application.isPlaying)
				{
					return !MakeStatic;
				}
				return true;
			}
		}

		private bool EnableIsTrigger
		{
			get
			{
				if (CanUpdate)
				{
					if (m_Collider == CGColliderEnum.Mesh)
					{
						return m_Convex;
					}
					return true;
				}
				return false;
			}
		}

		public override void Reset()
		{
			base.Reset();
			Combine = false;
			UnwrapUV2 = false;
			GroupMeshes = false;
			IncludeNormals = true;
			IncludeTangents = false;
			AddNormals = CGYesNoAuto.Auto;
			AddTangents = CGYesNoAuto.No;
			MakeStatic = false;
			Material = null;
			Layer = 0;
			Tag = "Untagged";
			CastShadows = ShadowCastingMode.On;
			RendererEnabled = true;
			ReceiveShadows = true;
			UseLightProbes = true;
			LightProbeUsage = LightProbeUsage.BlendProbes;
			ReflectionProbes = ReflectionProbeUsage.BlendProbes;
			AnchorOverride = null;
			Collider = CGColliderEnum.Mesh;
			AutoUpdateColliders = true;
			Convex = false;
			IsTrigger = false;
			AddUV2 = true;
			CookingOptions = MeshColliderCookingOptions.CookForFasterSimulation | MeshColliderCookingOptions.EnableMeshCleaning | MeshColliderCookingOptions.WeldColocatedVertices | MeshColliderCookingOptions.UseFastMidphase;
		}

		public CreateMesh()
		{
			base.Version = "1";
		}

		public override bool DeleteAllOutputManagedResources()
		{
			bool flag = base.DeleteAllOutputManagedResources();
			int childCount = base.transform.childCount;
			flag = flag || childCount > 0;
			List<CGMeshResource> list = new List<CGMeshResource>(childCount);
			List<Transform> list2 = new List<Transform>();
			for (int i = 0; i < childCount; i++)
			{
				Transform child = base.transform.GetChild(i);
				if (child.TryGetComponent<CGMeshResource>(out var component))
				{
					list.Add(component);
				}
				else
				{
					list2.Add(child);
				}
			}
			foreach (CGMeshResource item in list)
			{
				DeleteManagedResource("Mesh", item);
			}
			foreach (Transform item2 in list2)
			{
				item2.gameObject.Destroy(isUndoable: false, doPrefabCheck: true);
			}
			VertexCount = 0;
			m_MeshResources.Items.Clear();
			return flag;
		}

		[UsedImplicitly]
		[Obsolete("Use DeleteAllOutputManagedResources instead")]
		public void Clear()
		{
			DeleteAllOutputManagedResources();
		}

		public override void Refresh()
		{
			base.Refresh();
			if (CanUpdate)
			{
				TryDeleteChildrenFromAssociatedPrefab();
				DeleteAllOutputManagedResources();
				bool isDataDisposable;
				List<CGVMesh> allData = InVMeshArray.GetAllData<CGVMesh>(out isDataDisposable, Array.Empty<CGDataRequestParameter>());
				bool isDataDisposable2;
				List<CGSpots> allData2 = InSpots.GetAllData<CGSpots>(out isDataDisposable2, Array.Empty<CGDataRequestParameter>());
				bool arrayIsCopy;
				SubArray<CGSpot>? subArray = ToOneDimensionalArray(allData2, out arrayIsCopy);
				int count = allData.Count;
				VertexCount = 0;
				m_MeshResources.Items.Clear();
				if (count > 0 && (!InSpots.IsLinked || (subArray.HasValue && subArray.Value.Count > 0)))
				{
					if (subArray.HasValue && subArray.Value.Count > 0)
					{
						SubArray<CGSpot> value = subArray.Value;
						for (int i = 0; i < value.Count; i++)
						{
							CGSpot cGSpot = value.Array[i];
							if (cGSpot.Index >= count)
							{
								int num = count - 1;
								UIMessages.Add($"Spot index {cGSpot.Index} references an non existing VMesh. There is/are only {count} valid input VMesh(es). An index of {num} was used instead");
								value.Array[i] = new CGSpot(num, cGSpot.Position, cGSpot.Rotation, cGSpot.Scale);
							}
						}
						CreateSpotMeshes(allData, subArray.Value, Combine, arrayIsCopy, m_MeshResources.Items);
					}
					else
					{
						CreateMeshes(allData, Combine, m_MeshResources.Items);
					}
				}
				if (arrayIsCopy)
				{
					ArrayPools.CGSpot.Free(subArray.Value);
				}
				if (isDataDisposable)
				{
					allData.ForEach(delegate(CGVMesh d)
					{
						d.Dispose();
					});
				}
				if (isDataDisposable2)
				{
					allData2.ForEach(delegate(CGSpots d)
					{
						d.Dispose();
					});
				}
				if (AutoUpdateColliders)
				{
					UpdateColliders();
				}
			}
			else
			{
				UIMessages.Add("Make Static is enabled. This stops mesh generation in Play Mode, to maintain Unity optimizations done in Edit Mode on static GameObjects.");
			}
			if (MakeStatic && !CurvyGlobalManager.SaveGeneratorOutputs)
			{
				UIMessages.Add("Make Static is incompatible with Preferences -> Curvy -> Save Generator Outputs being false.");
			}
		}

		public void UpdateColliders()
		{
			List<CGMeshResource> items = m_MeshResources.Items;
			bool flag = true;
			if (Collider == CGColliderEnum.Mesh && items.Count > 1)
			{
				SubArray<int> meshIds = ArrayPools.Int32.Allocate(items.Count, clearArray: false);
				for (int j = 0; j < items.Count; j++)
				{
					if (items[j] == null)
					{
						meshIds.Array[j] = 0;
					}
					else
					{
						meshIds.Array[j] = items[j].Filter.sharedMesh.GetInstanceID();
					}
				}
				Parallel.For(0, items.Count, delegate(int i)
				{
					Physics.BakeMesh(meshIds.Array[i], Convex);
				});
				ArrayPools.Int32.Free(meshIds);
			}
			for (int k = 0; k < items.Count; k++)
			{
				if (!(items[k] == null) && !items[k].UpdateCollider(Collider, Convex, IsTrigger, Material, CookingOptions))
				{
					flag = false;
				}
			}
			if (!flag)
			{
				UIMessages.Add("Error setting collider!");
			}
		}

		public void OnBeforeSerialize()
		{
		}

		public void OnAfterDeserialize()
		{
			if (string.IsNullOrEmpty(base.Version))
			{
				base.Version = "1";
				IncludeNormals = AddNormals != CGYesNoAuto.No;
				IncludeTangents = AddTangents != CGYesNoAuto.No;
			}
		}

		private void CreateMeshes(List<CGVMesh> vMeshes, bool combine, [NotNull] List<CGMeshResource> createdMeshes)
		{
			if (combine && vMeshes.Count > 1)
			{
				CGVMesh cGVMesh = new CGVMesh();
				cGVMesh.MergeVMeshes(vMeshes, 0, vMeshes.Count - 1);
				WriteVMeshToMesh(cGVMesh, createdMeshes);
			}
			else
			{
				for (int i = 0; i < vMeshes.Count; i++)
				{
					WriteVMeshToMesh(vMeshes[i], createdMeshes);
				}
			}
		}

		private void CreateSpotMeshes(List<CGVMesh> vMeshes, SubArray<CGSpot> spots, bool combine, bool spotsIsACopy, [NotNull] List<CGMeshResource> createdMeshes)
		{
			int count = vMeshes.Count;
			bool flag = combine && GroupMeshes && !spotsIsACopy;
			if (flag)
			{
				spots = ArrayPools.CGSpot.Clone(spots);
			}
			if (combine)
			{
				if (GroupMeshes)
				{
					Array.Sort(spots.Array, 0, spots.Count, cgSpotComparer);
				}
				CGSpot cGSpot = spots.Array[0];
				CGVMesh cGVMesh = new CGVMesh(vMeshes[cGSpot.Index]);
				if (cGSpot.Position != Vector3.zero || cGSpot.Rotation != Quaternion.identity || cGSpot.Scale != Vector3.one)
				{
					cGVMesh.TRS(cGSpot.Matrix);
				}
				for (int i = 1; i < spots.Count; i++)
				{
					cGSpot = spots.Array[i];
					if (cGSpot.Index <= -1 || cGSpot.Index >= count)
					{
						continue;
					}
					if (GroupMeshes && cGSpot.Index != spots.Array[i - 1].Index)
					{
						WriteVMeshToMesh(cGVMesh, createdMeshes);
						cGVMesh.Dispose();
						cGVMesh = new CGVMesh(vMeshes[cGSpot.Index]);
						if (!cGSpot.Matrix.isIdentity)
						{
							cGVMesh.TRS(cGSpot.Matrix);
						}
					}
					else
					{
						cGVMesh.MergeVMesh(vMeshes[cGSpot.Index], cGSpot.Matrix);
					}
				}
				WriteVMeshToMesh(cGVMesh, createdMeshes);
				cGVMesh.Dispose();
			}
			else
			{
				for (int j = 0; j < spots.Count; j++)
				{
					CGSpot cGSpot = spots.Array[j];
					if (cGSpot.Index > -1 && cGSpot.Index < count)
					{
						CGMeshResource cGMeshResource = WriteVMeshToMesh(vMeshes[cGSpot.Index], createdMeshes);
						if (cGSpot.Position != Vector3.zero || cGSpot.Rotation != Quaternion.identity || cGSpot.Scale != Vector3.one)
						{
							cGSpot.ToTransform(cGMeshResource.Filter.transform);
						}
					}
				}
			}
			if (flag)
			{
				ArrayPools.CGSpot.Free(spots);
			}
		}

		private CGMeshResource WriteVMeshToMesh(CGVMesh vmesh, List<CGMeshResource> cgMeshResources)
		{
			CGMeshResource newMesh = GetNewMesh(cgMeshResources.Count);
			cgMeshResources.Add(newMesh);
			MeshFilter filter = newMesh.Filter;
			if (CanModifyStaticFlag)
			{
				filter.gameObject.isStatic = false;
			}
			Mesh mesh = filter.sharedMesh;
			newMesh.gameObject.layer = Layer;
			newMesh.gameObject.tag = Tag;
			vmesh.ToMesh(ref mesh, IncludeNormals, IncludeTangents);
			VertexCount += vmesh.Count;
			if (IncludeNormals && (!vmesh.HasNormals || vmesh.HasPartialNormals))
			{
				mesh.RecalculateNormals();
			}
			if (IncludeTangents && (!vmesh.HasTangents || vmesh.HasPartialTangents))
			{
				mesh.RecalculateTangents();
			}
			if (Combine && UnwrapUV2 && vmesh.HasUV2)
			{
				DTLog.LogError("[Curvy] UV2 Unwrapping is not available outside of the editor", this);
			}
			filter.transform.localPosition = Vector3.zero;
			filter.transform.localRotation = Quaternion.identity;
			filter.transform.localScale = Vector3.one;
			if (CanModifyStaticFlag)
			{
				filter.gameObject.isStatic = MakeStatic;
			}
			newMesh.Renderer.sharedMaterials = vmesh.GetMaterials();
			return newMesh;
		}

		private CGMeshResource GetNewMesh(int currentMeshCount)
		{
			CGMeshResource cGMeshResource = (CGMeshResource)AddManagedResource("Mesh", "", currentMeshCount);
			cGMeshResource.Renderer.shadowCastingMode = CastShadows;
			cGMeshResource.Renderer.enabled = RendererEnabled;
			cGMeshResource.Renderer.receiveShadows = ReceiveShadows;
			cGMeshResource.Renderer.lightProbeUsage = LightProbeUsage;
			cGMeshResource.Renderer.reflectionProbeUsage = ReflectionProbes;
			cGMeshResource.Renderer.probeAnchor = AnchorOverride;
			if (!cGMeshResource.ColliderMatches(Collider))
			{
				cGMeshResource.RemoveCollider();
			}
			cGMeshResource.Filter.sharedMesh.name = "Mesh";
			return cGMeshResource;
		}

		private static SubArray<CGSpot>? ToOneDimensionalArray(List<CGSpots> spotsList, out bool arrayIsCopy)
		{
			SubArray<CGSpot>? result;
			switch (spotsList.Count)
			{
			case 1:
				if (spotsList[0] != null)
				{
					result = new SubArray<CGSpot>(spotsList[0].Spots.Array, spotsList[0].Spots.Count);
					arrayIsCopy = false;
				}
				else
				{
					result = null;
					arrayIsCopy = false;
				}
				break;
			case 0:
				result = null;
				arrayIsCopy = false;
				break;
			default:
			{
				result = ArrayPools.CGSpot.Allocate(spotsList.Where((CGSpots s) => s != null).Sum((CGSpots s) => s.Count));
				arrayIsCopy = true;
				CGSpot[] array = result.Value.Array;
				int num = 0;
				foreach (CGSpots spots in spotsList)
				{
					if (spots != null)
					{
						Array.Copy(spots.Spots.Array, 0, array, num, spots.Spots.Count);
						num += spots.Spots.Count;
					}
				}
				break;
			}
			}
			return result;
		}

		[System.Diagnostics.Conditional("CURVY_SANITY_CHECKS_PRIVATE")]
		private void ValidateMesh(Mesh mesh)
		{
			if (IncludeNormals)
			{
				Vector3[] normals = mesh.normals;
				for (int i = 0; i < normals.Length; i++)
				{
					if (normals[i] == Vector3.zero)
					{
						DTLog.LogError($"Mesh {mesh.name} has a zero normal at index {i}");
					}
				}
			}
			if (!IncludeTangents)
			{
				return;
			}
			Vector4[] tangents = mesh.tangents;
			for (int j = 0; j < tangents.Length; j++)
			{
				if (tangents[j] == Vector4.zero)
				{
					DTLog.LogError($"Mesh {mesh.name} has a zero tangent at index {j}");
				}
			}
		}

		protected override void ResetOnEnable()
		{
			base.ResetOnEnable();
		}

		public void SaveToAsset()
		{
			throw new InvalidOperationException("Operation available only in editor");
		}

		public void SaveToSceneAndAsset()
		{
			throw new InvalidOperationException("Operation available only in editor");
		}

		protected override GameObject SaveResourceToScene(Component managedResource, Transform newParent)
		{
			Mesh sharedMesh = UnityEngine.Object.Instantiate(managedResource.GetComponent<MeshFilter>().sharedMesh);
			GameObject obj = managedResource.gameObject.DuplicateGameObject(newParent);
			obj.name = managedResource.name;
			obj.GetComponent<CGMeshResource>().Destroy(isUndoable: false, doPrefabCheck: true);
			obj.GetComponent<MeshFilter>().sharedMesh = sharedMesh;
			return obj;
		}
	}
}
