using System;
using System.Collections.Generic;
using FluffyUnderware.Curvy.Pools;
using JetBrains.Annotations;
using ToolBuddy.Pooling.Collections;

namespace FluffyUnderware.Curvy.Generator
{
	[CGDataInfo(0.96f, 0.96f, 0.96f, 1f)]
	public class CGSpots : CGData
	{
		private SubArray<CGSpot> spots;

		public SubArray<CGSpot> Spots
		{
			get
			{
				return spots;
			}
			set
			{
				ArrayPools.CGSpot.Free(spots);
				spots = value;
			}
		}

		[UsedImplicitly]
		[Obsolete("Use Spots instead")]
		public CGSpot[] Points
		{
			get
			{
				return Spots.CopyToArray(ArrayPools.CGSpot);
			}
			set
			{
				Spots = new SubArray<CGSpot>(value);
			}
		}

		public override int Count => spots.Count;

		public CGSpots()
		{
			spots = ArrayPools.CGSpot.Allocate(0);
		}

		public CGSpots(params CGSpot[] points)
		{
			spots = new SubArray<CGSpot>(points);
		}

		public CGSpots(SubArray<CGSpot> spots)
		{
			this.spots = spots;
		}

		public CGSpots(List<CGSpot> spots)
		{
			this.spots = ArrayPools.CGSpot.Allocate(spots.Count);
			spots.CopyTo(0, this.spots.Array, 0, spots.Count);
		}

		public CGSpots(params List<CGSpot>[] spots)
		{
			int num = 0;
			for (int i = 0; i < spots.Length; i++)
			{
				num += spots[i].Count;
			}
			this.spots = ArrayPools.CGSpot.Allocate(num);
			num = 0;
			foreach (List<CGSpot> list in spots)
			{
				list.CopyTo(0, this.spots.Array, num, list.Count);
				num += list.Count;
			}
		}

		public CGSpots(CGSpots source)
		{
			spots = ArrayPools.CGSpot.Clone(source.spots);
		}

		protected override bool Dispose(bool disposing)
		{
			bool num = base.Dispose(disposing);
			if (num)
			{
				ArrayPools.CGSpot.Free(spots);
			}
			return num;
		}

		public override T Clone<T>()
		{
			return new CGSpots(this) as T;
		}
	}
}
