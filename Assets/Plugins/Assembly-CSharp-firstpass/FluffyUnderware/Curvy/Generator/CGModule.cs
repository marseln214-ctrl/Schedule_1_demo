using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using FluffyUnderware.DevTools;
using FluffyUnderware.DevTools.Extensions;
using JetBrains.Annotations;
using UnityEngine;

namespace FluffyUnderware.Curvy.Generator
{
	[ExecuteAlways]
	public abstract class CGModule : DTVersionedMonoBehaviour
	{
		private class DirtinessManager
		{
			[NotNull]
			private readonly CGModule module;

			private bool isDirty = true;

			private bool isStateChangeDirty;

			private bool lastIsConfiguredState;

			private static readonly Action<CGModule> SetDirtyAction = delegate(CGModule m)
			{
				m.Dirty = true;
			};

			private static readonly Action<CGModule> SetTreeDirtyStateChangeAction = delegate(CGModule m)
			{
				m.dirtinessManager.SetTreeDirtyStateChange();
			};

			public bool IsDirty
			{
				get
				{
					return isDirty;
				}
				set
				{
					isDirty = value;
					if (isDirty)
					{
						bool isConfigured = module.IsConfigured;
						if (lastIsConfiguredState != isConfigured)
						{
							isStateChangeDirty = true;
						}
						lastIsConfiguredState = isConfigured;
						ForEachValidOutputModule(SetDirtyAction);
					}
					if (module is IOnRequestProcessing || module is INoProcessing)
					{
						isDirty = false;
						module.slots.ResetLasRequestedParameters();
					}
				}
			}

			public DirtinessManager([NotNull] CGModule module)
			{
				this.module = module;
			}

			public void UnsetDirtyFlag()
			{
				isDirty = false;
			}

			public void Reset()
			{
				isDirty = true;
				isStateChangeDirty = false;
				lastIsConfiguredState = false;
			}

			public void CheckOnStateChanged()
			{
				if (isStateChangeDirty)
				{
					module.OnStateChange();
				}
				isStateChangeDirty = false;
			}

			public void OnDestroy()
			{
				SetTreeDirtyStateChange();
			}

			private void SetTreeDirtyStateChange()
			{
				isStateChangeDirty = true;
				ForEachValidOutputModule(SetTreeDirtyStateChangeAction);
			}

			private void ForEachValidOutputModule(Action<CGModule> action)
			{
				List<CGModuleOutputSlot> output = module.Output;
				for (int i = 0; i < output.Count; i++)
				{
					CGModuleOutputSlot cGModuleOutputSlot = output[i];
					if (!cGModuleOutputSlot.IsLinked)
					{
						continue;
					}
					List<CGModule> linkedModules = cGModuleOutputSlot.GetLinkedModules();
					for (int j = 0; j < linkedModules.Count; j++)
					{
						CGModule cGModule = linkedModules[j];
						if (cGModule != module || cGModule.Generator.HasCircularReference(cGModule))
						{
							action(cGModule);
						}
					}
				}
			}
		}

		private class Identifier
		{
			[NotNull]
			private readonly CGModule module;

			[CanBeNull]
			private string cachedStringID;

			public int ID
			{
				get
				{
					return module.m_UniqueID;
				}
				set
				{
					module.m_UniqueID = value;
					cachedStringID = null;
				}
			}

			[NotNull]
			public string StringID
			{
				get
				{
					if (cachedStringID == null)
					{
						cachedStringID = ID.ToString(CultureInfo.InvariantCulture);
					}
					return cachedStringID;
				}
			}

			public Identifier([NotNull] CGModule module)
			{
				this.module = module;
			}

			public void Reset()
			{
				cachedStringID = null;
			}
		}

		private class InformationProvider
		{
			[NotNull]
			private readonly CGModule module;

			private ModuleInfoAttribute moduleInformation;

			[CanBeNull]
			public ModuleInfoAttribute Information
			{
				get
				{
					if (moduleInformation == null)
					{
						moduleInformation = GetInformation();
					}
					return moduleInformation;
				}
			}

			public InformationProvider([NotNull] CGModule module)
			{
				this.module = module;
			}

			[CanBeNull]
			private ModuleInfoAttribute GetInformation()
			{
				object[] customAttributes = module.GetType().GetCustomAttributes(typeof(ModuleInfoAttribute), inherit: true);
				if (customAttributes.Length == 0)
				{
					return null;
				}
				return (ModuleInfoAttribute)customAttributes[0];
			}
		}

		private class ResourceNamer
		{
			private readonly CGModule cgModule;

			private readonly Dictionary<string, Dictionary<int, string>> resourcesNameCache = new Dictionary<string, Dictionary<int, string>>();

			public ResourceNamer(CGModule cgModule)
			{
				this.cgModule = cgModule;
			}

			public void ClearCache()
			{
				resourcesNameCache.Clear();
			}

			[NotNull]
			private string GetResourceName([NotNull] string resourceName, int index)
			{
				if (!resourcesNameCache.TryGetValue(resourceName, out var value))
				{
					value = (resourcesNameCache[resourceName] = new Dictionary<int, string>());
				}
				if (!value.TryGetValue(index, out var value2))
				{
					value2 = (value[index] = ((index > -1) ? string.Format(CultureInfo.InvariantCulture, "{0}_{1}_{2}{3:000}", cgModule.ModuleName, cgModule.identifier.StringID, resourceName, index) : string.Format(CultureInfo.InvariantCulture, "{0}_{1}_{2}", cgModule.ModuleName, cgModule.identifier.StringID, resourceName)));
				}
				return value2;
			}

			public void Rename([NotNull] string resourceName, [NotNull] Component resource, int index)
			{
				string resourceName2 = GetResourceName(resourceName, index);
				if (resource.name != resourceName2)
				{
					resource.name = resourceName2;
				}
			}
		}

		private class Slots
		{
			[NotNull]
			private readonly CGModule module;

			[NotNull]
			public Dictionary<string, CGModuleInputSlot> InputSlotsByName { get; } = new Dictionary<string, CGModuleInputSlot>();


			[NotNull]
			public Dictionary<string, CGModuleOutputSlot> OutputSlotsByName { get; } = new Dictionary<string, CGModuleOutputSlot>();


			[NotNull]
			public List<CGModuleInputSlot> InputSlots { get; } = new List<CGModuleInputSlot>();


			[NotNull]
			public List<CGModuleOutputSlot> OutputSlots { get; } = new List<CGModuleOutputSlot>();


			public bool IsConfigured
			{
				get
				{
					int num = 0;
					foreach (CGModuleInputSlot inputSlot in InputSlots)
					{
						InputSlotInfo inputInfo = inputSlot.InputInfo;
						if (inputSlot.IsLinked)
						{
							for (int i = 0; i < inputSlot.Count; i++)
							{
								if (inputSlot.SourceSlot(i) != null)
								{
									if (inputSlot.SourceSlot(i).Module.IsConfigured)
									{
										num++;
									}
									else if (!inputInfo.Optional)
									{
										return false;
									}
								}
							}
						}
						else if (inputInfo == null || !inputInfo.Optional)
						{
							return false;
						}
					}
					if (num <= 0)
					{
						return InputSlots.Count == 0;
					}
					return true;
				}
			}

			public Slots([NotNull] CGModule module)
			{
				this.module = module;
				Setup();
			}

			private void Setup()
			{
				FieldInfo[] allFields = module.GetType().GetAllFields();
				foreach (FieldInfo fieldInfo in allFields)
				{
					CGModuleSlot slot = GetSlot(fieldInfo);
					if (slot != null)
					{
						slot.Module = module;
						slot.SetInfoFromField(fieldInfo);
						Store(slot);
					}
				}
			}

			[CanBeNull]
			private CGModuleSlot GetSlot([NotNull] FieldInfo fieldInfo)
			{
				if (fieldInfo.FieldType == typeof(CGModuleInputSlot))
				{
					return (CGModuleInputSlot)fieldInfo.GetValue(module);
				}
				if (fieldInfo.FieldType == typeof(CGModuleOutputSlot))
				{
					return (CGModuleOutputSlot)fieldInfo.GetValue(module);
				}
				return null;
			}

			private void Store([NotNull] CGModuleSlot slot)
			{
				if (!(slot is CGModuleInputSlot cGModuleInputSlot))
				{
					if (slot is CGModuleOutputSlot cGModuleOutputSlot)
					{
						OutputSlotsByName.Add(cGModuleOutputSlot.Info.Name, cGModuleOutputSlot);
						OutputSlots.Add(cGModuleOutputSlot);
					}
				}
				else
				{
					InputSlotsByName.Add(cGModuleInputSlot.Info.Name, cGModuleInputSlot);
					InputSlots.Add(cGModuleInputSlot);
				}
			}

			public void ReinitializeLinkedModulesLinkedSlots()
			{
				foreach (CGModuleInputSlot inputSlot in InputSlots)
				{
					ReinitializeLinkedModulesLinkedSlots(inputSlot);
				}
				foreach (CGModuleOutputSlot outputSlot in OutputSlots)
				{
					ReinitializeLinkedModulesLinkedSlots(outputSlot);
				}
			}

			private static void ReinitializeLinkedModulesLinkedSlots([NotNull] CGModuleSlot slot)
			{
				List<CGModule> linkedModules = slot.GetLinkedModules();
				for (int i = 0; i < linkedModules.Count; i++)
				{
					CGModule cGModule = linkedModules[i];
					if (cGModule != null)
					{
						cGModule.slots.ReInitializeLinkedSlots();
					}
				}
			}

			public void ReInitializeLinkedSlots()
			{
				foreach (CGModuleInputSlot inputSlot in InputSlots)
				{
					inputSlot.ReInitializeLinkedSlots();
				}
				foreach (CGModuleOutputSlot outputSlot in OutputSlots)
				{
					outputSlot.ReInitializeLinkedSlots();
				}
			}

			[System.Diagnostics.Conditional("UNITY_EDITOR")]
			public void ResetInputSlotsLastDataCount()
			{
			}

			public void ResetLasRequestedParameters()
			{
				foreach (CGModuleOutputSlot outputSlot in OutputSlots)
				{
					outputSlot.LastRequestParameters = null;
				}
			}

			public void ClearOutputData()
			{
				foreach (CGModuleOutputSlot outputSlot in OutputSlots)
				{
					outputSlot.ClearData();
				}
			}

			public CGModuleInputSlot GetInputSlot(string name)
			{
				if (!InputSlotsByName.ContainsKey(name))
				{
					return null;
				}
				return InputSlotsByName[name];
			}

			public CGModuleOutputSlot GetOutputSlot(string name)
			{
				if (!OutputSlotsByName.ContainsKey(name))
				{
					return null;
				}
				return OutputSlotsByName[name];
			}

			public void CheckInputModulesNotDirty()
			{
				foreach (CGModuleInputSlot inputSlot in InputSlots)
				{
					foreach (CGModuleSlot linkedSlot in inputSlot.LinkedSlots)
					{
						if (linkedSlot.Module.IsConfigured && linkedSlot.Module.Dirty)
						{
							DTLog.LogError($"[Curvy] Getting data from a dirty module. This shouldn't happen at all. Please raise a bug report. Source module is {linkedSlot.Module}", module);
						}
					}
				}
			}
		}

		[Group("Events", Expanded = false, Sort = 1000)]
		[SerializeField]
		protected CurvyCGEvent m_OnBeforeRefresh = new CurvyCGEvent();

		[Group("Events")]
		[SerializeField]
		protected CurvyCGEvent m_OnRefresh = new CurvyCGEvent();

		[SerializeField]
		[HideInInspector]
		private string m_ModuleName;

		[SerializeField]
		[HideInInspector]
		private bool m_Active = true;

		[Group("Seed Options", Expanded = false, Sort = 1001)]
		[GroupCondition("UsesRandom")]
		[FieldAction("CBSeedOptions", ActionAttribute.ActionEnum.Callback, ShowBelowProperty = true)]
		[SerializeField]
		private bool m_RandomizeSeed;

		[SerializeField]
		[HideInInspector]
		private int m_Seed = (int)DateTime.Now.Ticks;

		[SerializeField]
		[HideInInspector]
		private int m_UniqueID;

		private CurvyGenerator generator;

		private bool isInitialized;

		[NotNull]
		private readonly ResourceNamer resourceNamer;

		[NotNull]
		private readonly InformationProvider informationProvider;

		[NotNull]
		private readonly DirtinessManager dirtinessManager;

		[NotNull]
		private readonly Slots slots;

		[NotNull]
		private readonly Identifier identifier;

		[NotNull]
		private readonly List<(Component ResourceManager, string ResourceName)> resourceManagers;

		[NonSerialized]
		public List<string> UIMessages = new List<string>();

		[HideInInspector]
		public CGModuleProperties Properties = new CGModuleProperties();

		[HideInInspector]
		public List<CGModuleLink> InputLinks = new List<CGModuleLink>();

		[HideInInspector]
		public List<CGModuleLink> OutputLinks = new List<CGModuleLink>();

		[UsedImplicitly]
		[Obsolete]
		internal int SortAncestors;

		public CurvyCGEvent OnBeforeRefresh
		{
			get
			{
				return m_OnBeforeRefresh;
			}
			set
			{
				m_OnBeforeRefresh = value;
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

		public string ModuleName
		{
			get
			{
				return base.name;
			}
			set
			{
				if (base.name != value)
				{
					base.name = value;
					resourceNamer.ClearCache();
					renameManagedResourcesINTERNAL();
				}
			}
		}

		public bool Active
		{
			get
			{
				return m_Active;
			}
			set
			{
				if (m_Active != value)
				{
					m_Active = value;
					Dirty = true;
					Generator.sortModulesINTERNAL();
				}
			}
		}

		public int Seed
		{
			get
			{
				return m_Seed;
			}
			set
			{
				if (m_Seed != value)
				{
					m_Seed = value;
					Dirty = true;
				}
			}
		}

		public bool RandomizeSeed
		{
			get
			{
				return m_RandomizeSeed;
			}
			set
			{
				m_RandomizeSeed = value;
			}
		}

		public bool Dirty
		{
			get
			{
				return dirtinessManager.IsDirty;
			}
			set
			{
				dirtinessManager.IsDirty = value;
			}
		}

		public virtual bool IsConfigured
		{
			get
			{
				if (!IsInitialized || Generator.HasCircularReference(this) || !Active)
				{
					return false;
				}
				return slots.IsConfigured;
			}
		}

		public virtual bool IsInitialized => isInitialized;

		public CurvyGenerator Generator
		{
			get
			{
				if (!generator)
				{
					generator = ((base.transform.parent != null) ? base.transform.parent.GetComponent<CurvyGenerator>() : null);
				}
				return generator;
			}
		}

		public int UniqueID
		{
			get
			{
				return identifier.ID;
			}
			set
			{
				if (identifier.ID != value)
				{
					identifier.ID = value;
					resourceNamer.ClearCache();
					renameManagedResourcesINTERNAL();
				}
			}
		}

		[UsedImplicitly]
		[Obsolete("Use Generator.HasCircularReference instead")]
		public bool CircularReferenceError
		{
			get
			{
				return Generator.HasCircularReference(this);
			}
			set
			{
				throw new NotSupportedException(" CircularReferenceError is read-only");
			}
		}

		[NotNull]
		public Dictionary<string, CGModuleInputSlot> InputByName => slots.InputSlotsByName;

		[NotNull]
		public Dictionary<string, CGModuleOutputSlot> OutputByName => slots.OutputSlotsByName;

		[NotNull]
		public List<CGModuleInputSlot> Input => slots.InputSlots;

		[NotNull]
		public List<CGModuleOutputSlot> Output => slots.OutputSlots;

		[UsedImplicitly]
		[Obsolete]
		[CanBeNull]
		public ModuleInfoAttribute Info => informationProvider.Information;

		protected CurvyCGEventArgs OnBeforeRefreshEvent(CurvyCGEventArgs e)
		{
			if (OnBeforeRefresh != null)
			{
				OnBeforeRefresh.Invoke(e);
			}
			return e;
		}

		protected CurvyCGEventArgs OnRefreshEvent(CurvyCGEventArgs e)
		{
			if (OnRefresh != null)
			{
				OnRefresh.Invoke(e);
			}
			return e;
		}

		protected virtual void Awake()
		{
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			if ((bool)Generator)
			{
				Initialize();
			}
		}

		protected virtual void OnDestroy()
		{
			dirtinessManager.OnDestroy();
			if (GetManagedResources(out var components, out var resourceNames))
			{
				for (int num = components.Count - 1; num >= 0; num--)
				{
					DeleteManagedResource(resourceNames[num], components[num], string.Empty, dontUsePool: true);
				}
			}
			slots.ReinitializeLinkedModulesLinkedSlots();
			if ((bool)Generator)
			{
				Generator.RemoveModule(this);
			}
			isInitialized = false;
		}

		protected override void OnValidate()
		{
			base.OnValidate();
			Dirty = true;
			resourceNamer.ClearCache();
			renameManagedResourcesINTERNAL();
		}

		[UsedImplicitly]
		private void Update()
		{
		}

		public new virtual void Reset()
		{
			ModuleName = (string.IsNullOrEmpty(Info.ModuleName) ? GetType().Name : Info.ModuleName);
			if ((bool)Generator && Generator.Modules.Any((CGModule m) => m != this && m.UniqueID == UniqueID))
			{
				UniqueID = Generator.GetModuleUniqueID(this);
			}
			if (OnBeforeRefresh != null)
			{
				OnBeforeRefresh.RemoveAllListeners();
			}
			if (OnRefresh != null)
			{
				OnRefresh.RemoveAllListeners();
			}
			OnBeforeRefresh = new CurvyCGEvent();
			OnRefresh = new CurvyCGEvent();
			DeleteAllOutputManagedResources();
			base.Reset();
		}

		public virtual void Refresh()
		{
			slots.CheckInputModulesNotDirty();
			UIMessages.Clear();
		}

		public virtual bool DeleteAllOutputManagedResources()
		{
			return false;
		}

		public virtual void OnStateChange()
		{
			Dirty = true;
			slots.ClearOutputData();
			if (!IsConfigured)
			{
				DeleteAllOutputManagedResources();
			}
		}

		public virtual void OnTemplateCreated()
		{
			DeleteAllOutputManagedResources();
		}

		protected static T GetRequestParameter<T>(ref CGDataRequestParameter[] requests) where T : CGDataRequestParameter
		{
			for (int i = 0; i < requests.Length; i++)
			{
				if (requests[i] is T)
				{
					return (T)requests[i];
				}
			}
			return null;
		}

		protected static void RemoveRequestParameter(ref CGDataRequestParameter[] requests, CGDataRequestParameter request)
		{
			for (int i = 0; i < requests.Length; i++)
			{
				if (requests[i] == request)
				{
					requests = requests.RemoveAt(i);
					break;
				}
			}
		}

		public CGModule()
		{
			resourceNamer = new ResourceNamer(this);
			dirtinessManager = new DirtinessManager(this);
			identifier = new Identifier(this);
			informationProvider = new InformationProvider(this);
			slots = new Slots(this);
			resourceManagers = GetResourceManagers();
		}

		public void Initialize()
		{
			if (!Generator)
			{
				Invoke("Delete", 0f);
				return;
			}
			if (string.IsNullOrEmpty(ModuleName))
			{
				SetModuleName();
			}
			slots.ReInitializeLinkedSlots();
			isInitialized = true;
		}

		[CanBeNull]
		public CGModuleLink GetOutputLink(CGModuleOutputSlot outputSlot, CGModuleInputSlot inputSlot)
		{
			return GetLink(OutputLinks, outputSlot, inputSlot);
		}

		[NotNull]
		public List<CGModuleLink> GetOutputLinks(CGModuleOutputSlot outputSlot)
		{
			return GetLinks(OutputLinks, outputSlot);
		}

		[CanBeNull]
		public CGModuleLink GetInputLink(CGModuleInputSlot inputSlot, CGModuleOutputSlot outputSlot)
		{
			return GetLink(InputLinks, inputSlot, outputSlot);
		}

		[NotNull]
		public List<CGModuleLink> GetInputLinks(CGModuleInputSlot inputSlot)
		{
			return GetLinks(InputLinks, inputSlot);
		}

		[UsedImplicitly]
		[Obsolete("Use ComponentExt.DuplicateGameObject and CurvyGenerator.AddModule to duplicate the module then add it to the generator ")]
		public CGModule CopyTo(CurvyGenerator targetGenerator)
		{
			if (this == null)
			{
				throw new InvalidOperationException("[Curvy] Trying to copy an already deleted module");
			}
			CGModule cGModule = this.DuplicateGameObject<CGModule>(targetGenerator.transform);
			cGModule.name = base.name;
			targetGenerator.AddModule(cGModule);
			return cGModule;
		}

		public Component AddManagedResource([NotNull] string resourceName, string context = "", int index = -1)
		{
			Component component = CGResourceHandler.CreateResource(this, resourceName, context);
			RenameResource((context == "") ? resourceName : (resourceName + context), component, index);
			component.transform.SetParent(base.transform);
			return component;
		}

		public void DeleteManagedResource(string resourceName, Component res, [NotNull] string context = "", bool dontUsePool = false)
		{
			if ((bool)res)
			{
				CGResourceHandler.DestroyResource(this, resourceName, res, context, dontUsePool);
			}
		}

		public bool IsManagedResource(Component res)
		{
			if ((bool)res)
			{
				return res.transform.parent == base.transform;
			}
			return false;
		}

		public List<IPool> GetAllPrefabPools()
		{
			return Generator.PoolManager.FindPools(identifier.StringID + "_");
		}

		public void DeleteAllPrefabPools()
		{
			Generator.PoolManager.DeletePools(identifier.StringID + "_");
		}

		public void Delete()
		{
			OnStateChange();
			base.gameObject.Destroy(isUndoable: true, doPrefabCheck: true);
		}

		public CGModuleInputSlot GetInputSlot(string name)
		{
			return slots.GetInputSlot(name);
		}

		public CGModuleOutputSlot GetOutputSlot(string name)
		{
			return slots.GetOutputSlot(name);
		}

		public bool GetManagedResources(out List<Component> components, out List<string> resourceNames)
		{
			components = new List<Component>();
			resourceNames = new List<string>();
			FieldInfo[] allFields = GetType().GetAllFields(includeInherited: false, includePrivate: true);
			foreach (FieldInfo fieldInfo in allFields)
			{
				CGResourceManagerAttribute customAttribute = fieldInfo.GetCustomAttribute<CGResourceManagerAttribute>();
				if (customAttribute == null)
				{
					continue;
				}
				if (typeof(ICGResourceCollection).IsAssignableFrom(fieldInfo.FieldType))
				{
					if (!(fieldInfo.GetValue(this) is ICGResourceCollection { ItemsArray: var itemsArray }))
					{
						continue;
					}
					foreach (Component component in itemsArray)
					{
						if ((bool)component && component.transform.parent == base.transform)
						{
							components.Add(component);
							resourceNames.Add(customAttribute.ResourceName);
						}
					}
				}
				else
				{
					Component component2 = fieldInfo.GetValue(this) as Component;
					if ((bool)component2 && component2.transform.parent == base.transform)
					{
						components.Add(component2);
						resourceNames.Add(customAttribute.ResourceName);
					}
				}
			}
			return components.Count > 0;
		}

		private void SetModuleName()
		{
			string moduleName = Info.ModuleName;
			string moduleName2 = (string.IsNullOrEmpty(moduleName) ? Info.MenuName.Substring(Info.MenuName.LastIndexOf("/", StringComparison.Ordinal) + 1) : moduleName);
			ModuleName = moduleName2;
			ModuleName = Generator.GetModuleUniqueName(this);
		}

		protected void RenameResource([NotNull] string resourceName, Component resource, int index = -1)
		{
			resourceNamer.Rename(resourceName, resource, index);
		}

		[CanBeNull]
		private static CGModuleLink GetLink(List<CGModuleLink> lst, CGModuleSlot source, CGModuleSlot target)
		{
			return lst.FirstOrDefault((CGModuleLink t) => t.IsSame(source, target));
		}

		[NotNull]
		private static List<CGModuleLink> GetLinks(List<CGModuleLink> lst, CGModuleSlot source)
		{
			return lst.Where((CGModuleLink t) => t.IsFrom(source)).ToList();
		}

		protected PrefabPool GetPrefabPool(GameObject prefab)
		{
			return Generator.PoolManager.GetPrefabPool(identifier.StringID + "_" + prefab.name, prefab);
		}

		protected bool TryDeleteChildrenFromAssociatedPrefab()
		{
			return false;
		}

		internal void doRefresh()
		{
			if (RandomizeSeed)
			{
				UnityEngine.Random.InitState((int)DateTime.Now.Ticks);
			}
			else
			{
				UnityEngine.Random.InitState(Seed);
			}
			OnBeforeRefreshEvent(new CurvyCGEventArgs(this));
			Refresh();
			UnityEngine.Random.InitState((int)DateTime.Now.Ticks);
			OnRefreshEvent(new CurvyCGEventArgs(this));
			dirtinessManager.UnsetDirtyFlag();
		}

		public void checkOnStateChangedINTERNAL()
		{
			dirtinessManager.CheckOnStateChanged();
		}

		[NotNull]
		[UsedImplicitly]
		[Obsolete("This does not return all resource managers. Read todo inside and fix it first")]
		private List<(Component ResourceManager, string ResourceName)> GetResourceManagers()
		{
			List<(Component, string)> list = new List<(Component, string)>();
			FieldInfo[] allFields = GetType().GetAllFields(includeInherited: false, includePrivate: true);
			foreach (FieldInfo fieldInfo in allFields)
			{
				CGResourceManagerAttribute customAttribute = fieldInfo.GetCustomAttribute<CGResourceManagerAttribute>();
				if (customAttribute != null && fieldInfo.GetValue(this) is Component item)
				{
					list.Add((item, customAttribute.ResourceName));
				}
			}
			return list;
		}

		protected override void ResetOnEnable()
		{
			base.ResetOnEnable();
			identifier.Reset();
			dirtinessManager.Reset();
			UIMessages.Clear();
			resourceNamer.ClearCache();
		}

		private bool UsesRandom()
		{
			if (Info != null)
			{
				return Info.UsesRandom;
			}
			return false;
		}

		[UsedImplicitly]
		[Obsolete]
		internal void initializeSort()
		{
			SortAncestors = slots.InputSlots.Where((CGModuleInputSlot t) => t.IsLinked).Sum((CGModuleInputSlot t) => t.LinkedSlots.Count);
		}

		[UsedImplicitly]
		[Obsolete]
		internal List<CGModule> decrementChilds()
		{
			foreach (CGModuleSlot item in slots.OutputSlots.SelectMany((CGModuleOutputSlot outputSlot) => outputSlot.LinkedSlots))
			{
				item.Module.SortAncestors--;
			}
			List<CGModule> list = new List<CGModule>();
			foreach (CGModuleOutputSlot outputSlot in slots.OutputSlots)
			{
				list.AddRange(from t in outputSlot.LinkedSlots
					where t.Module.SortAncestors == 0
					select t.Module);
			}
			return list;
		}

		[UsedImplicitly]
		[Obsolete]
		public int SetUniqueIdINTERNAL()
		{
			int id;
			for (id = 0; Generator.Modules.Exists((CGModule m) => (object)m != this && m.UniqueID == id); id++)
			{
			}
			identifier.ID = id;
			return identifier.ID;
		}

		[UsedImplicitly]
		[Obsolete]
		internal ModuleInfoAttribute getInfo()
		{
			object[] customAttributes = GetType().GetCustomAttributes(typeof(ModuleInfoAttribute), inherit: true);
			if (customAttributes.Length == 0)
			{
				return null;
			}
			return (ModuleInfoAttribute)customAttributes[0];
		}

		[UsedImplicitly]
		[Obsolete]
		public void renameManagedResourcesINTERNAL()
		{
			foreach (var (component, resourceName) in resourceManagers)
			{
				if (!(component == null) && !(component.transform.parent != base.transform))
				{
					RenameResource(resourceName, component);
				}
			}
		}

		[UsedImplicitly]
		[Obsolete]
		public void ReInitializeLinkedSlots()
		{
			slots.ReInitializeLinkedSlots();
		}

		[UsedImplicitly]
		[Obsolete("Will be removed. Copy the method's implementation if needed")]
		public List<CGModuleInputSlot> GetInputSlots(Type filterType = null)
		{
			if (filterType == null)
			{
				return new List<CGModuleInputSlot>(Input);
			}
			List<CGModuleInputSlot> list = new List<CGModuleInputSlot>();
			for (int i = 0; i < Output.Count; i++)
			{
				if (Output[i].Info.DataTypes[0] == filterType || Output[i].Info.DataTypes[0].IsSubclassOf(filterType))
				{
					list.Add(Input[i]);
				}
			}
			return list;
		}

		[UsedImplicitly]
		[Obsolete("Will be removed. Copy the method's implementation if needed")]
		public List<CGModuleOutputSlot> GetOutputSlots(Type filterType = null)
		{
			if (filterType == null)
			{
				return new List<CGModuleOutputSlot>(Output);
			}
			List<CGModuleOutputSlot> list = new List<CGModuleOutputSlot>();
			for (int i = 0; i < Output.Count; i++)
			{
				if (Output[i].Info.DataTypes[0] == filterType || Output[i].Info.DataTypes[0].IsSubclassOf(filterType))
				{
					list.Add(Output[i]);
				}
			}
			return list;
		}
	}
}
