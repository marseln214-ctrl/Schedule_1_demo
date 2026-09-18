using System;
using System.Collections.Generic;
using System.Linq;
using FluffyUnderware.DevTools;
using JetBrains.Annotations;

namespace FluffyUnderware.Curvy.Generator
{
	[Serializable]
	public class CGModuleInputSlot : CGModuleSlot
	{
		public InputSlotInfo InputInfo => base.Info as InputSlotInfo;

		protected override void LoadLinkedSlots()
		{
			if (!base.Module.Generator.IsInitialized)
			{
				return;
			}
			base.LoadLinkedSlots();
			mLinkedSlots = new List<CGModuleSlot>();
			foreach (CGModuleLink inputLink in base.Module.GetInputLinks(this))
			{
				CGModule module = base.Module.Generator.GetModule(inputLink.TargetModuleID, includeOnRequestProcessing: true);
				if ((bool)module)
				{
					CGModuleOutputSlot cGModuleOutputSlot = module.OutputByName[inputLink.TargetSlotName];
					if (!cGModuleOutputSlot.Module.GetOutputLink(cGModuleOutputSlot, this))
					{
						cGModuleOutputSlot.Module.OutputLinks.Add(new CGModuleLink(cGModuleOutputSlot, this));
						cGModuleOutputSlot.ReInitializeLinkedSlots();
					}
					if (!mLinkedSlots.Contains(cGModuleOutputSlot))
					{
						mLinkedSlots.Add(cGModuleOutputSlot);
					}
				}
				else
				{
					base.Module.InputLinks.Remove(inputLink);
				}
			}
		}

		public override void LinkTo(CGModuleSlot outputSlot)
		{
			if (!HasLinkTo(outputSlot))
			{
				CGModuleSlot.LinkInputAndOutput(this, outputSlot);
				base.LinkTo(outputSlot);
			}
		}

		public override void UnlinkFrom(CGModuleSlot outputSlot)
		{
			if (HasLinkTo(outputSlot))
			{
				CGModuleOutputSlot outputSlot2 = (CGModuleOutputSlot)outputSlot;
				CGModuleLink inputLink = base.Module.GetInputLink(this, outputSlot2);
				base.Module.InputLinks.Remove(inputLink);
				CGModuleLink outputLink = outputSlot.Module.GetOutputLink(outputSlot2, this);
				outputSlot.Module.OutputLinks.Remove(outputLink);
				base.UnlinkFrom(outputSlot);
			}
		}

		public CGModuleOutputSlot SourceSlot(int index = 0)
		{
			if (index >= base.Count || index < 0)
			{
				return null;
			}
			return (CGModuleOutputSlot)base.LinkedSlots[index];
		}

		public bool CanLinkTo(CGModuleOutputSlot source)
		{
			if (source.Module != base.Module)
			{
				return AreInputAndOutputSlotsCompatible(InputInfo, base.OnRequestModule != null, source.OutputInfo, source.OnRequestModule != null);
			}
			return false;
		}

		public static bool AreInputAndOutputSlotsCompatible(InputSlotInfo inputSlotInfo, bool inputSlotModuleIsOnRequest, OutputSlotInfo outputSlotInfo, bool outputSlotModuleIsOnRequest)
		{
			if (inputSlotInfo.IsValidFrom(outputSlotInfo.DataType))
			{
				if (!outputSlotModuleIsOnRequest || !(inputSlotInfo.RequestDataOnly || inputSlotModuleIsOnRequest))
				{
					if (!outputSlotModuleIsOnRequest)
					{
						return !inputSlotInfo.RequestDataOnly;
					}
					return false;
				}
				return true;
			}
			return false;
		}

		private CGModule SourceModule(int index)
		{
			if (index >= base.Count || index < 0)
			{
				return null;
			}
			return base.LinkedSlots[index].Module;
		}

		[CanBeNull]
		public T GetData<T>(params CGDataRequestParameter[] requests) where T : CGData
		{
			bool isDataDisposable;
			return GetData<T>(out isDataDisposable, requests);
		}

		[CanBeNull]
		public T GetData<T>(out bool isDataDisposable, params CGDataRequestParameter[] requests) where T : CGData
		{
			CGData[] data = GetData<T>(0, out isDataDisposable, requests);
			if (data.Length == 0 || data[0] == null)
			{
				isDataDisposable = false;
				return null;
			}
			return data[0] as T;
		}

		[NotNull]
		public List<T> GetAllData<T>(params CGDataRequestParameter[] requests) where T : CGData
		{
			bool isDataDisposable;
			return GetAllData<T>(out isDataDisposable, requests);
		}

		[NotNull]
		public List<T> GetAllData<T>(out bool isDataDisposable, params CGDataRequestParameter[] requests) where T : CGData
		{
			isDataDisposable = true;
			List<T> list = new List<T>();
			for (int i = 0; i < base.Count; i++)
			{
				bool isDataDisposable2;
				CGData[] data = GetData<T>(i, out isDataDisposable2, requests);
				isDataDisposable &= isDataDisposable2;
				if (!base.Info.Array)
				{
					list.Add(data[0] as T);
					break;
				}
				for (int j = 0; j < data.Length; j++)
				{
					list.Add(data[j] as T);
				}
			}
			return list;
		}

		[NotNull]
		private CGData[] GetData<T>(int slotIndex, out bool isDataDisposable, params CGDataRequestParameter[] requests) where T : CGData
		{
			CGModuleOutputSlot cGModuleOutputSlot = SourceSlot(slotIndex);
			if (cGModuleOutputSlot == null || !cGModuleOutputSlot.Module.Active)
			{
				isDataDisposable = true;
				return new CGData[0];
			}
			if (cGModuleOutputSlot.Module is IOnRequestProcessing)
			{
				bool flag = cGModuleOutputSlot.Data.Length == 0;
				if (!flag && cGModuleOutputSlot.LastRequestParameters != null && cGModuleOutputSlot.LastRequestParameters.Length == requests.Length)
				{
					for (int i = 0; i < requests.Length; i++)
					{
						if (!requests[i].Equals(cGModuleOutputSlot.LastRequestParameters[i]))
						{
							flag = true;
							break;
						}
					}
				}
				else
				{
					flag = true;
				}
				if (flag)
				{
					cGModuleOutputSlot.LastRequestParameters = requests;
					cGModuleOutputSlot.Module.UIMessages.Clear();
					CGData[] array = ((IOnRequestProcessing)cGModuleOutputSlot.Module).OnSlotDataRequest(this, cGModuleOutputSlot, requests);
					if (array == null)
					{
						cGModuleOutputSlot.ClearData();
					}
					else if (array.Length == 0)
					{
						cGModuleOutputSlot.ClearData();
					}
					else if (array.All((CGData d) => d == null))
					{
						DTLog.LogWarning("[Curvy] " + cGModuleOutputSlot.Module.name + "'s output data is invalid. All data elements are null. Modify the module's IOnRequestProcessing.OnSlotDataRequest's implementation to always return arrays that are not null and contain no null element.");
						cGModuleOutputSlot.ClearData();
					}
					else if (array.Contains(null))
					{
						DTLog.LogWarning("[Curvy] " + cGModuleOutputSlot.Module.name + "'s output data is invalid. Some data elements are null. Modify the module's IOnRequestProcessing.OnSlotDataRequest's implementation to always return arrays that are not null and contain no null element.");
						cGModuleOutputSlot.SetDataToCollection(array.Where((CGData d) => d != null).ToArray());
					}
					else
					{
						cGModuleOutputSlot.SetDataToCollection(array);
					}
				}
			}
			bool flag2 = InputInfo.ModifiesData || cGModuleOutputSlot.Module is IOnRequestProcessing;
			CGData[] result = (flag2 ? CloneData<T>(cGModuleOutputSlot.Data) : cGModuleOutputSlot.Data);
			isDataDisposable = flag2;
			return result;
		}

		[NotNull]
		private static CGData[] CloneData<T>([NotNull] CGData[] source) where T : CGData
		{
			T[] array = new T[source.Length];
			for (int i = 0; i < source.Length; i++)
			{
				array[i] = ((source[i] == null) ? null : source[i].Clone<T>());
			}
			return array;
		}
	}
}
