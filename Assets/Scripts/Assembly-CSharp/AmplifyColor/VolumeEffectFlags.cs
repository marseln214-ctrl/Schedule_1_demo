using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AmplifyColor
{
	[Serializable]
	public class VolumeEffectFlags
	{
		public List<VolumeEffectComponentFlags> components;

		public VolumeEffectFlags()
		{
			components = new List<VolumeEffectComponentFlags>();
		}

		public void AddComponent(Component c)
		{
			VolumeEffectComponentFlags volumeEffectComponentFlags;
			if ((volumeEffectComponentFlags = components.Find((VolumeEffectComponentFlags s) => s.componentName == (c.GetType()?.ToString() ?? ""))) != null)
			{
				volumeEffectComponentFlags.UpdateComponentFlags(c);
			}
			else
			{
				components.Add(new VolumeEffectComponentFlags(c));
			}
		}

		public void UpdateFlags(VolumeEffect effectVol)
		{
			foreach (VolumeEffectComponent comp in effectVol.components)
			{
				VolumeEffectComponentFlags volumeEffectComponentFlags = null;
				if ((volumeEffectComponentFlags = components.Find((VolumeEffectComponentFlags s) => s.componentName == comp.componentName)) == null)
				{
					components.Add(new VolumeEffectComponentFlags(comp));
				}
				else
				{
					volumeEffectComponentFlags.UpdateComponentFlags(comp);
				}
			}
		}

		public static void UpdateCamFlags(AmplifyColorEffect[] effects, AmplifyColorVolumeBase[] volumes)
		{
			foreach (AmplifyColorEffect amplifyColorEffect in effects)
			{
				amplifyColorEffect.EffectFlags = new VolumeEffectFlags();
				for (int j = 0; j < volumes.Length; j++)
				{
					VolumeEffect volumeEffect = volumes[j].EffectContainer.FindVolumeEffect(amplifyColorEffect);
					if (volumeEffect != null)
					{
						amplifyColorEffect.EffectFlags.UpdateFlags(volumeEffect);
					}
				}
			}
		}

		public VolumeEffect GenerateEffectData(AmplifyColorEffect go)
		{
			VolumeEffect volumeEffect = new VolumeEffect(go);
			foreach (VolumeEffectComponentFlags component2 in components)
			{
				if (component2.blendFlag)
				{
					Component component = go.GetComponent(component2.componentName);
					if (component != null)
					{
						volumeEffect.AddComponent(component, component2);
					}
				}
			}
			return volumeEffect;
		}

		public VolumeEffectComponentFlags FindComponentFlags(string compName)
		{
			for (int i = 0; i < components.Count; i++)
			{
				if (components[i].componentName == compName)
				{
					return components[i];
				}
			}
			return null;
		}

		public string[] GetComponentNames()
		{
			return (from r in components
				where r.blendFlag
				select r.componentName).ToArray();
		}
	}
}
