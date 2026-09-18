using System;
using ScheduleOne.NPCs;
using ScheduleOne.Persistence.Datas;
using UnityEngine.Events;

namespace ScheduleOne.Management
{
	public class NPCField : ConfigField
	{
		public Type TypeRequirement;

		public UnityEvent<NPC> onNPCChanged = new UnityEvent<NPC>();

		public NPC SelectedNPC { get; protected set; }

		public NPCField(EntityConfiguration parentConfig)
			: base(parentConfig)
		{
		}

		public void SetNPC(NPC npc, bool network)
		{
			if (!(SelectedNPC == npc))
			{
				SelectedNPC = npc;
				if (network)
				{
					base.ParentConfig.ReplicateField(this);
				}
				if (onNPCChanged != null)
				{
					onNPCChanged.Invoke(npc);
				}
			}
		}

		public bool DoesNPCMatchRequirement(NPC npc)
		{
			if (!(TypeRequirement == null))
			{
				return npc.GetType() == TypeRequirement;
			}
			return true;
		}

		public override bool IsValueDefault()
		{
			return SelectedNPC == null;
		}

		public NPCFieldData GetData()
		{
			return new NPCFieldData((SelectedNPC != null) ? SelectedNPC.GUID.ToString() : "");
		}

		public void Load(NPCFieldData data)
		{
			if (data != null && !string.IsNullOrEmpty(data.NPCGuid))
			{
				NPC @object = GUIDManager.GetObject<NPC>(new Guid(data.NPCGuid));
				if (@object != null)
				{
					SetNPC(@object, network: true);
				}
			}
		}
	}
}
