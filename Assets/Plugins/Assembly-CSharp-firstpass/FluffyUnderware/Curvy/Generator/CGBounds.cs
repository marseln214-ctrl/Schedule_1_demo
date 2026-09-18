using UnityEngine;

namespace FluffyUnderware.Curvy.Generator
{
	[CGDataInfo(1f, 0.8f, 0.5f, 1f)]
	public class CGBounds : CGData
	{
		protected Bounds? mBounds;

		public Bounds Bounds
		{
			get
			{
				if (!mBounds.HasValue)
				{
					RecalculateBounds();
				}
				return mBounds.Value;
			}
			set
			{
				mBounds = value;
			}
		}

		public float Depth => Bounds.size.z;

		public CGBounds()
		{
		}

		public CGBounds(Bounds bounds)
		{
			Bounds = bounds;
		}

		public CGBounds(CGBounds source)
		{
			Name = source.Name;
			if (source.mBounds.HasValue)
			{
				Bounds = source.Bounds;
			}
		}

		public virtual void RecalculateBounds()
		{
			Bounds = default(Bounds);
		}

		public override T Clone<T>()
		{
			return new CGBounds(this) as T;
		}

		public static void Copy(CGBounds dest, CGBounds source)
		{
			if (source.mBounds.HasValue)
			{
				dest.Bounds = source.Bounds;
			}
		}
	}
}
