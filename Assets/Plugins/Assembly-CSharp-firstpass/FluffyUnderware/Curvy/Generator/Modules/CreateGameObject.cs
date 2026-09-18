using System;
using System.Collections.Generic;
using FluffyUnderware.DevTools;
using FluffyUnderware.DevTools.Extensions;
using JetBrains.Annotations;
using UnityEngine;

namespace FluffyUnderware.Curvy.Generator.Modules
{
	[ModuleInfo("Create/GameObject", ModuleName = "Create GameObject")]
	[HelpURL("https://curvyeditor.com/doclink/cgcreategameobject")]
	public class CreateGameObject : ResourceExportingModule
	{
		[HideInInspector]
		[InputSlotInfo(new Type[] { typeof(CGGameObject) }, Array = true, Name = "GameObject")]
		public CGModuleInputSlot InGameObjectArray = new CGModuleInputSlot();

		[HideInInspector]
		[InputSlotInfo(new Type[] { typeof(CGSpots) }, Name = "Spots")]
		public CGModuleInputSlot InSpots = new CGModuleInputSlot();

		[SerializeField]
		[CGResourceCollectionManager("GameObject", ShowCount = true)]
		private CGGameObjectResourceCollection m_Resources = new CGGameObjectResourceCollection();

		[Tab("General")]
		[SerializeField]
		private bool m_MakeStatic;

		[SerializeField]
		[Layer("", "")]
		private int m_Layer;

		[Tooltip("Whether Layer should be applied only on the root of a created game object, or it should be applied on its whole hierarchy")]
		[SerializeField]
		private bool applyLayerOnChildren;

		private readonly Dictionary<Transform, string> usedPoolsDictionary = new Dictionary<Transform, string>();

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

		public bool ApplyLayerOnChildren
		{
			get
			{
				return applyLayerOnChildren;
			}
			set
			{
				if (applyLayerOnChildren != value)
				{
					applyLayerOnChildren = value;
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

		public CGGameObjectResourceCollection GameObjects => m_Resources;

		public int GameObjectCount => GameObjects.Count;

		public override void Reset()
		{
			base.Reset();
			MakeStatic = false;
			Layer = 0;
			ApplyLayerOnChildren = false;
		}

		protected override void OnDestroy()
		{
			if (!base.Generator.Destroying)
			{
				DeleteAllPrefabPools();
			}
			base.OnDestroy();
		}

		public override bool DeleteAllOutputManagedResources()
		{
			bool flag = base.DeleteAllOutputManagedResources();
			int childCount = base.transform.childCount;
			flag = flag || childCount > 0;
			Transform[] array = new Transform[childCount];
			for (int i = 0; i < childCount; i++)
			{
				array[i] = base.transform.GetChild(i);
			}
			Transform[] array2 = array;
			foreach (Transform transform in array2)
			{
				if (usedPoolsDictionary.TryGetValue(transform, out var value))
				{
					DeleteManagedResource("GameObject", transform, value);
				}
				else
				{
					DeleteManagedResource("GameObject", transform, string.Empty, dontUsePool: true);
				}
			}
			GameObjects.Items.Clear();
			GameObjects.PoolNames.Clear();
			usedPoolsDictionary.Clear();
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
			TryDeleteChildrenFromAssociatedPrefab();
			DeleteAllOutputManagedResources();
			bool isDataDisposable;
			List<CGGameObject> allData = InGameObjectArray.GetAllData<CGGameObject>(out isDataDisposable, Array.Empty<CGDataRequestParameter>());
			bool isDataDisposable2;
			CGSpots data = InSpots.GetData<CGSpots>(out isDataDisposable2, Array.Empty<CGDataRequestParameter>());
			List<IPool> allPrefabPools = GetAllPrefabPools();
			HashSet<string> hashSet = new HashSet<string>();
			GameObjects.Items.Clear();
			GameObjects.PoolNames.Clear();
			usedPoolsDictionary.Clear();
			if (allData.Count > 0 && data.Count > 0)
			{
				for (int i = 0; i < data.Count; i++)
				{
					CGSpot cGSpot = data.Spots.Array[i];
					int index = cGSpot.Index;
					if (index < 0 || index >= allData.Count || !(allData[index].Object != null))
					{
						continue;
					}
					CGGameObject cGGameObject = allData[index];
					string text = GetPrefabPool(cGGameObject.Object).Identifier;
					hashSet.Add(text);
					Transform transform = (Transform)AddManagedResource("GameObject", text, i);
					transform.gameObject.isStatic = MakeStatic;
					transform.gameObject.layer = Layer;
					if (ApplyLayerOnChildren)
					{
						Transform[] componentsInChildren = transform.gameObject.GetComponentsInChildren<Transform>(includeInactive: true);
						for (int j = 0; j < componentsInChildren.Length; j++)
						{
							componentsInChildren[j].gameObject.layer = Layer;
						}
					}
					transform.localPosition = cGSpot.Position;
					transform.localRotation = cGSpot.Rotation;
					transform.localScale = new Vector3(cGGameObject.Object.transform.localScale.x * cGSpot.Scale.x * cGGameObject.Scale.x, cGGameObject.Object.transform.localScale.y * cGSpot.Scale.y * cGGameObject.Scale.y, cGGameObject.Object.transform.localScale.z * cGSpot.Scale.z * cGGameObject.Scale.z);
					if (cGGameObject.Translate != Vector3.zero)
					{
						transform.Translate(cGGameObject.Translate);
					}
					if (cGGameObject.Rotate != Vector3.zero)
					{
						transform.Rotate(cGGameObject.Rotate);
					}
					GameObjects.Items.Add(transform);
					GameObjects.PoolNames.Add(text);
					usedPoolsDictionary[transform] = text;
				}
			}
			foreach (IPool item in allPrefabPools)
			{
				if (!hashSet.Contains(item.Identifier))
				{
					base.Generator.PoolManager.DeletePool(item);
				}
			}
			if (isDataDisposable)
			{
				allData.ForEach(delegate(CGGameObject d)
				{
					d.Dispose();
				});
			}
			if (isDataDisposable2)
			{
				data.Dispose();
			}
		}

		protected override GameObject SaveResourceToScene(Component managedResource, Transform newParent)
		{
			GameObject obj = managedResource.gameObject.DuplicateGameObject(newParent);
			obj.name = managedResource.name;
			return obj;
		}

		protected override void ResetOnEnable()
		{
			base.ResetOnEnable();
			usedPoolsDictionary.Clear();
		}
	}
}
