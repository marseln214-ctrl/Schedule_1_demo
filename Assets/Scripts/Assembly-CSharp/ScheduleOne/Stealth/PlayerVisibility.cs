using System.Collections.Generic;
using System.Linq;
using FishNet.Object;
using ScheduleOne.DevUtilities;
using ScheduleOne.FX;
using ScheduleOne.NPCs;
using ScheduleOne.Vehicles;
using ScheduleOne.Vision;
using UnityEngine;

namespace ScheduleOne.Stealth
{
	public class PlayerVisibility : NetworkBehaviour
	{
		public const float MAX_VISIBLITY = 100f;

		public float CurrentVisibility;

		public List<VisibilityAttribute> activeAttributes = new List<VisibilityAttribute>();

		public List<VisibilityAttribute> filteredAttributes = new List<VisibilityAttribute>();

		[Header("Settings")]
		public LayerMask visibilityCheckMask;

		[Header("References")]
		public List<Transform> visibilityPoints = new List<Transform>();

		private VisibilityAttribute environmentalVisibility;

		private List<RaycastHit> hits;

		private bool NetworkInitialize___EarlyScheduleOne_002EStealth_002EPlayerVisibilityAssembly_002DCSharp_002Edll_Excuted;

		private bool NetworkInitialize__LateScheduleOne_002EStealth_002EPlayerVisibilityAssembly_002DCSharp_002Edll_Excuted;

		public VisionEvent HighestVisionEvent { get; set; }

		public override void OnStartClient()
		{
			base.OnStartClient();
			if (base.IsOwner)
			{
				environmentalVisibility = new VisibilityAttribute("Environmental Brightess", 0f);
			}
		}

		private void FixedUpdate()
		{
			UpdateEnvironmentalVisibilityAttribute();
			CurrentVisibility = CalculateVisibility();
		}

		private float CalculateVisibility()
		{
			float num = 0f;
			Dictionary<string, float> maxPointsChangesByUniquenessCode = (from UniqueVisibilityAttribute uva in activeAttributes.Where((VisibilityAttribute a) => a is UniqueVisibilityAttribute)
				group uva by uva.uniquenessCode).ToDictionary((IGrouping<string, UniqueVisibilityAttribute> group) => group.Key, (IGrouping<string, UniqueVisibilityAttribute> group) => group.Max((UniqueVisibilityAttribute uva) => uva.pointsChange));
			filteredAttributes = activeAttributes.Where((VisibilityAttribute attr) => !(attr is UniqueVisibilityAttribute) || (attr is UniqueVisibilityAttribute uniqueVisibilityAttribute && uniqueVisibilityAttribute.pointsChange >= maxPointsChangesByUniquenessCode.GetValueOrDefault(uniqueVisibilityAttribute.uniquenessCode, 0f))).ToList();
			for (int i = 0; i < filteredAttributes.Count; i++)
			{
				num += filteredAttributes[i].pointsChange;
				if (filteredAttributes[i].multiplier != 1f)
				{
					num *= filteredAttributes[i].multiplier;
				}
			}
			return Mathf.Clamp(num, 0f, 100f);
		}

		public VisibilityAttribute GetAttribute(string name)
		{
			return activeAttributes.Find((VisibilityAttribute x) => x.name.ToLower() == name.ToLower());
		}

		private void UpdateEnvironmentalVisibilityAttribute()
		{
			if (environmentalVisibility != null)
			{
				environmentalVisibility.multiplier = Singleton<EnvironmentFX>.Instance.normalizedEnvironmentalBrightness;
			}
		}

		public float CalculateExposureToPoint(Vector3 point, float checkRange = 50f, NPC checkingNPC = null)
		{
			float num = 0f;
			if (Vector3.Distance(point, base.transform.position) > checkRange + 1f)
			{
				return 0f;
			}
			List<VisionObscurer> list = new List<VisionObscurer>();
			foreach (Transform visibilityPoint in visibilityPoints)
			{
				float num2 = Vector3.Distance(point, visibilityPoint.position);
				if (num2 > checkRange)
				{
					continue;
				}
				hits = Physics.RaycastAll(point, (visibilityPoint.position - point).normalized, Mathf.Min(checkRange, num2), visibilityCheckMask, QueryTriggerInteraction.Collide).ToList();
				for (int i = 0; i < hits.Count; i++)
				{
					LandVehicle componentInParent = hits[i].collider.GetComponentInParent<LandVehicle>();
					if (checkingNPC != null && componentInParent != null)
					{
						if (checkingNPC.CurrentVehicle == componentInParent)
						{
							hits.RemoveAt(i);
							i--;
						}
						continue;
					}
					VisionObscurer componentInParent2 = hits[i].collider.GetComponentInParent<VisionObscurer>();
					if (componentInParent2 != null)
					{
						if (visibilityPoint == visibilityPoints[1] && !list.Contains(componentInParent2))
						{
							list.Add(componentInParent2);
						}
						hits.RemoveAt(i);
						i--;
					}
					else if (hits[i].collider.isTrigger)
					{
						hits.RemoveAt(i);
						i--;
					}
				}
				if (hits.Count > 0)
				{
					Debug.DrawRay(point, hits[0].point - point, Color.red, 0.1f);
					continue;
				}
				Debug.DrawRay(point, (visibilityPoint.position - point).normalized * num2, Color.green, 0.1f);
				num += 1f / (float)visibilityPoints.Count;
			}
			float num3 = 1f;
			for (int j = 0; j < list.Count; j++)
			{
				num3 *= 1f - list[j].ObscuranceAmount;
			}
			_ = 1f;
			return num * num3;
		}

		public virtual void NetworkInitialize___Early()
		{
			if (!NetworkInitialize___EarlyScheduleOne_002EStealth_002EPlayerVisibilityAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize___EarlyScheduleOne_002EStealth_002EPlayerVisibilityAssembly_002DCSharp_002Edll_Excuted = true;
			}
		}

		public virtual void NetworkInitialize__Late()
		{
			if (!NetworkInitialize__LateScheduleOne_002EStealth_002EPlayerVisibilityAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize__LateScheduleOne_002EStealth_002EPlayerVisibilityAssembly_002DCSharp_002Edll_Excuted = true;
			}
		}

		public override void NetworkInitializeIfDisabled()
		{
			NetworkInitialize___Early();
			NetworkInitialize__Late();
		}

		public virtual void Awake()
		{
			NetworkInitialize___Early();
			NetworkInitialize__Late();
		}
	}
}
