using System.Runtime.CompilerServices;
using FluffyUnderware.Curvy.Utils;
using JetBrains.Annotations;
using UnityEngine;

namespace FluffyUnderware.Curvy
{
	public static class BSplineHelper
	{
		public static Vector3 DeBoorClamped(int p, int k, float u, int nPlus1, [NotNull] Vector3[] pArray)
		{
			int num = k - p;
			int num2 = nPlus1 - p;
			for (int i = 1; i <= p; i++)
			{
				int num3 = k + 1 - i;
				for (int num4 = p; num4 >= i; num4--)
				{
					int num5 = num4 + num;
					int num6 = num4 + num3;
					int num7;
					int num8;
					if (num5 <= p)
					{
						num7 = 0;
						num8 = ((num6 > p) ? ((num6 >= nPlus1) ? num2 : (num6 - p)) : 0);
					}
					else if (num5 >= nPlus1)
					{
						num7 = (num8 = num2);
					}
					else
					{
						num7 = num5 - p;
						num8 = ((num6 >= nPlus1) ? num2 : (num6 - p));
					}
					float num9 = (u - (float)num7) / (float)(num8 - num7);
					pArray[num4] = pArray[num4 - 1].Multiply(1f - num9).Addition(pArray[num4].Multiply(num9));
				}
			}
			return pArray[p];
		}

		public static Vector3 DeBoorUnclamped(int p, int k, float u, [NotNull] Vector3[] pArray)
		{
			int num = k - p;
			for (int i = 1; i <= p; i++)
			{
				int num2 = k + 1 - i;
				for (int num3 = p; num3 >= i; num3--)
				{
					float num4 = (u - (float)(num3 + num)) / (float)(num3 + num2 - (num3 + num));
					pArray[num3] = pArray[num3 - 1].Multiply(1f - num4).Addition(pArray[num3].Multiply(num4));
				}
			}
			return pArray[p];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int GetBSplineN(int controlPointsCount, int degree, bool closed)
		{
			return controlPointsCount - 1 + (closed ? degree : 0);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void GetBSplineUAndK(float tf, bool isClamped, int p, int n, out float u, out int k)
		{
			if (isClamped)
			{
				u = (float)(n - p + 1) * tf;
				int num = (int)u;
				if (num == n - p + 1)
				{
					num--;
				}
				k = num + p;
			}
			else
			{
				u = (float)p + (float)(n + 1 - p) * tf;
				int num2 = (int)u;
				if (num2 == n + 1)
				{
					num2--;
				}
				k = num2;
			}
		}
	}
}
