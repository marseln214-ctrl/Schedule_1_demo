using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

namespace FluffyUnderware.Curvy.Generator
{
	[Serializable]
	public class CGModuleOutputSlot : CGModuleSlot
	{
		[NotNull]
		[ItemNotNull]
		private CGData[] data = Array.Empty<CGData>();

		[CanBeNull]
		public CGDataRequestParameter[] LastRequestParameters;

		[NotNull]
		[ItemNotNull]
		public CGData[] Data
		{
			get
			{
				return data;
			}
			[UsedImplicitly]
			[Obsolete("Use ClearData, SetDataToElement or SetDataToCollection instead.")]
			set
			{
				data = value;
			}
		}

		[CanBeNull]
		public OutputSlotInfo OutputInfo => base.Info as OutputSlotInfo;

		[UsedImplicitly]
		[Obsolete("Use Data instead")]
		public bool HasData => Data.Length != 0;

		protected override void LoadLinkedSlots()
		{
			if (!base.Module.Generator.IsInitialized)
			{
				return;
			}
			base.LoadLinkedSlots();
			mLinkedSlots = new List<CGModuleSlot>();
			foreach (CGModuleLink outputLink in base.Module.GetOutputLinks(this))
			{
				CGModule module = base.Module.Generator.GetModule(outputLink.TargetModuleID, includeOnRequestProcessing: true);
				if ((bool)module)
				{
					CGModuleInputSlot cGModuleInputSlot = module.InputByName[outputLink.TargetSlotName];
					if (!cGModuleInputSlot.Module.GetInputLink(cGModuleInputSlot, this))
					{
						cGModuleInputSlot.Module.InputLinks.Add(new CGModuleLink(cGModuleInputSlot, this));
						cGModuleInputSlot.ReInitializeLinkedSlots();
					}
					if (!mLinkedSlots.Contains(cGModuleInputSlot))
					{
						mLinkedSlots.Add(cGModuleInputSlot);
					}
				}
				else
				{
					base.Module.OutputLinks.Remove(outputLink);
				}
			}
		}

		public override void LinkTo(CGModuleSlot inputSlot)
		{
			if (!HasLinkTo(inputSlot))
			{
				CGModuleSlot.LinkInputAndOutput(inputSlot, this);
				base.LinkTo(inputSlot);
			}
		}

		public override void UnlinkFrom(CGModuleSlot inputSlot)
		{
			if (HasLinkTo(inputSlot))
			{
				CGModuleInputSlot inputSlot2 = (CGModuleInputSlot)inputSlot;
				CGModuleLink outputLink = base.Module.GetOutputLink(this, inputSlot2);
				base.Module.OutputLinks.Remove(outputLink);
				CGModuleLink inputLink = inputSlot.Module.GetInputLink(inputSlot2, this);
				inputSlot.Module.InputLinks.Remove(inputLink);
				base.UnlinkFrom(inputSlot);
			}
		}

		public void ClearData()
		{
			AssignNewData(Array.Empty<CGData>());
		}

		public void SetDataToElement<T>([NotNull] T element) where T : CGData
		{
			if (element == null)
			{
				throw new ArgumentNullException("element");
			}
			AssignNewData(new CGData[1] { element });
		}

		public void SetDataToCollection<T>([NotNull][ItemNotNull] T[] elements) where T : CGData
		{
			if (elements == null)
			{
				throw new ArgumentNullException("elements");
			}
			AssignNewData(elements);
		}

		[UsedImplicitly]
		[Obsolete("Use SetDataToElement or SetDataToCollection instead.")]
		public void SetData<T>([CanBeNull][ItemNotNull] List<T> newData) where T : CGData
		{
			if (newData == null)
			{
				newData = new List<T>();
			}
			if (newData.Contains(null))
			{
				newData = newData.Where((T d) => d != null).ToList();
			}
			SetDataToCollection(newData.ToArray());
		}

		[UsedImplicitly]
		[Obsolete("Use SetDataToElement or SetDataToCollection instead.")]
		public void SetData([CanBeNull] params CGData[] newData)
		{
			if (newData == null)
			{
				newData = Array.Empty<CGData>();
			}
			if (newData.Contains(null))
			{
				newData = newData.Where((CGData d) => d != null).ToArray();
			}
			SetDataToCollection(newData);
		}

		[CanBeNull]
		[UsedImplicitly]
		[Obsolete("Use Data instead")]
		public T GetData<T>() where T : CGData
		{
			if (Data.Length != 0)
			{
				return Data[0] as T;
			}
			return null;
		}

		[CanBeNull]
		[UsedImplicitly]
		[Obsolete("Use Data instead")]
		public T[] GetAllData<T>() where T : CGData
		{
			return Data as T[];
		}

		private void AssignNewData([NotNull][ItemNotNull] CGData[] newData)
		{
			if (newData == null)
			{
				throw new ArgumentNullException("newData");
			}
			for (int i = 0; i < newData.Length; i++)
			{
				if (newData[i] == null)
				{
					throw new ArgumentNullException("newData", "Data array contains null elements");
				}
			}
			if (!base.Info.Array && newData.Length > 1)
			{
				Debug.LogWarning("[Curvy] " + base.Module.GetType().Name + " (" + base.Info.DisplayName + ") only supports a single data item! Either avoid calculating unnecessary data or define the slot as an array, by setting its Info.Array to true");
			}
			if (Data == newData)
			{
				return;
			}
			CGData[] array = Data;
			foreach (CGData cGData in array)
			{
				if (!newData.Contains(cGData))
				{
					cGData.Dispose();
				}
			}
			Data = newData;
		}
	}
}
