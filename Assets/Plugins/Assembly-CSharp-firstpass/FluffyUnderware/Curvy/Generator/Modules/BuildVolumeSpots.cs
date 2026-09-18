using System;
using System.Collections.Generic;
using System.Linq;
using FluffyUnderware.Curvy.Pools;
using FluffyUnderware.Curvy.Utils;
using FluffyUnderware.DevTools;
using FluffyUnderware.DevTools.Extensions;
using JetBrains.Annotations;
using ToolBuddy.Pooling.Collections;
using UnityEngine;

namespace FluffyUnderware.Curvy.Generator.Modules
{
	[ModuleInfo("Build/Volume Spots", ModuleName = "Volume Spots", Description = "Generate spots along a path/volume", UsesRandom = true)]
	[HelpURL("https://curvyeditor.com/doclink/cgvolumespots")]
	public class BuildVolumeSpots : CGModule, ISerializationCallbackReceiver
	{
		private struct EditorData : IEquatable<EditorData>
		{
			public int SpotsCount { get; }

			public bool InputIsAVolume { get; }

			[NotNull]
			[ItemNotNull]
			public string[] BoundsNames { get; }

			public EditorData([NotNull] IReadOnlyList<CGBounds> bounds, bool inputIsAVolume, int spotsCount)
			{
				SpotsCount = spotsCount;
				InputIsAVolume = inputIsAVolume;
				BoundsNames = GetBoundsNames(bounds);
			}

			[Pure]
			[NotNull]
			private static string[] GetBoundsNames([NotNull] IReadOnlyList<CGBounds> bounds)
			{
				return Array.Empty<string>();
			}

			public bool Equals(EditorData other)
			{
				if (SpotsCount == other.SpotsCount && InputIsAVolume == other.InputIsAVolume)
				{
					return BoundsNames.Equals(other.BoundsNames);
				}
				return false;
			}

			public override bool Equals(object obj)
			{
				if (obj is EditorData other)
				{
					return Equals(other);
				}
				return false;
			}

			public override int GetHashCode()
			{
				return (((SpotsCount * 397) ^ InputIsAVolume.GetHashCode()) * 397) ^ ((BoundsNames != null) ? BoundsNames.GetHashCode() : 0);
			}

			public static bool operator ==(EditorData left, EditorData right)
			{
				return left.Equals(right);
			}

			public static bool operator !=(EditorData left, EditorData right)
			{
				return !left.Equals(right);
			}
		}

		private sealed class EndGroupData : IDisposable
		{
			private bool disposed;

			internal CGBoundsGroup BoundsGroup { get; }

			internal SubArray<int> ItemIndices { get; }

			internal float GroupDepth { get; }

			internal CGBounds[] ItemBounds { get; }

			internal float SpaceBefore { get; }

			internal float SpaceAfter { get; }

			internal EndGroupData(CGBoundsGroup boundsGroup, SubArray<int> itemIndices, float groupDepth, CGBounds[] itemBounds, float spaceBefore, float spaceAfter)
			{
				BoundsGroup = boundsGroup;
				ItemIndices = itemIndices;
				GroupDepth = groupDepth;
				ItemBounds = itemBounds;
				SpaceBefore = spaceBefore;
				SpaceAfter = spaceAfter;
			}

			private bool Dispose(bool disposing)
			{
				if (disposed)
				{
					DTLog.LogWarning("[Curvy] Attempt to dispose an EndGroupData twice. Please raise a bug report.");
					return false;
				}
				ArrayPools.Int32.Free(ItemIndices);
				disposed = true;
				return true;
			}

			public void Dispose()
			{
				Dispose(disposing: true);
				GC.SuppressFinalize(this);
			}

			~EndGroupData()
			{
				Dispose(disposing: false);
			}
		}

		private const int MinCrossBase = -1;

		private const int MaxCrossBase = 1;

		private const int MinRange = 0;

		private const int MaxRange = 1;

		[HideInInspector]
		[InputSlotInfo(new Type[] { typeof(CGPath) }, Name = "Path/Volume", DisplayName = "Volume/Rasterized Path")]
		public CGModuleInputSlot InPath = new CGModuleInputSlot();

		[HideInInspector]
		[InputSlotInfo(new Type[] { typeof(CGBounds) }, Array = true)]
		public CGModuleInputSlot InBounds = new CGModuleInputSlot();

		[HideInInspector]
		[OutputSlotInfo(typeof(CGSpots))]
		public CGModuleOutputSlot OutSpots = new CGModuleOutputSlot();

		[SerializeField]
		[HideInInspector]
		private bool m_WasUpgraded;

		[Tab("General")]
		[Section("Default/General/Volume Path", true, false, 100)]
		[FloatRegion(RegionOptionsPropertyName = "RangeOptions", Precision = 4)]
		[SerializeField]
		private FloatRegion m_Range = FloatRegion.ZeroOne;

		[Section("Default/General/Volume Cross", true, false, 100)]
		[Tooltip("When the source is a Volume, you can choose if you want to use it's path or the volume")]
		[FieldCondition("IsInputAVolume", false, true, ActionAttribute.ActionEnum.Show, null, ActionAttribute.ActionPositionEnum.Below)]
		[SerializeField]
		[Label("Use Volume's Surface", "")]
		private bool m_UseVolume = true;

		[SerializeField]
		[RangeEx(-1f, 1f, "", "")]
		[Tooltip("Shifts the Cross origin value by constant value")]
		private float m_CrossBase;

		[SerializeField]
		[Label("Cross Base Variation", "")]
		[Tooltip("Shifts the Cross origin value by a value that varies along the Volume's length. The Curve's X axis has values between 0 (start of the Range) and 1 (its end)")]
		private AnimationCurve m_CrossCurve = AnimationCurve.Linear(0f, 0f, 1f, 0f);

		[Section("Default/General/Advanced Settings", false, false, 100)]
		[Tooltip("Check to run a dry run without actually creating spots")]
		[SerializeField]
		private bool m_Simulate;

		[SerializeField]
		[Tooltip("Until version 6.3.1, this module had a bug in the computation of the randomized values. Enable this value to keep that bugged behaviour if your project depends on it")]
		private bool m_UseBuggedRNG;

		[Tab("Groups")]
		[ArrayEx(Space = 10)]
		[SerializeField]
		private List<CGBoundsGroup> m_Groups = new List<CGBoundsGroup>();

		[IntRegion(UseSlider = false, RegionOptionsPropertyName = "RepeatingGroupsOptions", Options = AttributeOptionsFlags.Compact)]
		[SerializeField]
		[Tooltip("The range of groups that will be placed repetitively along the volume. Groups that are not in this range will be placed only once")]
		private IntRegion m_RepeatingGroups;

		[SerializeField]
		private CurvyRepeatingOrderEnum m_RepeatingOrder = CurvyRepeatingOrderEnum.Row;

		[SerializeField]
		[FieldCondition("ShowFitEnd", true, false, ActionAttribute.ActionEnum.Show, null, ActionAttribute.ActionPositionEnum.Below)]
		[Label("Fits The End", "")]
		[Tooltip("If checked, the last non repeating group is placed exactly at the end of the volume used for spots. If not, the last group is placed at the first available spot, which might leave some space between it and the end of the volume")]
		private bool m_FitEnd;

		public CGSpots SimulatedSpots;

		private EditorData editorData;

		public FloatRegion Range
		{
			get
			{
				return m_Range;
			}
			set
			{
				if (m_Range != value)
				{
					m_Range = value;
					base.Dirty = true;
				}
			}
		}

		public bool UseVolume
		{
			get
			{
				return m_UseVolume;
			}
			set
			{
				if (m_UseVolume != value)
				{
					m_UseVolume = value;
					base.Dirty = true;
				}
			}
		}

		public bool Simulate
		{
			get
			{
				return m_Simulate;
			}
			set
			{
				if (m_Simulate != value)
				{
					m_Simulate = value;
					base.Dirty = true;
				}
			}
		}

		public bool UseBuggedRng
		{
			get
			{
				return m_UseBuggedRNG;
			}
			set
			{
				if (m_UseBuggedRNG != value)
				{
					m_UseBuggedRNG = value;
					base.Dirty = true;
				}
			}
		}

		public float CrossBase
		{
			get
			{
				return m_CrossBase;
			}
			set
			{
				float num = value.Repeat(-1f, 1f);
				if (m_CrossBase != num)
				{
					m_CrossBase = num;
					base.Dirty = true;
				}
			}
		}

		public AnimationCurve CrossCurve
		{
			get
			{
				return m_CrossCurve;
			}
			set
			{
				if (m_CrossCurve != value)
				{
					m_CrossCurve = value;
					base.Dirty = true;
				}
			}
		}

		public List<CGBoundsGroup> Groups
		{
			get
			{
				return m_Groups;
			}
			set
			{
				m_Groups = value;
			}
		}

		public CurvyRepeatingOrderEnum RepeatingOrder
		{
			get
			{
				return m_RepeatingOrder;
			}
			set
			{
				if (m_RepeatingOrder != value)
				{
					m_RepeatingOrder = value;
					base.Dirty = true;
				}
			}
		}

		public int FirstRepeating
		{
			get
			{
				return m_RepeatingGroups.From;
			}
			set
			{
				int num = Mathf.Clamp(value, 0, LastGroupIndex);
				if (m_RepeatingGroups.From != num)
				{
					m_RepeatingGroups.From = num;
					base.Dirty = true;
				}
			}
		}

		public int LastRepeating
		{
			get
			{
				return m_RepeatingGroups.To;
			}
			set
			{
				int num = Mathf.Clamp(value, FirstRepeating, LastGroupIndex);
				if (m_RepeatingGroups.To != num)
				{
					m_RepeatingGroups.To = num;
					base.Dirty = true;
				}
			}
		}

		public bool FitEnd
		{
			get
			{
				return m_FitEnd;
			}
			set
			{
				if (m_FitEnd != value)
				{
					m_FitEnd = value;
					base.Dirty = true;
				}
			}
		}

		public int GroupCount => Groups.Count;

		[UsedImplicitly]
		[Obsolete("Will become an editor only method")]
		public GUIContent[] BoundsNames => editorData.BoundsNames.Select((string n) => new GUIContent(n)).ToArray();

		[UsedImplicitly]
		[Obsolete]
		public int[] BoundsIndices
		{
			get
			{
				int[] array = new int[BoundsNames.Length];
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = i;
				}
				return array;
			}
		}

		public int Count => editorData.SpotsCount;

		private int LastGroupIndex => Mathf.Max(0, GroupCount - 1);

		private RegionOptions<float> RangeOptions => RegionOptions<float>.MinMax(0f, 1f);

		private RegionOptions<int> RepeatingGroupsOptions => RegionOptions<int>.MinMax(0, LastGroupIndex);

		private bool ShowFitEnd => LastRepeating != Groups.Count - 1;

		public BuildVolumeSpots()
		{
			base.Version = "1";
		}

		private bool IsInputAVolume()
		{
			return editorData.InputIsAVolume;
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			Properties.MinWidth = 350f;
		}

		public override void Reset()
		{
			base.Reset();
			Range = FloatRegion.ZeroOne;
			UseVolume = true;
			Simulate = false;
			UseBuggedRng = false;
			CrossBase = 0f;
			CrossCurve = AnimationCurve.Linear(0f, 0f, 1f, 0f);
			RepeatingOrder = CurvyRepeatingOrderEnum.Row;
			FirstRepeating = 0;
			LastRepeating = 0;
			FitEnd = false;
			Groups.Clear();
			AddGroup("Group");
		}

		public override void OnStateChange()
		{
			base.OnStateChange();
			if (!IsConfigured)
			{
				Clear();
			}
		}

		public void Clear()
		{
			editorData = default(EditorData);
			SimulatedSpots = new CGSpots();
			OutSpots.SetDataToElement(SimulatedSpots);
		}

		public override void Refresh()
		{
			base.Refresh();
			bool isDataDisposable;
			List<CGBounds> allData = InBounds.GetAllData<CGBounds>(out isDataDisposable, Array.Empty<CGDataRequestParameter>());
			bool flag = false;
			if (allData.Count == 0)
			{
				flag = true;
				UIMessages.Add("The input bounds list is empty. Add some to enable spots generation.");
			}
			if (Groups.Count == 0)
			{
				flag = true;
				UIMessages.Add("No group created. Create a group in the Groups tab to enable spots generation");
			}
			for (int j = 0; j < allData.Count; j++)
			{
				CGBounds cGBounds = allData[j];
				if (cGBounds is CGGameObject && ((CGGameObject)cGBounds).Object == null)
				{
					flag = true;
					UIMessages.Add($"Input object of index {j} has no Game Object attached to it. Correct this to enable spots generation.");
				}
				else if (cGBounds.Depth <= 0.001f)
				{
					CGBounds cGBounds2 = new CGBounds(cGBounds);
					UIMessages.Add($"Input object \"{cGBounds2.Name}\" has bounds with a depth of {cGBounds.Depth}. The minimal accepted depth is {0.001f}. The depth value was overriden.");
					cGBounds2.Bounds = new Bounds(cGBounds.Bounds.center, new Vector3(cGBounds.Bounds.size.x, cGBounds.Bounds.size.y, 0.001f));
					allData[j] = cGBounds2;
				}
			}
			foreach (CGBoundsGroup group in Groups)
			{
				if (group.ItemCount == 0)
				{
					flag = true;
					UIMessages.Add($"Group \"{group.Name}\" has 0 item in it. Add some to enable spots generation.");
					continue;
				}
				foreach (CGBoundsGroupItem item in group.Items)
				{
					int index = item.Index;
					if (index < 0 || index >= allData.Count)
					{
						flag = true;
						UIMessages.Add($"Group \"{group.Name}\" has a reference to an nonexistent item of index {index}. Correct the reference to enable spots generation.");
						break;
					}
				}
			}
			bool isDataDisposable2;
			CGPath data = InPath.GetData<CGPath>(out isDataDisposable2, Array.Empty<CGDataRequestParameter>());
			SubArrayList<CGSpot> spots = new SubArrayList<CGSpot>(100, ArrayPools.CGSpot);
			WeightedRandom<int> groupBag;
			Dictionary<CGBoundsGroup, WeightedRandom<int>> dictionary = Prepare(out groupBag);
			if ((bool)data && !flag)
			{
				bool flag2 = false;
				float num = data.FToDistance(Range.To);
				float num2 = data.FToDistance(Range.Low);
				float currentDistance = num2;
				for (int k = 0; k < FirstRepeating; k++)
				{
					int groupIndex = k;
					flag2 = AddGroupItems(allData, data, groupIndex, ref spots, num - currentDistance, num2, ref currentDistance, out var _, dictionary, 10000);
					if (flag2)
					{
						break;
					}
				}
				bool flag3 = GroupCount - LastRepeating - 1 > 0;
				List<EndGroupData> list;
				if (!flag2 && flag3)
				{
					list = new List<EndGroupData>();
					for (int l = LastRepeating + 1; l < GroupCount; l++)
					{
						CGBoundsGroup cGBoundsGroup = Groups[l];
						SubArray<int> groupItemIndices = GetGroupItemIndices(cGBoundsGroup, dictionary[cGBoundsGroup]);
						float spaceBefore = (UseBuggedRng ? cGBoundsGroup.SpaceBefore.Next : GetRegionNextValue(cGBoundsGroup.SpaceBefore));
						float spaceAfter = (UseBuggedRng ? cGBoundsGroup.SpaceAfter.Next : GetRegionNextValue(cGBoundsGroup.SpaceAfter));
						CGBounds[] itemsBounds;
						float groupDepth = GetGroupDepth(allData, groupItemIndices, spaceBefore, spaceAfter, out itemsBounds);
						list.Add(new EndGroupData(cGBoundsGroup, groupItemIndices, groupDepth, itemsBounds, spaceBefore, spaceAfter));
					}
				}
				else
				{
					list = null;
				}
				float num3 = num;
				if (flag3)
				{
					foreach (EndGroupData endGroupData in list)
					{
						float availableSpace = num3 - currentDistance;
						float num4 = num3 - endGroupData.GroupDepth * 1.00001f;
						if (endGroupData.GroupDepth <= availableSpace)
						{
							num3 = num4;
						}
						else if (!endGroupData.BoundsGroup.KeepTogether && endGroupData.ItemBounds.Any((CGBounds i) => i.Depth + endGroupData.SpaceBefore + endGroupData.SpaceAfter <= availableSpace))
						{
							num3 = num4;
						}
					}
				}
				if (RepeatingOrder == CurvyRepeatingOrderEnum.Row)
				{
					int firstRepeating = FirstRepeating;
					bool failedAddingAllItems2 = false;
					while (!flag2 && !failedAddingAllItems2 && num3 > currentDistance)
					{
						int groupIndex2 = firstRepeating++;
						if (firstRepeating > LastRepeating)
						{
							firstRepeating = FirstRepeating;
						}
						flag2 = AddGroupItems(allData, data, groupIndex2, ref spots, num3 - currentDistance, num2, ref currentDistance, out failedAddingAllItems2, dictionary, 10000);
						if (flag2)
						{
							break;
						}
					}
				}
				else
				{
					bool failedAddingAllItems3 = false;
					while (!flag2 && !failedAddingAllItems3 && num3 > currentDistance)
					{
						int groupIndex3 = groupBag.Next();
						flag2 = AddGroupItems(allData, data, groupIndex3, ref spots, num3 - currentDistance, num2, ref currentDistance, out failedAddingAllItems3, dictionary, 10000);
						if (flag2)
						{
							break;
						}
					}
				}
				if (!flag2 && flag3)
				{
					if (FitEnd)
					{
						currentDistance = Mathf.Max(currentDistance, num3);
					}
					foreach (EndGroupData item2 in list)
					{
						AddGroupItems(data, item2.BoundsGroup, ref spots, num - currentDistance, num2, ref currentDistance, out var _, item2.ItemIndices, item2.GroupDepth, item2.ItemBounds, item2.SpaceBefore, item2.SpaceAfter);
						if (spots.Count >= 10000)
						{
							flag2 = true;
							break;
						}
					}
				}
				if (flag2)
				{
					string text = $"Number of generated spots reached the maximal allowed number, which is {10000}. Spots generation was stopped. Try to reduce the number of spots needed by using bigger Bounds as inputs and/or setting bigger space between two spots.";
					UIMessages.Add(text);
					DTLog.LogError("[Curvy] Volume spots: " + text, this);
				}
				if (list != null)
				{
					foreach (EndGroupData item3 in list)
					{
						item3.Dispose();
					}
					list = null;
				}
			}
			editorData = new EditorData(allData, data is CGVolume, spots.Count);
			if (isDataDisposable)
			{
				foreach (CGBounds item4 in allData)
				{
					item4.Dispose();
				}
			}
			SimulatedSpots = new CGSpots(spots.ToSubArray());
			if (Simulate)
			{
				OutSpots.SetDataToElement(new CGSpots());
			}
			else
			{
				OutSpots.SetDataToElement(SimulatedSpots);
			}
			if (isDataDisposable2)
			{
				data.Dispose();
			}
		}

		public CGBoundsGroup AddGroup(string name)
		{
			CGBoundsGroup cGBoundsGroup = new CGBoundsGroup(name);
			cGBoundsGroup.Items.Add(new CGBoundsGroupItem());
			Groups.Add(cGBoundsGroup);
			base.Dirty = true;
			return cGBoundsGroup;
		}

		public void RemoveGroup(CGBoundsGroup group)
		{
			Groups.Remove(group);
			base.Dirty = true;
		}

		private static SubArray<int> GetGroupItemIndices(CGBoundsGroup boundsGroup, WeightedRandom<int> groupItemBag)
		{
			SubArray<int> result = ArrayPools.Int32.Allocate(boundsGroup.ItemCount, clearArray: false);
			for (int i = 0; i < boundsGroup.ItemCount; i++)
			{
				int index = ((boundsGroup.RandomizeItems && i >= boundsGroup.FirstRepeating && i <= boundsGroup.LastRepeating) ? groupItemBag.Next() : i);
				result.Array[i] = boundsGroup.Items[index].Index;
			}
			return result;
		}

		private static float GetGroupDepth(List<CGBounds> bounds, SubArray<int> groupItemIndices, float spaceBefore, float spaceAfter, out CGBounds[] itemsBounds)
		{
			itemsBounds = new CGBounds[groupItemIndices.Count];
			float num = spaceBefore + spaceAfter;
			for (int i = 0; i < groupItemIndices.Count; i++)
			{
				CGBounds cGBounds = bounds[groupItemIndices.Array[i]];
				itemsBounds[i] = cGBounds;
				num += cGBounds.Depth;
			}
			return num;
		}

		private bool AddGroupItems(List<CGBounds> bounds, CGPath path, int groupIndex, ref SubArrayList<CGSpot> spots, float remainingLength, float startDistance, ref float currentDistance, out bool failedAddingAllItems, Dictionary<CGBoundsGroup, WeightedRandom<int>> itemsBagDictionary, int MaxSpotsCount)
		{
			CGBoundsGroup cGBoundsGroup = Groups[groupIndex];
			WeightedRandom<int> groupItemBag = itemsBagDictionary[cGBoundsGroup];
			SubArray<int> groupItemIndices = GetGroupItemIndices(cGBoundsGroup, groupItemBag);
			float spaceBefore = (UseBuggedRng ? cGBoundsGroup.SpaceBefore.Next : GetRegionNextValue(cGBoundsGroup.SpaceBefore));
			float spaceAfter = (UseBuggedRng ? cGBoundsGroup.SpaceAfter.Next : GetRegionNextValue(cGBoundsGroup.SpaceAfter));
			CGBounds[] itemsBounds;
			float groupDepth = GetGroupDepth(bounds, groupItemIndices, spaceBefore, spaceAfter, out itemsBounds);
			AddGroupItems(path, cGBoundsGroup, ref spots, remainingLength, startDistance, ref currentDistance, out failedAddingAllItems, groupItemIndices, groupDepth, itemsBounds, spaceBefore, spaceAfter);
			ArrayPools.Int32.Free(groupItemIndices);
			return spots.Count >= MaxSpotsCount;
		}

		private void AddGroupItems(CGPath path, CGBoundsGroup group, ref SubArrayList<CGSpot> spots, float remainingLength, float startDistance, ref float currentDistance, out bool failedAddingAllItems, SubArray<int> itemIndices, float groupDepth, CGBounds[] itemBounds, float spaceBefore, float spaceAfter)
		{
			if (remainingLength >= groupDepth || !group.KeepTogether)
			{
				failedAddingAllItems = false;
				for (int i = 0; i < itemIndices.Count; i++)
				{
					float num = currentDistance;
					int itemID = itemIndices.Array[i];
					CGBounds cGBounds = itemBounds[i];
					bool flag;
					if (i != 0)
					{
						flag = ((i != itemIndices.Count - 1) ? (remainingLength > cGBounds.Depth) : (remainingLength > spaceAfter + cGBounds.Depth));
					}
					else
					{
						flag = remainingLength > spaceBefore + cGBounds.Depth;
						if (flag)
						{
							currentDistance += spaceBefore;
						}
					}
					if (!flag)
					{
						failedAddingAllItems = true;
						break;
					}
					spots.Add(GetSpot(path, itemID, group, cGBounds, currentDistance, startDistance));
					if (i == itemIndices.Count - 1)
					{
						currentDistance += cGBounds.Depth + spaceAfter;
					}
					else
					{
						currentDistance += cGBounds.Depth;
					}
					remainingLength -= currentDistance - num;
				}
			}
			else
			{
				failedAddingAllItems = true;
			}
		}

		private CGSpot GetSpot(CGPath path, int itemID, CGBoundsGroup boundsGroup, CGBounds bounds, float currentDistance, float startDistance)
		{
			float f = path.DistanceToF(currentDistance + bounds.Depth / 2f);
			float num = path.Length * Range.Length;
			float time = (currentDistance - startDistance) / num;
			float num2 = (UseBuggedRng ? boundsGroup.CrossBase.Next : GetRegionNextValue(boundsGroup.CrossBase));
			if (!boundsGroup.IgnoreModuleCrossBase)
			{
				num2 += CrossBase + m_CrossCurve.Evaluate(time);
			}
			float num3 = DTMath.MapValue(-0.5f, 0.5f, num2);
			CGVolume cGVolume = path as CGVolume;
			bool flag = UseVolume && (bool)cGVolume;
			Vector3 position;
			Vector3 direction;
			Vector3 up;
			switch (boundsGroup.RotationMode)
			{
			case CGBoundsGroup.RotationModeEnum.Full:
				if (flag)
				{
					cGVolume.InterpolateVolume(f, num3, out position, out direction, out up);
					break;
				}
				path.Interpolate(f, out position, out direction, out up);
				if (num3 != 0f)
				{
					up = Quaternion.AngleAxis(num3 * -360f, direction) * up;
				}
				break;
			case CGBoundsGroup.RotationModeEnum.Direction:
			case CGBoundsGroup.RotationModeEnum.Horizontal:
			{
				Vector3 up2;
				if (flag)
				{
					cGVolume.InterpolateVolume(f, num3, out position, out direction, out up2);
				}
				else
				{
					path.Interpolate(f, out position, out direction, out up2);
				}
				up = Vector3.up;
				if (boundsGroup.RotationMode == CGBoundsGroup.RotationModeEnum.Horizontal)
				{
					direction.y = 0f;
				}
				break;
			}
			case CGBoundsGroup.RotationModeEnum.Independent:
				position = ((!flag) ? path.InterpolatePosition(f) : cGVolume.InterpolateVolumePosition(f, num3));
				up = Vector3.up;
				direction = Vector3.forward;
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			Quaternion rotation;
			Vector3 translation;
			Vector3 scale;
			if (UseBuggedRng)
			{
				GetTRS630(boundsGroup, direction, up, out rotation, out translation, out scale);
			}
			else
			{
				GetTRS(boundsGroup, direction, up, out rotation, out translation, out scale);
			}
			return new CGSpot(itemID, position.Addition(boundsGroup.RelativeTranslation ? (rotation * translation) : translation), rotation, scale);
		}

		private static float GetRegionNextValue(FloatRegion floatRegion)
		{
			float result;
			if (floatRegion.SimpleValue)
			{
				result = floatRegion.From;
				UnityEngine.Random.Range(0f, 1f);
			}
			else
			{
				result = UnityEngine.Random.Range(floatRegion.From, floatRegion.To);
			}
			return result;
		}

		private void GetTRS(CGBoundsGroup boundsGroup, Vector3 tangent, Vector3 up, out Quaternion rotation, out Vector3 translation, out Vector3 scale)
		{
			Vector3 euler = default(Vector3);
			euler.x = GetRegionNextValue(boundsGroup.RotationX);
			euler.y = GetRegionNextValue(boundsGroup.RotationY);
			euler.z = GetRegionNextValue(boundsGroup.RotationZ);
			rotation = Quaternion.LookRotation(tangent, up) * Quaternion.Euler(euler);
			translation.x = GetRegionNextValue(boundsGroup.TranslationX);
			translation.y = GetRegionNextValue(boundsGroup.TranslationY);
			translation.z = GetRegionNextValue(boundsGroup.TranslationZ);
			scale.x = GetRegionNextValue(boundsGroup.ScaleX);
			if (boundsGroup.UniformScaling)
			{
				scale.y = (scale.z = scale.x);
				UnityEngine.Random.Range(0f, 1f);
				UnityEngine.Random.Range(0f, 1f);
			}
			else
			{
				scale.y = GetRegionNextValue(boundsGroup.ScaleY);
				scale.z = GetRegionNextValue(boundsGroup.ScaleZ);
			}
		}

		private void GetTRS630(CGBoundsGroup boundsGroup, Vector3 tangent, Vector3 up, out Quaternion rotation, out Vector3 translation, out Vector3 scale)
		{
			Vector3 vector = new Vector3(boundsGroup.RotationX.SimpleValue ? 0f : ((boundsGroup.RotationX.High - boundsGroup.RotationX.Low) * 0.5f), boundsGroup.RotationY.SimpleValue ? 0f : ((boundsGroup.RotationY.High - boundsGroup.RotationY.Low) * 0.5f), boundsGroup.RotationZ.SimpleValue ? 0f : ((boundsGroup.RotationZ.High - boundsGroup.RotationZ.Low) * 0.5f));
			Vector3 vector2 = new Vector3(boundsGroup.RotationX.SimpleValue ? boundsGroup.RotationX.From : ((boundsGroup.RotationX.From + boundsGroup.RotationX.To) * 0.5f), boundsGroup.RotationY.SimpleValue ? boundsGroup.RotationY.From : ((boundsGroup.RotationY.From + boundsGroup.RotationY.To) * 0.5f), boundsGroup.RotationZ.SimpleValue ? boundsGroup.RotationZ.From : ((boundsGroup.RotationZ.From + boundsGroup.RotationZ.To) * 0.5f));
			rotation = Quaternion.LookRotation(tangent, up) * Quaternion.Euler(vector2.x + vector.x * (float)UnityEngine.Random.Range(-1, 1), vector2.y + vector.y * (float)UnityEngine.Random.Range(-1, 1), vector2.z + vector.z * (float)UnityEngine.Random.Range(-1, 1));
			FloatRegion translationX = boundsGroup.TranslationX;
			FloatRegion translationY = boundsGroup.TranslationY;
			FloatRegion translationZ = boundsGroup.TranslationZ;
			if (translationY.SimpleValue)
			{
				translation.y = translationY.From;
			}
			else
			{
				translation.y = UnityEngine.Random.Range(translationY.From, translationY.To);
			}
			UnityEngine.Random.State state = UnityEngine.Random.state;
			if (translationX.SimpleValue)
			{
				translation.x = translationX.From;
			}
			else
			{
				translation.x = UnityEngine.Random.Range(translationX.From, translationX.To);
				UnityEngine.Random.state = state;
			}
			if (translationZ.SimpleValue)
			{
				translation.z = translationZ.From;
			}
			else
			{
				translation.z = UnityEngine.Random.Range(translationZ.From, translationZ.To);
				UnityEngine.Random.state = state;
			}
			FloatRegion scaleX = boundsGroup.ScaleX;
			if (scaleX.SimpleValue)
			{
				scale.x = scaleX.From;
			}
			else
			{
				scale.x = UnityEngine.Random.Range(scaleX.From, scaleX.To);
				UnityEngine.Random.state = state;
			}
			if (boundsGroup.UniformScaling)
			{
				scale.y = (scale.z = scale.x);
				return;
			}
			FloatRegion scaleY = boundsGroup.ScaleY;
			FloatRegion scaleZ = boundsGroup.ScaleZ;
			if (scaleY.SimpleValue)
			{
				scale.y = scaleY.From;
			}
			else
			{
				scale.y = UnityEngine.Random.Range(scaleY.From, scaleY.To);
				UnityEngine.Random.state = state;
			}
			if (scaleZ.SimpleValue)
			{
				scale.z = scaleZ.From;
				return;
			}
			scale.z = UnityEngine.Random.Range(scaleZ.From, scaleZ.To);
			UnityEngine.Random.state = state;
		}

		private Dictionary<CGBoundsGroup, WeightedRandom<int>> Prepare(out WeightedRandom<int> groupBag)
		{
			Dictionary<CGBoundsGroup, WeightedRandom<int>> dictionary = new Dictionary<CGBoundsGroup, WeightedRandom<int>>();
			m_RepeatingGroups.MakePositive();
			m_RepeatingGroups.Clamp(0, GroupCount - 1);
			groupBag = new WeightedRandom<int>(0, (!UseBuggedRng) ? UnityEngine.Random.Range(0, int.MaxValue) : 0);
			if (RepeatingOrder == CurvyRepeatingOrderEnum.Random)
			{
				List<CGWeightedItem> itemsWeights = Groups.Cast<CGWeightedItem>().ToList();
				CGBoundsGroup.FillItemBag(groupBag, itemsWeights, FirstRepeating, LastRepeating);
			}
			for (int i = 0; i < Groups.Count; i++)
			{
				CGBoundsGroup cGBoundsGroup = Groups[i];
				cGBoundsGroup.RepeatingItems.MakePositive();
				cGBoundsGroup.RepeatingItems.Clamp(0, cGBoundsGroup.ItemCount - 1);
				UnityEngine.Random.State state = UnityEngine.Random.state;
				WeightedRandom<int> weightedRandom = new WeightedRandom<int>(0, (!UseBuggedRng) ? UnityEngine.Random.Range(0, int.MaxValue) : 0);
				UnityEngine.Random.state = state;
				dictionary[cGBoundsGroup] = weightedRandom;
				if (cGBoundsGroup.Items.Count != 0 && cGBoundsGroup.RandomizeItems)
				{
					List<CGWeightedItem> itemsWeights2 = cGBoundsGroup.Items.Cast<CGWeightedItem>().ToList();
					CGBoundsGroup.FillItemBag(weightedRandom, itemsWeights2, cGBoundsGroup.FirstRepeating, cGBoundsGroup.LastRepeating);
				}
			}
			return dictionary;
		}

		protected override void ResetOnEnable()
		{
			base.ResetOnEnable();
			editorData = default(EditorData);
		}

		public void OnBeforeSerialize()
		{
		}

		public void OnAfterDeserialize()
		{
			if (string.IsNullOrEmpty(base.Version))
			{
				base.Version = "1";
				m_WasUpgraded = true;
				for (int i = 0; i < Groups.Count; i++)
				{
					CGBoundsGroup cGBoundsGroup = Groups[i];
					cGBoundsGroup.RelativeTranslation = true;
					cGBoundsGroup.TranslationX = new FloatRegion(0f);
					cGBoundsGroup.TranslationY = new FloatRegion(0f);
					cGBoundsGroup.TranslationZ = new FloatRegion(0f);
					cGBoundsGroup.RotationX = new FloatRegion(0f);
					cGBoundsGroup.RotationY = new FloatRegion(0f);
					cGBoundsGroup.RotationZ = new FloatRegion(0f);
					cGBoundsGroup.UniformScaling = true;
					cGBoundsGroup.ScaleX = new FloatRegion(1f);
					cGBoundsGroup.ScaleY = new FloatRegion(1f);
					cGBoundsGroup.ScaleZ = new FloatRegion(1f);
					cGBoundsGroup.ConvertObsoleteData();
				}
			}
		}
	}
}
