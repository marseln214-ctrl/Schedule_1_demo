using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace AmplifyColor
{
	[Serializable]
	public class VolumeEffectComponent
	{
		public string componentName;

		public List<VolumeEffectField> fields;

		public VolumeEffectComponent(string name)
		{
			componentName = name;
			fields = new List<VolumeEffectField>();
		}

		public VolumeEffectField AddField(FieldInfo pi, Component c)
		{
			return AddField(pi, c, -1);
		}

		public VolumeEffectField AddField(FieldInfo pi, Component c, int position)
		{
			VolumeEffectField volumeEffectField = (VolumeEffectField.IsValidType(pi.FieldType.FullName) ? new VolumeEffectField(pi, c) : null);
			if (volumeEffectField != null)
			{
				if (position < 0 || position >= fields.Count)
				{
					fields.Add(volumeEffectField);
				}
				else
				{
					fields.Insert(position, volumeEffectField);
				}
			}
			return volumeEffectField;
		}

		public void RemoveEffectField(VolumeEffectField field)
		{
			fields.Remove(field);
		}

		public VolumeEffectComponent(Component c, VolumeEffectComponentFlags compFlags)
			: this(compFlags.componentName)
		{
			foreach (VolumeEffectFieldFlags componentField in compFlags.componentFields)
			{
				if (componentField.blendFlag)
				{
					FieldInfo field = c.GetType().GetField(componentField.fieldName);
					VolumeEffectField volumeEffectField = (VolumeEffectField.IsValidType(field.FieldType.FullName) ? new VolumeEffectField(field, c) : null);
					if (volumeEffectField != null)
					{
						fields.Add(volumeEffectField);
					}
				}
			}
		}

		public void UpdateComponent(Component c, VolumeEffectComponentFlags compFlags)
		{
			foreach (VolumeEffectFieldFlags fieldFlags in compFlags.componentFields)
			{
				if (fieldFlags.blendFlag && !fields.Exists((VolumeEffectField s) => s.fieldName == fieldFlags.fieldName))
				{
					FieldInfo field = c.GetType().GetField(fieldFlags.fieldName);
					VolumeEffectField volumeEffectField = (VolumeEffectField.IsValidType(field.FieldType.FullName) ? new VolumeEffectField(field, c) : null);
					if (volumeEffectField != null)
					{
						fields.Add(volumeEffectField);
					}
				}
			}
		}

		public VolumeEffectField FindEffectField(string fieldName)
		{
			for (int i = 0; i < fields.Count; i++)
			{
				if (fields[i].fieldName == fieldName)
				{
					return fields[i];
				}
			}
			return null;
		}

		public static FieldInfo[] ListAcceptableFields(Component c)
		{
			if (c == null)
			{
				return new FieldInfo[0];
			}
			return (from f in c.GetType().GetFields()
				where VolumeEffectField.IsValidType(f.FieldType.FullName)
				select f).ToArray();
		}

		public string[] GetFieldNames()
		{
			return fields.Select((VolumeEffectField r) => r.fieldName).ToArray();
		}
	}
}
