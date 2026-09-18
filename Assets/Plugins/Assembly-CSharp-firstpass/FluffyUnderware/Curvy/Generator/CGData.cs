using System;
using FluffyUnderware.Curvy.Utils;
using FluffyUnderware.DevTools;
using ToolBuddy.Pooling.Collections;

namespace FluffyUnderware.Curvy.Generator
{
	public class CGData : IDisposable
	{
		private bool disposed;

		public string Name;

		public virtual int Count => 0;

		protected virtual bool Dispose(bool disposing)
		{
			if (disposed)
			{
				DTLog.LogWarning("[Curvy] Attempt to dispose a CGData twice. Please raise a bug report.");
				return false;
			}
			disposed = true;
			return true;
		}

		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}

		~CGData()
		{
			Dispose(disposing: false);
		}

		public static implicit operator bool(CGData a)
		{
			return a != null;
		}

		public virtual T Clone<T>() where T : CGData
		{
			return new CGData() as T;
		}

		protected int getGenericFIndex(SubArray<float> FMapArray, float fValue, out float frag)
		{
			int num = CurvyUtility.InterpolationSearch(FMapArray.Array, FMapArray.Count, fValue);
			if (num == FMapArray.Count - 1)
			{
				num--;
				frag = 1f;
			}
			else
			{
				frag = (fValue - FMapArray.Array[num]) / (FMapArray.Array[num + 1] - FMapArray.Array[num]);
			}
			return num;
		}
	}
}
