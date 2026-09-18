using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using FluffyUnderware.Curvy.Generator.Modules;
using FluffyUnderware.DevTools;
using FluffyUnderware.DevTools.Extensions;
using JetBrains.Annotations;
using UnityEngine;

namespace FluffyUnderware.Curvy.Generator
{
	[ExecuteAlways]
	[HelpURL("https://curvyeditor.com/doclink/generator")]
	[AddComponentMenu("Curvy/Generator")]
	[RequireComponent(typeof(PoolManager))]
	public class CurvyGenerator : DTVersionedMonoBehaviour
	{
		private class ModuleSorter
		{
			[ItemNotNull]
			[NotNull]
			private readonly HashSet<CGModule> modulesWithCircularReferences = new HashSet<CGModule>();

			[NotNull]
			private readonly Dictionary<CGModule, int> modulesAncestorCount = new Dictionary<CGModule, int>();

			public bool SortingNeeded { get; set; } = true;


			public bool HasCircularReference([NotNull] CGModule module)
			{
				return modulesWithCircularReferences.Contains(module);
			}

			public void EnsureIsSorted(List<CGModule> modules)
			{
				if (SortingNeeded)
				{
					Sort(modules);
					SortingNeeded = false;
				}
			}

			private void Sort([NotNull] List<CGModule> modules)
			{
				modulesWithCircularReferences.Clear();
				modulesAncestorCount.Clear();
				if (modules.Count == 0)
				{
					return;
				}
				List<CGModule> list = new List<CGModule>(modules);
				List<CGModule> list2 = new List<CGModule>();
				List<CGModule> list3 = new List<CGModule>();
				List<CGModule> list4 = new List<CGModule>(modules.Count);
				for (int num = list.Count - 1; num >= 0; num--)
				{
					CGModule cGModule = list[num];
					int num2 = cGModule.Input.Where((CGModuleInputSlot t) => t.IsLinked).Sum((CGModuleInputSlot t) => t.LinkedSlots.Count);
					modulesAncestorCount[cGModule] = num2;
					if (cGModule is INoProcessing)
					{
						list3.Add(cGModule);
						list.RemoveAt(num);
					}
					else if (num2 == 0)
					{
						list2.Add(cGModule);
						list.RemoveAt(num);
					}
				}
				list2.Sort((CGModule a, CGModule b) => a.UniqueID.CompareTo(b.UniqueID));
				int num3 = 0;
				while (list2.Count > 0)
				{
					CGModule cGModule2 = list2[0];
					list2.RemoveAt(0);
					IEnumerable<CGModuleSlot> enumerable = cGModule2.Output.SelectMany((CGModuleOutputSlot outputSlot) => outputSlot.LinkedSlots);
					List<CGModule> list5 = new List<CGModule>();
					foreach (CGModuleSlot item in enumerable)
					{
						CGModule module = item.Module;
						if (modulesAncestorCount[module] <= 0)
						{
							DTLog.LogError("[Curvy] Modules sorting encountered an unexpected error. Please raise a bug report.");
							if (!list5.Contains(module))
							{
								list5.Add(module);
							}
							modulesAncestorCount[module] = 0;
						}
						else
						{
							int num4 = modulesAncestorCount[module] - 1;
							if (num4 == 0)
							{
								list5.Add(module);
							}
							modulesAncestorCount[module] = num4;
						}
					}
					list2.AddRange(list5);
					for (int i = 0; i < list5.Count; i++)
					{
						list.Remove(list5[i]);
					}
					list4.Add(cGModule2);
					cGModule2.transform.SetSiblingIndex(num3++);
				}
				modulesWithCircularReferences.UnionWith(list);
				modules.Clear();
				modules.AddRange(list4);
				modules.AddRange(list);
				modules.AddRange(list3);
			}
		}

		private class ModulesSynchronizer
		{
			private bool hasPendingRequest;

			[System.Diagnostics.Conditional("UNITY_EDITOR")]
			public void RequestSynchronization()
			{
				hasPendingRequest = true;
			}

			[System.Diagnostics.Conditional("UNITY_EDITOR")]
			public void CancelRequests()
			{
				hasPendingRequest = false;
			}

			[System.Diagnostics.Conditional("UNITY_EDITOR")]
			public void ProcessRequests([NotNull] CurvyGenerator curvyGenerator)
			{
				if (hasPendingRequest)
				{
					hasPendingRequest = false;
				}
			}

			[System.Diagnostics.Conditional("UNITY_EDITOR")]
			private static void AddMissingChildModules([NotNull] CurvyGenerator curvyGenerator)
			{
				Transform transform = curvyGenerator.transform;
				for (int i = 0; i < transform.childCount; i++)
				{
					CGModule component = transform.GetChild(i).GetComponent<CGModule>();
					if (!(component == null) && !curvyGenerator.Modules.Contains(component))
					{
						component.InputLinks.Clear();
						component.OutputLinks.Clear();
						component.ReInitializeLinkedSlots();
						curvyGenerator.AddModule(component);
					}
				}
			}
		}

		private class Timer
		{
			private double lastTimestamp;

			private static double Now => DTTime.TimeSinceStartup;

			private void ValidateTimes(float timeLimit, float editorTimeLimit)
			{
				double now = Now;
				if (lastTimestamp > now)
				{
					lastTimestamp = now - (double)timeLimit;
				}
			}

			public bool Update(float timeLimit, float editorTimeLimit)
			{
				double now = Now;
				ValidateTimes(timeLimit, editorTimeLimit);
				if (Application.isPlaying)
				{
					if (now - lastTimestamp > (double)timeLimit)
					{
						lastTimestamp = now;
						return true;
					}
					return false;
				}
				return false;
			}

			public void Reset()
			{
				lastTimestamp = 0.0;
			}
		}

		[Tooltip("Show Debug Output?")]
		[SerializeField]
		private bool m_ShowDebug;

		[Tooltip("Whether to automatically refresh the generator's output when necessary")]
		[SerializeField]
		private bool m_AutoRefresh = true;

		[FieldCondition("m_AutoRefresh", true, false, ActionAttribute.ActionEnum.Show, null, ActionAttribute.ActionPositionEnum.Below)]
		[Positive(Tooltip = "The minimum delay between two automatic generator's refreshing while in Play mode. Expressed in milliseconds of real time")]
		[SerializeField]
		private int m_RefreshDelay;

		[FieldCondition("m_AutoRefresh", true, false, ActionAttribute.ActionEnum.Show, null, ActionAttribute.ActionPositionEnum.Below)]
		[Positive(Tooltip = "The minimum delay between two automatic generator's refreshing while in Edit mode. Expressed in milliseconds of real time")]
		[SerializeField]
		private int m_RefreshDelayEditor = 10;

		[Section("Events", false, false, 1000, HelpURL = "https://curvyeditor.com/doclink/generator_events")]
		[SerializeField]
		protected CurvyCGEvent m_OnRefresh = new CurvyCGEvent();

		[HideInInspector]
		public List<CGModule> Modules = new List<CGModule>();

		private bool isInitialized;

		private bool isInitializedPhaseOne;

		private PoolManager poolManager;

		[NotNull]
		private readonly Timer autoRefreshTimer = new Timer();

		[NotNull]
		private readonly ModuleSorter moduleSorter = new ModuleSorter();

		[NotNull]
		private readonly ModulesSynchronizer modulesSynchronizer = new ModulesSynchronizer();

		private const int ModulesReorderingDeltaX = 50;

		private const int ModulesReorderingDeltaY = 20;

		[UsedImplicitly]
		[Obsolete("No more used. Retrieve the Ids from Modules by using Modules[x].UniqueID")]
		internal int m_LastModuleID
		{
			get
			{
				return Modules.Max((CGModule m) => m.UniqueID);
			}
			set
			{
				throw new InvalidOperationException("ModulesByID can't be set");
			}
		}

		public bool ShowDebug
		{
			get
			{
				return m_ShowDebug;
			}
			set
			{
				m_ShowDebug = value;
			}
		}

		public bool AutoRefresh
		{
			get
			{
				return m_AutoRefresh;
			}
			set
			{
				m_AutoRefresh = value;
			}
		}

		public int RefreshDelay
		{
			get
			{
				return m_RefreshDelay;
			}
			set
			{
				int num = Mathf.Max(0, value);
				if (m_RefreshDelay != num)
				{
					m_RefreshDelay = num;
				}
			}
		}

		public int RefreshDelayEditor
		{
			get
			{
				return m_RefreshDelayEditor;
			}
			set
			{
				int num = Mathf.Max(0, value);
				if (m_RefreshDelayEditor != num)
				{
					m_RefreshDelayEditor = num;
				}
			}
		}

		public PoolManager PoolManager
		{
			get
			{
				if (poolManager == null)
				{
					poolManager = GetComponent<PoolManager>();
				}
				return poolManager;
			}
		}

		public CurvyCGEvent OnRefresh
		{
			get
			{
				return m_OnRefresh;
			}
			set
			{
				m_OnRefresh = value;
			}
		}

		public bool IsInitialized => isInitialized;

		public bool Destroying { get; private set; }

		[UsedImplicitly]
		[Obsolete("Dictionary no more used. Retrieve he Ids from Modules by using Modules[x].UniqueID")]
		public Dictionary<int, CGModule> ModulesByID
		{
			get
			{
				return Modules.ToDictionary((CGModule m) => m.UniqueID, (CGModule m) => m);
			}
			set
			{
				throw new InvalidOperationException("ModulesByID can't be set");
			}
		}

		private bool HasModulesWithSameID => (from module in Modules
			group module by module.UniqueID).Any((IGrouping<int, CGModule> group) => group.Count() > 1);

		protected override void OnEnable()
		{
			base.OnEnable();
			PoolManager.AutoCreatePools = true;
		}

		protected override void OnDisable()
		{
			base.OnDisable();
			isInitialized = false;
			isInitializedPhaseOne = false;
		}

		[UsedImplicitly]
		private void OnDestroy()
		{
			Destroying = true;
		}

		[UsedImplicitly]
		private void Update()
		{
			if (!IsInitialized)
			{
				Initialize();
			}
			else
			{
				TryAutoRefresh();
			}
		}

		public static CurvyGenerator Create()
		{
			return new GameObject("Curvy Generator", typeof(CurvyGenerator)).GetComponent<CurvyGenerator>();
		}

		public T AddModule<T>() where T : CGModule
		{
			return (T)AddModule(typeof(T));
		}

		[NotNull]
		public CGModule AddModule(Type type)
		{
			GameObject obj = new GameObject("");
			obj.transform.SetParent(base.transform, worldPositionStays: false);
			CGModule cGModule = (CGModule)obj.AddComponent(type);
			AddModule(cGModule);
			return cGModule;
		}

		public void AddModule([NotNull] CGModule module)
		{
			if (module == null)
			{
				throw new ArgumentNullException("module");
			}
			if (module.transform.parent != base.transform)
			{
				throw new ArgumentException("Module must be a child of the Generator");
			}
			Modules.Add(module);
			if (!module.IsInitialized)
			{
				module.Initialize();
			}
			module.UniqueID = GetModuleUniqueID(module);
			module.ModuleName = GetModuleUniqueName(module);
			moduleSorter.SortingNeeded = true;
		}

		public void RemoveModule([NotNull] CGModule module)
		{
			if (Modules.Remove(module) && Modules.Any())
			{
				moduleSorter.SortingNeeded = true;
			}
		}

		public void ArrangeModules()
		{
			Vector2 vector = new Vector2(float.MaxValue, float.MaxValue);
			foreach (CGModule module in Modules)
			{
				vector.x = Mathf.Min(module.Properties.Dimensions.x, vector.x);
				vector.y = Mathf.Min(module.Properties.Dimensions.y, vector.y);
			}
			vector -= new Vector2(10f, 10f);
			foreach (CGModule module2 in Modules)
			{
				module2.Properties.Dimensions.x -= vector.x;
				module2.Properties.Dimensions.y -= vector.y;
			}
		}

		public void ReorderModules()
		{
			Dictionary<CGModule, Rect> dictionary = new Dictionary<CGModule, Rect>(Modules.Count);
			foreach (CGModule module in Modules)
			{
				dictionary[module] = module.Properties.Dimensions;
			}
			List<CGModule> list = Modules.Where((CGModule m) => !m.OutputLinks.Any()).ToList();
			Dictionary<CGModule, HashSet<CGModule>> dictionary2 = new Dictionary<CGModule, HashSet<CGModule>>(Modules.Count);
			foreach (CGModule item in list)
			{
				UpdateModulesRecursiveInputs(dictionary2, item);
			}
			HashSet<int> hashSet = new HashSet<int>();
			for (int i = 0; i < list.Count; i++)
			{
				float y = ((i == 0) ? 0f : (dictionary2[list[i - 1]].Max((CGModule m) => m.Properties.Dimensions.yMax) + 20f));
				CGModule cGModule = list[i];
				cGModule.Properties.Dimensions.position = new Vector2(0f, y);
				hashSet.Add(cGModule.UniqueID);
				ReorderEndpointRecursiveInputs(cGModule, hashSet, dictionary2);
			}
			ArrangeModules();
		}

		public void Clear()
		{
			if (DTUtility.IsInEditMode)
			{
				return;
			}
			for (int num = Modules.Count - 1; num >= 0; num--)
			{
				if (!Modules[num].gameObject.Destroy(isUndoable: true, doPrefabCheck: false))
				{
					UnityEngine.Debug.LogError("Could not destroy a CG module. This is not expected. Please send a bug report.");
				}
			}
			Modules.Clear();
		}

		public void DeleteModule(CGModule module)
		{
			if ((bool)module)
			{
				module.Delete();
			}
		}

		[UsedImplicitly]
		[Obsolete("Use the overload that has a mandatory includeOnRequestProcessing parameter")]
		public List<T> FindModules<T>() where T : CGModule
		{
			return FindModules<T>(includeOnRequestProcessing: false);
		}

		public List<T> FindModules<T>(bool includeOnRequestProcessing) where T : CGModule
		{
			List<T> list = new List<T>();
			for (int i = 0; i < Modules.Count; i++)
			{
				if (Modules[i] is T && (includeOnRequestProcessing || !(Modules[i] is IOnRequestProcessing)))
				{
					list.Add((T)Modules[i]);
				}
			}
			return list;
		}

		[UsedImplicitly]
		[Obsolete("Use the overload that has a mandatory includeOnRequestProcessing parameter")]
		public List<CGModule> GetModules()
		{
			return GetModules(includeOnRequestProcessing: false);
		}

		[UsedImplicitly]
		[Obsolete("Method will be removed. You can copy its implementation if needed.")]
		public List<CGModule> GetModules(bool includeOnRequestProcessing)
		{
			if (includeOnRequestProcessing)
			{
				return new List<CGModule>(Modules);
			}
			return Modules.Where((CGModule t) => !(t is IOnRequestProcessing)).ToList();
		}

		[UsedImplicitly]
		[Obsolete("Use the overload that has a mandatory includeOnRequestProcessing parameter")]
		public CGModule GetModule(int moduleID)
		{
			return GetModule(moduleID, includeOnRequestProcessing: false);
		}

		[CanBeNull]
		public CGModule GetModule(int moduleID, bool includeOnRequestProcessing)
		{
			CGModule cGModule = Modules.FirstOrDefault((CGModule m) => m.UniqueID == moduleID);
			if (cGModule == null)
			{
				return null;
			}
			if (includeOnRequestProcessing || !(cGModule is IOnRequestProcessing))
			{
				return cGModule;
			}
			return null;
		}

		[UsedImplicitly]
		[Obsolete("Use the overload that has a mandatory includeOnRequestProcessing parameter")]
		public T GetModule<T>(int moduleID) where T : CGModule
		{
			return GetModule<T>(moduleID, includeOnRequestProcessing: false);
		}

		public T GetModule<T>(int moduleID, bool includeOnRequestProcessing) where T : CGModule
		{
			return GetModule(moduleID, includeOnRequestProcessing) as T;
		}

		[UsedImplicitly]
		[Obsolete("Use the overload that has a mandatory includeOnRequestProcessing parameter")]
		public CGModule GetModule(string moduleName)
		{
			return GetModule(moduleName, includeOnRequestProcessing: false);
		}

		public CGModule GetModule(string moduleName, bool includeOnRequestProcessing)
		{
			for (int i = 0; i < Modules.Count; i++)
			{
				if (Modules[i].ModuleName.Equals(moduleName, StringComparison.CurrentCultureIgnoreCase) && (includeOnRequestProcessing || !(Modules[i] is IOnRequestProcessing)))
				{
					return Modules[i];
				}
			}
			return null;
		}

		[UsedImplicitly]
		[Obsolete("Use the overload that has a mandatory includeOnRequestProcessing parameter")]
		public T GetModule<T>(string moduleName) where T : CGModule
		{
			return GetModule<T>(moduleName, includeOnRequestProcessing: false);
		}

		public T GetModule<T>(string moduleName, bool includeOnRequestProcessing) where T : CGModule
		{
			return GetModule(moduleName, includeOnRequestProcessing) as T;
		}

		[UsedImplicitly]
		[Obsolete("Use GetModule and CGModule.GetOutputSlot instead")]
		public CGModuleOutputSlot GetModuleOutputSlot(int moduleId, string slotName)
		{
			CGModule module = GetModule(moduleId, includeOnRequestProcessing: true);
			if ((bool)module)
			{
				return module.GetOutputSlot(slotName);
			}
			return null;
		}

		[UsedImplicitly]
		[Obsolete("Use GetModule and CGModule.GetOutputSlot instead")]
		public CGModuleOutputSlot GetModuleOutputSlot(string moduleName, string slotName)
		{
			CGModule module = GetModule(moduleName, includeOnRequestProcessing: true);
			if ((bool)module)
			{
				return module.GetOutputSlot(slotName);
			}
			return null;
		}

		public void Initialize(bool force = false)
		{
			if (this == null)
			{
				return;
			}
			if (!isInitializedPhaseOne || force)
			{
				SetModulesFromChildren();
				if (CorrectDuplicateModuleIDs())
				{
					ResetAllModuleLinks();
				}
				foreach (CGModule module in Modules)
				{
					if (force || !module.IsInitialized)
					{
						module.Initialize();
					}
				}
				isInitializedPhaseOne = true;
			}
			for (int i = 0; i < Modules.Count; i++)
			{
				if (Modules[i] is IExternalInput && !Modules[i].IsInitialized)
				{
					return;
				}
			}
			isInitialized = true;
			isInitializedPhaseOne = false;
			if (force)
			{
				moduleSorter.SortingNeeded = true;
			}
			Refresh(forceUpdate: true);
		}

		public void Refresh(bool forceUpdate = false)
		{
			if (!IsInitialized)
			{
				return;
			}
			moduleSorter.EnsureIsSorted(Modules);
			CGModule cGModule = null;
			for (int i = 0; i < Modules.Count; i++)
			{
				CGModule cGModule2 = Modules[i];
				if (cGModule2 is IOnRequestProcessing)
				{
					if (forceUpdate)
					{
						cGModule2.Dirty = true;
					}
				}
				else
				{
					if (cGModule2 is INoProcessing || (!cGModule2.Dirty && !forceUpdate))
					{
						continue;
					}
					cGModule2.checkOnStateChangedINTERNAL();
					if (cGModule2.IsInitialized && cGModule2.IsConfigured)
					{
						if (cGModule == null)
						{
							cGModule = cGModule2;
						}
						cGModule2.doRefresh();
					}
				}
			}
			if (cGModule != null)
			{
				OnRefreshEvent(new CurvyCGEventArgs(this, cGModule));
			}
		}

		public void TryAutoRefresh()
		{
			if (AutoRefresh && autoRefreshTimer.Update((float)RefreshDelay * 0.001f, (float)RefreshDelayEditor * 0.001f))
			{
				Refresh();
			}
		}

		public bool DeleteAllOutputManagedResources(out bool associatedPrefabWasModified)
		{
			associatedPrefabWasModified = false;
			bool flag = false;
			foreach (CGModule module in Modules)
			{
				flag |= module.DeleteAllOutputManagedResources();
			}
			return flag;
		}

		public string GetModuleUniqueName(CGModule module)
		{
			int num = 1;
			string text = module.ModuleName;
			while (!IsModuleNameUnique(module, text))
			{
				text = module.ModuleName + num;
				num++;
			}
			return text;
		}

		public int GetModuleUniqueID(CGModule module)
		{
			int id;
			for (id = 0; Modules.Exists((CGModule m) => (object)m != module && m.UniqueID == id); id++)
			{
			}
			return id;
		}

		protected CurvyCGEventArgs OnRefreshEvent(CurvyCGEventArgs e)
		{
			if (OnRefresh != null)
			{
				OnRefresh.Invoke(e);
			}
			return e;
		}

		protected override void ResetOnEnable()
		{
			base.ResetOnEnable();
			autoRefreshTimer.Reset();
			moduleSorter.SortingNeeded = true;
		}

		private bool IsModuleNameUnique(CGModule module, string uniqueName)
		{
			return Modules.All((CGModule m) => (object)m == module || !m.ModuleName.Equals(uniqueName, StringComparison.CurrentCultureIgnoreCase));
		}

		[UsedImplicitly]
		[Obsolete]
		public string getUniqueModuleNameINTERNAL(string name)
		{
			string text = name;
			int num = 1;
			bool flag;
			do
			{
				flag = true;
				foreach (CGModule module in Modules)
				{
					if (module.ModuleName.Equals(text, StringComparison.CurrentCultureIgnoreCase))
					{
						text = name + num++.ToString(CultureInfo.InvariantCulture);
						flag = false;
						break;
					}
				}
			}
			while (!flag);
			return text;
		}

		internal void sortModulesINTERNAL()
		{
			moduleSorter.SortingNeeded = true;
		}

		private bool CorrectDuplicateModuleIDs()
		{
			bool result = false;
			foreach (IGrouping<int, CGModule> item in from module in Modules
				group module by module.UniqueID)
			{
				if (item.Count() <= 1)
				{
					continue;
				}
				result = true;
				DTLog.LogError("[Curvy] Curvy Generator " + base.name + ": The following modules have the same ID. This is not allowed. Their IDs will be reset:");
				foreach (CGModule item2 in item)
				{
					DTLog.LogError($"[Curvy] Curvy Generator {base.name}: Module {item2.ModuleName} with ID {item2.UniqueID}.");
					item2.UniqueID = GetModuleUniqueID(item2);
				}
				DTLog.LogError("[Curvy] Consequently all links were reset. Please raise a bug report if you encounter this error.");
			}
			return result;
		}

		[UsedImplicitly]
		private void ResetAllModuleLinks()
		{
			Modules.ForEach(delegate(CGModule m)
			{
				m.InputLinks.Clear();
				m.OutputLinks.Clear();
				m.ReInitializeLinkedSlots();
			});
		}

		public bool HasCircularReference([NotNull] CGModule module)
		{
			return moduleSorter.HasCircularReference(module);
		}

		private static void ReorderEndpointRecursiveInputs(CGModule endPoint, HashSet<int> reordredModuleIds, Dictionary<CGModule, HashSet<CGModule>> modulesRecursiveInputs)
		{
			float num = endPoint.Properties.Dimensions.xMin - 50f;
			float num2 = endPoint.Properties.Dimensions.yMin;
			foreach (CGModule item in endPoint.Input.SelectMany((CGModuleInputSlot i) => i.GetLinkedModules()).ToList())
			{
				float num3 = num - item.Properties.Dimensions.width;
				if (!reordredModuleIds.Contains(item.UniqueID))
				{
					item.Properties.Dimensions.position = new Vector2(num3, num2);
					reordredModuleIds.Add(item.UniqueID);
					ReorderEndpointRecursiveInputs(item, reordredModuleIds, modulesRecursiveInputs);
				}
				else if (num3 < item.Properties.Dimensions.xMin)
				{
					item.Properties.Dimensions.position = new Vector2(num3, item.Properties.Dimensions.yMin);
					ReorderEndpointRecursiveInputs(item, reordredModuleIds, modulesRecursiveInputs);
				}
				num2 = Math.Max(num2, modulesRecursiveInputs[item].Max((CGModule m) => m.Properties.Dimensions.yMax) + 20f);
			}
		}

		private static HashSet<CGModule> UpdateModulesRecursiveInputs(Dictionary<CGModule, HashSet<CGModule>> modulesRecursiveInputs, CGModule moduleToAdd)
		{
			if (modulesRecursiveInputs.ContainsKey(moduleToAdd))
			{
				return modulesRecursiveInputs[moduleToAdd];
			}
			List<CGModule> source = moduleToAdd.Input.SelectMany((CGModuleInputSlot i) => i.GetLinkedModules()).ToList();
			HashSet<CGModule> hashSet = new HashSet<CGModule> { moduleToAdd };
			hashSet.UnionWith(source.SelectMany((CGModule i) => UpdateModulesRecursiveInputs(modulesRecursiveInputs, i)));
			modulesRecursiveInputs[moduleToAdd] = hashSet;
			return hashSet;
		}

		private void SetModulesFromChildren()
		{
			Modules.Clear();
			GetComponentsInChildren(Modules);
			Modules.RemoveAll((CGModule m) => m.transform.parent != base.transform);
			if (Modules.Any())
			{
				moduleSorter.SortingNeeded = true;
			}
		}

		public bool DeleteAllOutputManagedResourcesFromAssociatedPrefab()
		{
			return false;
		}

		public void SaveAllOutputManagedResources()
		{
			GameObject result = new GameObject(base.name + " Exported Resources");
			result.transform.position = base.transform.position;
			result.transform.rotation = base.transform.rotation;
			result.transform.localScale = base.transform.localScale;
			Modules.Where((CGModule m) => m is ResourceExportingModule).ForEach(delegate(CGModule m)
			{
				((ResourceExportingModule)m).SaveToScene(result.transform);
			});
		}
	}
}
