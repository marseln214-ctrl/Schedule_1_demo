using System;
using FishNet.Object;
using FishNet.Serializing.Helping;
using ScheduleOne.PlayerScripts;
using UnityEngine;

namespace ScheduleOne.Combat
{
	[Serializable]
	public class Impact
	{
		[CodegenExclude]
		public RaycastHit Hit;

		public Vector3 HitPoint;

		public Vector3 ImpactForceDirection;

		public float ImpactForce;

		public float ImpactDamage;

		public EImpactType ImpactType;

		public NetworkObject ImpactSource;

		public int ImpactID;

		public Impact(RaycastHit hit, Vector3 hitPoint, Vector3 impactForceDirection, float impactForce, float impactDamage, EImpactType impactType, Player impactSource, int impactID)
		{
			Hit = hit;
			HitPoint = hitPoint;
			ImpactForceDirection = impactForceDirection;
			ImpactForce = impactForce;
			ImpactDamage = impactDamage;
			ImpactType = impactType;
			if (impactSource != null)
			{
				ImpactSource = impactSource.NetworkObject;
			}
			ImpactID = impactID;
		}

		public Impact()
		{
		}

		public static bool IsLethal(EImpactType impactType)
		{
			if (impactType == EImpactType.SharpMetal || impactType == EImpactType.Bullet)
			{
				return true;
			}
			return false;
		}

		public bool IsPlayerImpact(out Player player)
		{
			if (ImpactSource == null)
			{
				player = null;
				return false;
			}
			player = ImpactSource.GetComponent<Player>();
			return player != null;
		}
	}
}
