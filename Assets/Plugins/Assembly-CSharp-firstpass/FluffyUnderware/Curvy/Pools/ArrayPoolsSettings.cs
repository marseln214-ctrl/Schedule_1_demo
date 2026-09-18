using System;
using FluffyUnderware.DevTools;
using JetBrains.Annotations;
using UnityEngine;

namespace FluffyUnderware.Curvy.Pools
{
	[HelpURL("https://curvyeditor.com/doclink/arraypoolsettings")]
	[ExecuteAlways]
	[DisallowMultipleComponent]
	public class ArrayPoolsSettings : DTVersionedMonoBehaviour
	{
		[SerializeField]
		[Tooltip("The maximal number of elements of type Vector2 allowed to be stored in the arrays' pool waiting to be reused")]
		private long vector2Capacity = 100000L;

		[SerializeField]
		[Tooltip("The maximal number of elements of type Vector3 allowed to be stored in the arrays' pool waiting to be reused")]
		private long vector3Capacity = 100000L;

		[SerializeField]
		[Tooltip("The maximal number of elements of type Vector4 allowed to be stored in the arrays' pool waiting to be reused")]
		private long vector4Capacity = 100000L;

		[SerializeField]
		[Tooltip("The maximal number of elements of type Int32 allowed to be stored in the arrays' pool waiting to be reused")]
		private long intCapacity = 100000L;

		[SerializeField]
		[Tooltip("The maximal number of elements of type Single (a.k.a float) allowed to be stored in the arrays' pool waiting to be reused")]
		private long floatCapacity = 10000L;

		[SerializeField]
		[Tooltip("The maximal number of elements of type CGSpots allowed to be stored in the arrays' pool waiting to be reused")]
		private long cgSpotCapacity = 10000L;

		[Tooltip("Log in the console each time an array pool allocates a new array in memory")]
		[SerializeField]
		private bool logAllocations;

		public long Vector2Capacity
		{
			get
			{
				return vector2Capacity;
			}
			set
			{
				vector2Capacity = Math.Max(0L, value);
				if (base.IsActiveAndEnabled)
				{
					ArrayPools.Vector2.ElementsCapacity = vector2Capacity;
				}
			}
		}

		public long Vector3Capacity
		{
			get
			{
				return vector3Capacity;
			}
			set
			{
				vector3Capacity = Math.Max(0L, value);
				if (base.IsActiveAndEnabled)
				{
					ArrayPools.Vector3.ElementsCapacity = vector3Capacity;
				}
			}
		}

		public long Vector4Capacity
		{
			get
			{
				return vector4Capacity;
			}
			set
			{
				vector4Capacity = Math.Max(0L, value);
				if (base.IsActiveAndEnabled)
				{
					ArrayPools.Vector4.ElementsCapacity = vector4Capacity;
				}
			}
		}

		public long IntCapacity
		{
			get
			{
				return intCapacity;
			}
			set
			{
				intCapacity = Math.Max(0L, value);
				if (base.IsActiveAndEnabled)
				{
					ArrayPools.Int32.ElementsCapacity = IntCapacity;
				}
			}
		}

		public long FloatCapacity
		{
			get
			{
				return floatCapacity;
			}
			set
			{
				floatCapacity = Math.Max(0L, value);
				if (base.IsActiveAndEnabled)
				{
					ArrayPools.Single.ElementsCapacity = floatCapacity;
				}
			}
		}

		public long CGSpotCapacity
		{
			get
			{
				return cgSpotCapacity;
			}
			set
			{
				cgSpotCapacity = Math.Max(0L, value);
				if (base.IsActiveAndEnabled)
				{
					ArrayPools.CGSpot.ElementsCapacity = cgSpotCapacity;
				}
			}
		}

		public bool LogAllocations
		{
			get
			{
				return logAllocations;
			}
			set
			{
				logAllocations = value;
				ArrayPools.CGSpot.LogAllocations = logAllocations;
				ArrayPools.Int32.LogAllocations = logAllocations;
				ArrayPools.Single.LogAllocations = logAllocations;
				ArrayPools.Vector2.LogAllocations = logAllocations;
				ArrayPools.Vector3.LogAllocations = logAllocations;
				ArrayPools.Vector4.LogAllocations = logAllocations;
			}
		}

		protected override void OnValidate()
		{
			base.OnValidate();
			ValidateAndApply();
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			ValidateAndApply();
		}

		[UsedImplicitly]
		private void Start()
		{
			ValidateAndApply();
		}

		private void ValidateAndApply()
		{
			Vector2Capacity = vector2Capacity;
			Vector3Capacity = vector3Capacity;
			Vector4Capacity = vector4Capacity;
			IntCapacity = intCapacity;
			FloatCapacity = floatCapacity;
			CGSpotCapacity = cgSpotCapacity;
			LogAllocations = logAllocations;
		}
	}
}
