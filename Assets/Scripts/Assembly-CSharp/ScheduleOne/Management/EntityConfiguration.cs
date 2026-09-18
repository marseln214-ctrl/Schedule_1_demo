using System.Collections.Generic;
using FishNet.Connection;
using UnityEngine.Events;

namespace ScheduleOne.Management
{
	public class EntityConfiguration
	{
		public List<ConfigField> Fields = new List<ConfigField>();

		public UnityEvent onChanged = new UnityEvent();

		public ConfigurationReplicator Replicator { get; protected set; }

		public IConfigurable Configurable { get; protected set; }

		public bool IsSelected { get; protected set; }

		public EntityConfiguration(ConfigurationReplicator replicator, IConfigurable configurable)
		{
			Replicator = replicator;
			Replicator.Configuration = this;
			Configurable = configurable;
		}

		protected void InvokeChanged()
		{
			if (onChanged != null)
			{
				onChanged.Invoke();
			}
		}

		public void ReplicateField(ConfigField field, NetworkConnection conn = null)
		{
			Replicator.ReplicateField(field, conn);
		}

		public void ReplicateAllFields(NetworkConnection conn = null, bool replicateDefaults = true)
		{
			foreach (ConfigField field in Fields)
			{
				if (replicateDefaults || !field.IsValueDefault())
				{
					ReplicateField(field, conn);
				}
			}
		}

		public virtual void Destroy()
		{
		}

		public virtual void Selected()
		{
			IsSelected = true;
		}

		public virtual void Deselected()
		{
			IsSelected = false;
		}

		public virtual bool ShouldSave()
		{
			return false;
		}

		public virtual string GetSaveString()
		{
			return string.Empty;
		}
	}
}
