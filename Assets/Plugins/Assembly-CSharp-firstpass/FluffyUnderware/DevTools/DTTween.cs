using System;
using UnityEngine;

namespace FluffyUnderware.DevTools
{
	public static class DTTween
	{
		public enum EasingMethod
		{
			Linear = 0,
			ExponentialIn = 1,
			ExponentialOut = 2,
			ExponentialInOut = 3,
			ExponentialOutIn = 4,
			CircularIn = 5,
			CircularOut = 6,
			CircularInOut = 7,
			CircularOutIn = 8,
			QuadraticIn = 9,
			QuadraticOut = 10,
			QuadraticInOut = 11,
			QuadraticOutIn = 12,
			SinusIn = 13,
			SinusOut = 14,
			SinusInOut = 15,
			SinusOutIn = 16,
			CubicIn = 17,
			CubicOut = 18,
			CubicInOut = 19,
			CubicOutIn = 20,
			QuarticIn = 21,
			QuarticOut = 22,
			QuarticInOut = 23,
			QuarticOutIn = 24,
			QuinticIn = 25,
			QuinticOut = 26,
			QuinticInOut = 27,
			QuinticOutIn = 28
		}

		public static float Ease(EasingMethod method, float t, float b, float c)
		{
			return method switch
			{
				EasingMethod.ExponentialIn => ExpoIn(t, b, c), 
				EasingMethod.ExponentialOut => ExpoOut(t, b, c), 
				EasingMethod.ExponentialInOut => ExpoInOut(t, b, c), 
				EasingMethod.ExponentialOutIn => ExpoOutIn(t, b, c), 
				EasingMethod.CircularIn => CircIn(t, b, c), 
				EasingMethod.CircularOut => CircOut(t, b, c), 
				EasingMethod.CircularInOut => CircInOut(t, b, c), 
				EasingMethod.CircularOutIn => CircOutIn(t, b, c), 
				EasingMethod.QuadraticIn => QuadIn(t, b, c), 
				EasingMethod.QuadraticOut => QuadOut(t, b, c), 
				EasingMethod.QuadraticInOut => QuadInOut(t, b, c), 
				EasingMethod.QuadraticOutIn => QuadOutIn(t, b, c), 
				EasingMethod.SinusIn => SineIn(t, b, c), 
				EasingMethod.SinusOut => SineOut(t, b, c), 
				EasingMethod.SinusInOut => SineInOut(t, b, c), 
				EasingMethod.SinusOutIn => SineOutIn(t, b, c), 
				EasingMethod.CubicIn => CubicIn(t, b, c), 
				EasingMethod.CubicOut => CubicOut(t, b, c), 
				EasingMethod.CubicInOut => CubicInOut(t, b, c), 
				EasingMethod.CubicOutIn => CubicOutIn(t, b, c), 
				EasingMethod.QuarticIn => QuartIn(t, b, c), 
				EasingMethod.QuarticOut => QuartOut(t, b, c), 
				EasingMethod.QuarticInOut => QuartInOut(t, b, c), 
				EasingMethod.QuarticOutIn => QuartOutIn(t, b, c), 
				EasingMethod.QuinticIn => QuintIn(t, b, c), 
				EasingMethod.QuinticOut => QuintOut(t, b, c), 
				EasingMethod.QuinticInOut => QuintInOut(t, b, c), 
				EasingMethod.QuinticOutIn => QuintOutIn(t, b, c), 
				_ => Linear(t, b, c), 
			};
		}

		public static float Ease(EasingMethod method, float t, float b, float c, float d)
		{
			return method switch
			{
				EasingMethod.ExponentialIn => ExpoIn(t, b, c, d), 
				EasingMethod.ExponentialOut => ExpoOut(t, b, c, d), 
				EasingMethod.ExponentialInOut => ExpoInOut(t, b, c, d), 
				EasingMethod.ExponentialOutIn => ExpoOutIn(t, b, c, d), 
				EasingMethod.CircularIn => CircIn(t, b, c, d), 
				EasingMethod.CircularOut => CircOut(t, b, c, d), 
				EasingMethod.CircularInOut => CircInOut(t, b, c, d), 
				EasingMethod.CircularOutIn => CircOutIn(t, b, c, d), 
				EasingMethod.QuadraticIn => QuadIn(t, b, c, d), 
				EasingMethod.QuadraticOut => QuadOut(t, b, c, d), 
				EasingMethod.QuadraticInOut => QuadInOut(t, b, c, d), 
				EasingMethod.QuadraticOutIn => QuadOutIn(t, b, c, d), 
				EasingMethod.SinusIn => SineIn(t, b, c, d), 
				EasingMethod.SinusOut => SineOut(t, b, c, d), 
				EasingMethod.SinusInOut => SineInOut(t, b, c, d), 
				EasingMethod.SinusOutIn => SineOutIn(t, b, c, d), 
				EasingMethod.CubicIn => CubicIn(t, b, c, d), 
				EasingMethod.CubicOut => CubicOut(t, b, c, d), 
				EasingMethod.CubicInOut => CubicInOut(t, b, c, d), 
				EasingMethod.CubicOutIn => CubicOutIn(t, b, c, d), 
				EasingMethod.QuarticIn => QuartIn(t, b, c, d), 
				EasingMethod.QuarticOut => QuartOut(t, b, c, d), 
				EasingMethod.QuarticInOut => QuartInOut(t, b, c, d), 
				EasingMethod.QuarticOutIn => QuartOutIn(t, b, c, d), 
				EasingMethod.QuinticIn => QuintIn(t, b, c, d), 
				EasingMethod.QuinticOut => QuintOut(t, b, c, d), 
				EasingMethod.QuinticInOut => QuintInOut(t, b, c, d), 
				EasingMethod.QuinticOutIn => QuintOutIn(t, b, c, d), 
				_ => Linear(t, b, c, d), 
			};
		}

		public static float Linear(float t, float b, float c)
		{
			return c * Mathf.Clamp01(t) + b;
		}

		public static float Linear(float t, float b, float c, float d)
		{
			return c * t / d + b;
		}

		public static float ExpoOut(float t, float b, float c)
		{
			t = Mathf.Clamp01(t);
			if (t != 1f)
			{
				return c * (0f - Mathf.Pow(2f, -10f * t) + 1f) + b;
			}
			return b + c;
		}

		public static float ExpoOut(float t, float b, float c, float d)
		{
			if (t != d)
			{
				return c * (0f - Mathf.Pow(2f, -10f * t / d) + 1f) + b;
			}
			return b + c;
		}

		public static float ExpoIn(float t, float b, float c)
		{
			t = Mathf.Clamp01(t);
			if (t != 0f)
			{
				return c * Mathf.Pow(2f, 10f * (t - 1f)) + b;
			}
			return b;
		}

		public static float ExpoIn(float t, float b, float c, float d)
		{
			if (t != 0f)
			{
				return c * Mathf.Pow(2f, 10f * (t / d - 1f)) + b;
			}
			return b;
		}

		public static float ExpoInOut(float t, float b, float c)
		{
			t = Mathf.Clamp01(t);
			if (t == 0f)
			{
				return b;
			}
			if (t == 1f)
			{
				return b + c;
			}
			if ((t /= 0.5f) < 1f)
			{
				return c / 2f * Mathf.Pow(2f, 10f * (t - 1f)) + b;
			}
			return c / 2f * (0f - Mathf.Pow(2f, -10f * (t -= 1f)) + 2f) + b;
		}

		public static float ExpoInOut(float t, float b, float c, float d)
		{
			if (t == 0f)
			{
				return b;
			}
			if (t == d)
			{
				return b + c;
			}
			if ((t /= d / 2f) < 1f)
			{
				return c / 2f * Mathf.Pow(2f, 10f * (t - 1f)) + b;
			}
			return c / 2f * (0f - Mathf.Pow(2f, -10f * (t -= 1f)) + 2f) + b;
		}

		public static float ExpoOutIn(float t, float b, float c)
		{
			t = Mathf.Clamp01(t);
			if (t < 0.5f)
			{
				return ExpoOut(t * 2f, b, c / 2f);
			}
			return ExpoIn(t * 2f - 1f, b + c / 2f, c / 2f);
		}

		public static float ExpoOutIn(float t, float b, float c, float d)
		{
			if (t < d / 2f)
			{
				return ExpoOut(t * 2f, b, c / 2f, d);
			}
			return ExpoIn(t * 2f - d, b + c / 2f, c / 2f, d);
		}

		public static float CircOut(float t, float b, float c)
		{
			t = Mathf.Clamp01(t);
			return c * Mathf.Sqrt(1f - (t -= 1f) * t) + b;
		}

		public static float CircOut(float t, float b, float c, float d)
		{
			return c * Mathf.Sqrt(1f - (t = t / d - 1f) * t) + b;
		}

		public static float CircIn(float t, float b, float c)
		{
			t = Mathf.Clamp01(t);
			return (0f - c) * (Mathf.Sqrt(1f - t * t) - 1f) + b;
		}

		public static float CircIn(float t, float b, float c, float d)
		{
			return (0f - c) * (Mathf.Sqrt(1f - (t /= d) * t) - 1f) + b;
		}

		public static float CircInOut(float t, float b, float c)
		{
			t = Mathf.Clamp01(t);
			if ((t /= 0.5f) < 1f)
			{
				return (0f - c) / 2f * (Mathf.Sqrt(1f - t * t) - 1f) + b;
			}
			return c / 2f * (Mathf.Sqrt(1f - (t -= 2f) * t) + 1f) + b;
		}

		public static float CircInOut(float t, float b, float c, float d)
		{
			if ((t /= d / 2f) < 1f)
			{
				return (0f - c) / 2f * (Mathf.Sqrt(1f - t * t) - 1f) + b;
			}
			return c / 2f * (Mathf.Sqrt(1f - (t -= 2f) * t) + 1f) + b;
		}

		public static float CircOutIn(float t, float b, float c)
		{
			t = Mathf.Clamp01(t);
			if (t < 0.5f)
			{
				return CircOut(t * 2f, b, c / 2f);
			}
			return CircIn(t * 2f - 1f, b + c / 2f, c / 2f);
		}

		public static float CircOutIn(float t, float b, float c, float d)
		{
			if (t < d / 2f)
			{
				return CircOut(t * 2f, b, c / 2f, d);
			}
			return CircIn(t * 2f - d, b + c / 2f, c / 2f, d);
		}

		public static float QuadOut(float t, float b, float c)
		{
			t = Mathf.Clamp01(t);
			return (0f - c) * t * (t - 2f) + b;
		}

		public static float QuadOut(float t, float b, float c, float d)
		{
			return (0f - c) * (t /= d) * (t - 2f) + b;
		}

		public static float QuadIn(float t, float b, float c)
		{
			t = Mathf.Clamp01(t);
			return c * t * t + b;
		}

		public static float QuadIn(float t, float b, float c, float d)
		{
			return c * (t /= d) * t + b;
		}

		public static float QuadInOut(float t, float b, float c)
		{
			t = Mathf.Clamp01(t);
			if ((t /= 0.5f) < 1f)
			{
				return c / 2f * t * t + b;
			}
			return (0f - c) / 2f * ((t -= 1f) * (t - 2f) - 1f) + b;
		}

		public static float QuadInOut(float t, float b, float c, float d)
		{
			if ((t /= d / 2f) < 1f)
			{
				return c / 2f * t * t + b;
			}
			return (0f - c) / 2f * ((t -= 1f) * (t - 2f) - 1f) + b;
		}

		public static float QuadOutIn(float t, float b, float c)
		{
			t = Mathf.Clamp01(t);
			if (t < 0.5f)
			{
				return QuadOut(t * 2f, b, c / 2f);
			}
			return QuadIn(t * 2f - 1f, b + c / 2f, c / 2f);
		}

		public static float QuadOutIn(float t, float b, float c, float d)
		{
			if (t < d / 2f)
			{
				return QuadOut(t * 2f, b, c / 2f, d);
			}
			return QuadIn(t * 2f - d, b + c / 2f, c / 2f, d);
		}

		public static float SineOut(float t, float b, float c)
		{
			return c * Mathf.Sin(Mathf.Clamp01(t) * (MathF.PI / 2f)) + b;
		}

		public static float SineOut(float t, float b, float c, float d)
		{
			return c * Mathf.Sin(t / d * (MathF.PI / 2f)) + b;
		}

		public static float SineIn(float t, float b, float c)
		{
			return (0f - c) * Mathf.Cos(Mathf.Clamp01(t) * (MathF.PI / 2f)) + c + b;
		}

		public static float SineIn(float t, float b, float c, float d)
		{
			return (0f - c) * Mathf.Cos(t / d * (MathF.PI / 2f)) + c + b;
		}

		public static float SineInOut(float t, float b, float c)
		{
			t = Mathf.Clamp01(t);
			if ((t /= 0.5f) < 1f)
			{
				return c / 2f * Mathf.Sin(MathF.PI * t / 2f) + b;
			}
			return (0f - c) / 2f * (Mathf.Cos(MathF.PI * (t -= 1f) / 2f) - 2f) + b;
		}

		public static float SineInOut(float t, float b, float c, float d)
		{
			if ((t /= d / 2f) < 1f)
			{
				return c / 2f * Mathf.Sin(MathF.PI * t / 2f) + b;
			}
			return (0f - c) / 2f * (Mathf.Cos(MathF.PI * (t -= 1f) / 2f) - 2f) + b;
		}

		public static float SineOutIn(float t, float b, float c)
		{
			t = Mathf.Clamp01(t);
			if (t < 0.5f)
			{
				return SineOut(t * 2f, b, c / 2f);
			}
			return SineIn(t * 2f - 1f, b + c / 2f, c / 2f);
		}

		public static float SineOutIn(float t, float b, float c, float d)
		{
			if (t < d / 2f)
			{
				return SineOut(t * 2f, b, c / 2f, d);
			}
			return SineIn(t * 2f - d, b + c / 2f, c / 2f, d);
		}

		public static float CubicOut(float t, float b, float c)
		{
			t = Mathf.Clamp01(t);
			return c * ((t -= 1f) * t * t + 1f) + b;
		}

		public static float CubicOut(float t, float b, float c, float d)
		{
			return c * ((t = t / d - 1f) * t * t + 1f) + b;
		}

		public static float CubicIn(float t, float b, float c)
		{
			t = Mathf.Clamp01(t);
			return c * t * t * t + b;
		}

		public static float CubicIn(float t, float b, float c, float d)
		{
			return c * (t /= d) * t * t + b;
		}

		public static float CubicInOut(float t, float b, float c)
		{
			t = Mathf.Clamp01(t);
			if ((t /= 0.5f) < 1f)
			{
				return c / 2f * t * t * t + b;
			}
			return c / 2f * ((t -= 2f) * t * t + 2f) + b;
		}

		public static float CubicInOut(float t, float b, float c, float d)
		{
			if ((t /= d / 2f) < 1f)
			{
				return c / 2f * t * t * t + b;
			}
			return c / 2f * ((t -= 2f) * t * t + 2f) + b;
		}

		public static float CubicOutIn(float t, float b, float c)
		{
			t = Mathf.Clamp01(t);
			if (t < 0.5f)
			{
				return CubicOut(t * 2f, b, c / 2f);
			}
			return CubicIn(t * 2f - 1f, b + c / 2f, c / 2f);
		}

		public static float CubicOutIn(float t, float b, float c, float d)
		{
			if (t < d / 2f)
			{
				return CubicOut(t * 2f, b, c / 2f, d);
			}
			return CubicIn(t * 2f - d, b + c / 2f, c / 2f, d);
		}

		public static float QuartOut(float t, float b, float c)
		{
			t = Mathf.Clamp01(t);
			return (0f - c) * ((t -= 1f) * t * t * t - 1f) + b;
		}

		public static float QuartOut(float t, float b, float c, float d)
		{
			return (0f - c) * ((t = t / d - 1f) * t * t * t - 1f) + b;
		}

		public static float QuartIn(float t, float b, float c)
		{
			t = Mathf.Clamp01(t);
			return c * t * t * t * t + b;
		}

		public static float QuartIn(float t, float b, float c, float d)
		{
			return c * (t /= d) * t * t * t + b;
		}

		public static float QuartInOut(float t, float b, float c)
		{
			t = Mathf.Clamp01(t);
			if ((t /= 0.5f) < 1f)
			{
				return c / 2f * t * t * t * t + b;
			}
			return (0f - c) / 2f * ((t -= 2f) * t * t * t - 2f) + b;
		}

		public static float QuartInOut(float t, float b, float c, float d)
		{
			if ((t /= d / 2f) < 1f)
			{
				return c / 2f * t * t * t * t + b;
			}
			return (0f - c) / 2f * ((t -= 2f) * t * t * t - 2f) + b;
		}

		public static float QuartOutIn(float t, float b, float c)
		{
			t = Mathf.Clamp01(t);
			if (t < 0.5f)
			{
				return QuartOut(t * 2f, b, c / 2f);
			}
			return QuartIn(t * 2f - 1f, b + c / 2f, c / 2f);
		}

		public static float QuartOutIn(float t, float b, float c, float d)
		{
			if (t < d / 2f)
			{
				return QuartOut(t * 2f, b, c / 2f, d);
			}
			return QuartIn(t * 2f - d, b + c / 2f, c / 2f, d);
		}

		public static float QuintOut(float t, float b, float c)
		{
			t = Mathf.Clamp01(t);
			return c * ((t -= 1f) * t * t * t * t + 1f) + b;
		}

		public static float QuintOut(float t, float b, float c, float d)
		{
			return c * ((t = t / d - 1f) * t * t * t * t + 1f) + b;
		}

		public static float QuintIn(float t, float b, float c)
		{
			t = Mathf.Clamp01(t);
			return c * t * t * t * t * t + b;
		}

		public static float QuintIn(float t, float b, float c, float d)
		{
			return c * (t /= d) * t * t * t * t + b;
		}

		public static float QuintInOut(float t, float b, float c)
		{
			t = Mathf.Clamp01(t);
			if ((t /= 0.5f) < 1f)
			{
				return c / 2f * t * t * t * t * t + b;
			}
			return c / 2f * ((t -= 2f) * t * t * t * t + 2f) + b;
		}

		public static float QuintInOut(float t, float b, float c, float d)
		{
			if ((t /= d / 2f) < 1f)
			{
				return c / 2f * t * t * t * t * t + b;
			}
			return c / 2f * ((t -= 2f) * t * t * t * t + 2f) + b;
		}

		public static float QuintOutIn(float t, float b, float c)
		{
			t = Mathf.Clamp01(t);
			if (t < 0.5f)
			{
				return QuintOut(t * 2f, b, c / 2f);
			}
			return QuintIn(t * 2f - 1f, b + c / 2f, c / 2f);
		}

		public static float QuintOutIn(float t, float b, float c, float d)
		{
			if (t < d / 2f)
			{
				return QuintOut(t * 2f, b, c / 2f, d);
			}
			return QuintIn(t * 2f - d, b + c / 2f, c / 2f, d);
		}
	}
}
