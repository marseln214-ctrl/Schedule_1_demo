using System;
using System.Globalization;
using UnityEngine;

namespace FluffyUnderware.Curvy.Generator
{
	[Serializable]
	public class CGModuleLink
	{
		[SerializeField]
		private int m_ModuleID;

		[SerializeField]
		private string m_SlotName;

		[SerializeField]
		private int m_TargetModuleID;

		[SerializeField]
		private string m_TargetSlotName;

		public int ModuleID => m_ModuleID;

		public string SlotName => m_SlotName;

		public int TargetModuleID => m_TargetModuleID;

		public string TargetSlotName => m_TargetSlotName;

		public CGModuleLink(int sourceID, string sourceSlotName, int targetID, string targetSlotName)
		{
			m_ModuleID = sourceID;
			m_SlotName = sourceSlotName;
			m_TargetModuleID = targetID;
			m_TargetSlotName = targetSlotName;
		}

		public CGModuleLink(CGModuleSlot source, CGModuleSlot target)
			: this(source.Module.UniqueID, source.Name, target.Module.UniqueID, target.Name)
		{
		}

		public bool IsSame(CGModuleLink o)
		{
			if (ModuleID == o.ModuleID && SlotName == o.SlotName && TargetModuleID == o.TargetModuleID)
			{
				return TargetSlotName == o.m_TargetSlotName;
			}
			return false;
		}

		public bool IsSame(CGModuleSlot source, CGModuleSlot target)
		{
			if (ModuleID == source.Module.UniqueID && SlotName == source.Name && TargetModuleID == target.Module.UniqueID)
			{
				return TargetSlotName == target.Name;
			}
			return false;
		}

		public bool IsTo(CGModuleSlot s)
		{
			if (s.Module.UniqueID == TargetModuleID)
			{
				return s.Name == TargetSlotName;
			}
			return false;
		}

		public bool IsFrom(CGModuleSlot s)
		{
			if (s.Module.UniqueID == ModuleID)
			{
				return s.Name == SlotName;
			}
			return false;
		}

		public bool IsUsing(CGModule module)
		{
			if (ModuleID != module.UniqueID)
			{
				return TargetModuleID == module.UniqueID;
			}
			return true;
		}

		public bool IsBetween(CGModuleSlot one, CGModuleSlot another)
		{
			if (!IsTo(one) || !IsFrom(another))
			{
				if (IsTo(another))
				{
					return IsFrom(one);
				}
				return false;
			}
			return true;
		}

		public void SetModuleIDIINTERNAL(int moduleID, int targetModuleID)
		{
			m_ModuleID = moduleID;
			m_TargetModuleID = targetModuleID;
		}

		public static implicit operator bool(CGModuleLink a)
		{
			return a != null;
		}

		public override string ToString()
		{
			return string.Format(CultureInfo.InvariantCulture, "{0}({1})->{2}({3})", SlotName, ModuleID, TargetSlotName, TargetModuleID);
		}
	}
}
