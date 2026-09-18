using System;
using System.Collections.Generic;
using FluffyUnderware.DevTools;
using FluffyUnderware.DevTools.Extensions;
using JetBrains.Annotations;
using UnityEngine;

namespace FluffyUnderware.Curvy.Generator
{
	[RequireComponent(typeof(MeshRenderer))]
	[HelpURL("https://curvyeditor.com/doclink/cgmeshresource")]
	public class CGMeshResource : DuplicateEditorMesh, IPoolable
	{
		public const MeshColliderCookingOptions EverMeshColliderCookingOptions = MeshColliderCookingOptions.CookForFasterSimulation | MeshColliderCookingOptions.EnableMeshCleaning | MeshColliderCookingOptions.WeldColocatedVertices | MeshColliderCookingOptions.UseFastMidphase;

		private MeshRenderer mRenderer;

		private Collider mCollider;

		private static readonly HashSet<Mesh> UsedMeshes = new HashSet<Mesh>();

		public MeshRenderer Renderer
		{
			get
			{
				if (mRenderer == null)
				{
					mRenderer = GetComponent<MeshRenderer>();
				}
				return mRenderer;
			}
		}

		public Collider Collider
		{
			get
			{
				if (mCollider == null)
				{
					mCollider = GetComponent<Collider>();
				}
				return mCollider;
			}
		}

		[UsedImplicitly]
		[Obsolete("No more used in Curvy. Will get removed. Copy it if you still need it")]
		public Mesh Prepare()
		{
			return base.Filter.PrepareNewShared();
		}

		public bool ColliderMatches(CGColliderEnum type)
		{
			if (Collider == null && type == CGColliderEnum.None)
			{
				return true;
			}
			if (Collider is MeshCollider && type == CGColliderEnum.Mesh)
			{
				return true;
			}
			if (Collider is BoxCollider && type == CGColliderEnum.Box)
			{
				return true;
			}
			if (Collider is SphereCollider && type == CGColliderEnum.Sphere)
			{
				return true;
			}
			if (Collider is CapsuleCollider && type == CGColliderEnum.Capsule)
			{
				return true;
			}
			return false;
		}

		public void RemoveCollider()
		{
			if ((bool)Collider)
			{
				mCollider.Destroy(isUndoable: false, doPrefabCheck: false);
				mCollider = null;
			}
		}

		public bool UpdateCollider(CGColliderEnum mode, bool convex, bool isTrigger, PhysicMaterial material, MeshColliderCookingOptions meshCookingOptions = MeshColliderCookingOptions.CookForFasterSimulation | MeshColliderCookingOptions.EnableMeshCleaning | MeshColliderCookingOptions.WeldColocatedVertices | MeshColliderCookingOptions.UseFastMidphase)
		{
			if (Collider == null)
			{
				switch (mode)
				{
				case CGColliderEnum.Mesh:
					mCollider = base.gameObject.AddComponent<MeshCollider>();
					break;
				case CGColliderEnum.Box:
					mCollider = base.gameObject.AddComponent<BoxCollider>();
					break;
				case CGColliderEnum.Sphere:
					mCollider = base.gameObject.AddComponent<SphereCollider>();
					break;
				case CGColliderEnum.Capsule:
					mCollider = base.gameObject.AddComponent<CapsuleCollider>();
					break;
				default:
					throw new ArgumentOutOfRangeException();
				case CGColliderEnum.None:
					break;
				}
			}
			switch (mode)
			{
			case CGColliderEnum.Mesh:
			{
				MeshCollider meshCollider = Collider as MeshCollider;
				if (meshCollider != null)
				{
					meshCollider.sharedMesh = null;
					meshCollider.convex = convex;
					meshCollider.isTrigger = isTrigger;
					meshCollider.cookingOptions = meshCookingOptions;
					try
					{
						meshCollider.sharedMesh = base.Filter.sharedMesh;
					}
					catch
					{
						return false;
					}
				}
				else
				{
					DTLog.LogError("[Curvy] Collider of wrong type", this);
				}
				goto IL_027c;
			}
			case CGColliderEnum.Box:
			{
				BoxCollider boxCollider = Collider as BoxCollider;
				if (boxCollider != null)
				{
					boxCollider.isTrigger = isTrigger;
					boxCollider.center = base.Filter.sharedMesh.bounds.center;
					boxCollider.size = base.Filter.sharedMesh.bounds.size;
				}
				else
				{
					DTLog.LogError("[Curvy] Collider of wrong type", this);
				}
				goto IL_027c;
			}
			case CGColliderEnum.Sphere:
			{
				SphereCollider sphereCollider = Collider as SphereCollider;
				if (sphereCollider != null)
				{
					sphereCollider.isTrigger = isTrigger;
					sphereCollider.center = base.Filter.sharedMesh.bounds.center;
					sphereCollider.radius = base.Filter.sharedMesh.bounds.extents.magnitude;
				}
				else
				{
					DTLog.LogError("[Curvy] Collider of wrong type", this);
				}
				goto IL_027c;
			}
			case CGColliderEnum.Capsule:
			{
				CapsuleCollider capsuleCollider = Collider as CapsuleCollider;
				if (capsuleCollider != null)
				{
					Bounds bounds = base.Filter.sharedMesh.bounds;
					capsuleCollider.isTrigger = isTrigger;
					capsuleCollider.center = bounds.center;
					capsuleCollider.radius = new Vector2(bounds.extents.x, bounds.extents.y).magnitude;
					capsuleCollider.height = bounds.size.z;
					capsuleCollider.direction = 2;
				}
				else
				{
					DTLog.LogError("[Curvy] Collider of wrong type", this);
				}
				goto IL_027c;
			}
			default:
				throw new ArgumentOutOfRangeException();
			case CGColliderEnum.None:
				break;
				IL_027c:
				Collider.material = material;
				break;
			}
			return true;
		}

		public void OnBeforePush()
		{
			Mesh sharedMesh = base.Filter.sharedMesh;
			if ((object)sharedMesh != null)
			{
				sharedMesh.Clear();
				sharedMesh.subMeshCount = 0;
			}
			base.transform.DeleteChildren(isUndoable: false, doPrefabCheck: true);
		}

		public void OnAfterPop()
		{
			MeshFilter filter = base.Filter;
			if ((object)filter.sharedMesh == null)
			{
				Mesh newMesh = GetNewMesh();
				filter.sharedMesh = newMesh;
			}
		}

		private static Mesh GetNewMesh()
		{
			Mesh mesh = new Mesh();
			mesh.MarkDynamic();
			UsedMeshes.Add(mesh);
			return mesh;
		}

		private static Mesh GetNewMesh([NotNull] Mesh oldMesh)
		{
			Mesh mesh = UnityEngine.Object.Instantiate(oldMesh);
			mesh.MarkDynamic();
			UsedMeshes.Add(mesh);
			return mesh;
		}

		[UsedImplicitly]
		protected void Awake()
		{
			MeshFilter filter = base.Filter;
			Mesh sharedMesh = filter.sharedMesh;
			if ((object)sharedMesh == null)
			{
				return;
			}
			if (UsedMeshes.Contains(sharedMesh))
			{
				if (sharedMesh.isReadable)
				{
					Mesh sharedMesh2 = (filter.sharedMesh = GetNewMesh(sharedMesh));
					MeshCollider meshCollider = Collider as MeshCollider;
					if (meshCollider != null && (object)meshCollider.sharedMesh == sharedMesh)
					{
						meshCollider.sharedMesh = sharedMesh2;
					}
				}
			}
			else
			{
				UsedMeshes.Add(sharedMesh);
			}
		}

		[UsedImplicitly]
		public void OnDestroy()
		{
			Mesh sharedMesh = base.Filter.sharedMesh;
			if ((object)sharedMesh != null)
			{
				UsedMeshes.Remove(sharedMesh);
			}
		}

		[UsedImplicitly]
		[Obsolete("Too slow, used only in sanity checks")]
		private static bool UsesSharedMesh(CGMeshResource meshResource)
		{
			MeshFilter filter = meshResource.Filter;
			if ((bool)filter && (object)filter.sharedMesh != null)
			{
				UnityEngine.Object[] array = UnityEngine.Object.FindObjectsOfType(typeof(CGMeshResource));
				for (int i = 0; i < array.Length; i++)
				{
					CGMeshResource cGMeshResource = (CGMeshResource)array[i];
					if ((object)cGMeshResource != meshResource)
					{
						MeshFilter filter2 = cGMeshResource.Filter;
						if ((object)filter2 != null && (object)filter2.sharedMesh == filter.sharedMesh)
						{
							return true;
						}
					}
				}
			}
			return false;
		}
	}
}
