#define THREADING_SUPPORTED
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using JetBrains.Annotations;

namespace FluffyUnderware.DevTools
{
	public class ThreadPoolWorker<T> : IDisposable
	{
		private readonly SimplePool<QueuedCallback> queuedCallbackPool = new SimplePool<QueuedCallback>(4);

		private readonly SimplePool<LoopState<T>> loopStatePool = new SimplePool<LoopState<T>>(4);

		private int _remainingWorkItems = 1;

		private ManualResetEvent _done = new ManualResetEvent(initialState: false);

		private readonly WaitCallback handleWorkItemCallBack;

		private readonly WaitCallback handleLoopCallBack;

		private static int ThreadsToUseCount
		{
			get
			{
				ThreadPool.GetAvailableThreads(out var workerThreads, out var _);
				return 1 + Math.Min(workerThreads, System.Environment.ProcessorCount - 1);
			}
		}

		public ThreadPoolWorker()
		{
			handleWorkItemCallBack = delegate(object o)
			{
				QueuedCallback queuedCallback = (QueuedCallback)o;
				try
				{
					queuedCallback.Callback(queuedCallback.State);
				}
				finally
				{
					lock (queuedCallbackPool)
					{
						queuedCallbackPool.ReleaseItem(queuedCallback);
					}
					DoneWorkItem();
				}
			};
			handleLoopCallBack = delegate(object state)
			{
				LoopState<T> loopState = (LoopState<T>)state;
				for (int i = loopState.StartIndex; i <= loopState.EndIndex; i++)
				{
					loopState.Action(loopState.Items.ElementAt(i), i, loopState.ItemsCount);
				}
				lock (loopStatePool)
				{
					loopStatePool.ReleaseItem(loopState);
				}
			};
		}

		[UsedImplicitly]
		[Obsolete("Use ParallelFor(Action<T,int,int> action, IEnumerable<T> list) instead")]
		public void ParralelFor(Action<T> action, List<T> list)
		{
			ParallelFor(delegate(T item, int itemIndex, int itemsCount)
			{
				action(item);
			}, list, list.Count());
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void ParallelFor(Action<T, int, int> action, IEnumerable<T> list)
		{
			ParallelFor(action, list, list.Count());
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void ParallelFor(Action<T, int, int> action, IEnumerable<T> list, int elementsCount)
		{
			if (Environment.IsThreadingSupported)
			{
				DoParallelFor(action, list, elementsCount);
				return;
			}
			for (int i = 0; i < elementsCount; i++)
			{
				action(list.ElementAt(i), i, elementsCount);
			}
		}

		public void Dispose()
		{
			if (_done != null)
			{
				((IDisposable)_done).Dispose();
				_done = null;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[System.Diagnostics.Conditional("THREADING_SUPPORTED")]
		private void DoParallelFor(Action<T, int, int> action, IEnumerable<T> list, int elementsCount)
		{
			int threadsToUseCount = ThreadsToUseCount;
			int num = ((threadsToUseCount == 1) ? elementsCount : ((int)Math.Ceiling((float)elementsCount / (float)threadsToUseCount)));
			int num2 = 0;
			while (num2 < elementsCount)
			{
				int num3 = Math.Min(num2 + num - 1, elementsCount - 1);
				if (num3 == elementsCount - 1)
				{
					for (int i = num2; i <= num3; i++)
					{
						action(list.ElementAt(i), i, elementsCount);
					}
				}
				else
				{
					QueuedCallback item;
					lock (queuedCallbackPool)
					{
						item = queuedCallbackPool.GetItem();
					}
					LoopState<T> item2;
					lock (loopStatePool)
					{
						item2 = loopStatePool.GetItem();
					}
					item2.Set((short)num2, (short)num3, list, elementsCount, action);
					item.State = item2;
					item.Callback = handleLoopCallBack;
					ThrowIfDisposed();
					lock (_done)
					{
						_remainingWorkItems++;
					}
					ThreadPool.QueueUserWorkItem(handleWorkItemCallBack, item);
				}
				num2 = num3 + 1;
			}
			WaitAll(-1, exitContext: false);
		}

		private bool WaitAll(int millisecondsTimeout, bool exitContext)
		{
			ThrowIfDisposed();
			DoneWorkItem();
			bool flag = _done.WaitOne(millisecondsTimeout, exitContext);
			lock (_done)
			{
				if (flag)
				{
					_remainingWorkItems = 1;
					_done.Reset();
				}
				else
				{
					_remainingWorkItems++;
				}
			}
			return flag;
		}

		private void ThrowIfDisposed()
		{
			if (_done == null)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
		}

		private void DoneWorkItem()
		{
			lock (_done)
			{
				_remainingWorkItems--;
				if (_remainingWorkItems == 0)
				{
					_done.Set();
				}
			}
		}
	}
	[Obsolete("Use ThreadPoolWorker<T> instead")]
	public class ThreadPoolWorker : IDisposable
	{
		private int _remainingWorkItems = 1;

		private ManualResetEvent _done = new ManualResetEvent(initialState: false);

		public void QueueWorkItem(WaitCallback callback)
		{
			QueueWorkItem(callback, null);
		}

		public void QueueWorkItem(Action act)
		{
			QueueWorkItem(act, null);
		}

		public void ParralelFor<T>(Action<T> action, List<T> list)
		{
			ThreadPool.GetAvailableThreads(out var workerThreads, out var _);
			int num = 1 + Math.Min(workerThreads, System.Environment.ProcessorCount - 1);
			int iterationsCount = list.Count;
			if (num == 1 || iterationsCount == 1)
			{
				for (int i = 0; i < iterationsCount; i++)
				{
					action(list[i]);
				}
				return;
			}
			int num2 = (int)Math.Ceiling((float)iterationsCount / (float)num);
			int num3 = 0;
			while (num3 < iterationsCount)
			{
				QueuedCallback queuedCallback = new QueuedCallback();
				int num4 = Math.Min(num3 + num2, iterationsCount - 1);
				LoopState<T> state2 = new LoopState<T>((short)num3, (short)num4, list, iterationsCount, delegate(T item, int itemIndex, int itemsCount)
				{
					action(item);
				});
				queuedCallback.State = state2;
				queuedCallback.Callback = delegate(object state)
				{
					LoopState<T> loopState = (LoopState<T>)state;
					for (int j = loopState.StartIndex; j <= loopState.EndIndex; j++)
					{
						loopState.Action(loopState.Items.ElementAt(j), j, iterationsCount);
					}
				};
				QueueWorkItem(queuedCallback);
				num3 = num4 + 1;
			}
		}

		private void QueueWorkItem(QueuedCallback callback)
		{
			ThrowIfDisposed();
			lock (_done)
			{
				_remainingWorkItems++;
			}
			ThreadPool.QueueUserWorkItem(HandleWorkItem, callback);
		}

		public void QueueWorkItem(WaitCallback callback, object state)
		{
			QueuedCallback queuedCallback = new QueuedCallback();
			queuedCallback.Callback = callback;
			queuedCallback.State = state;
			QueueWorkItem(queuedCallback);
		}

		public void QueueWorkItem(Action act, object state)
		{
			QueuedCallback queuedCallback = new QueuedCallback();
			queuedCallback.Callback = delegate
			{
				act();
			};
			queuedCallback.State = state;
			QueueWorkItem(queuedCallback);
		}

		public bool WaitAll()
		{
			return WaitAll(-1, exitContext: false);
		}

		public bool WaitAll(TimeSpan timeout, bool exitContext)
		{
			return WaitAll((int)timeout.TotalMilliseconds, exitContext);
		}

		public bool WaitAll(int millisecondsTimeout, bool exitContext)
		{
			ThrowIfDisposed();
			DoneWorkItem();
			bool flag = _done.WaitOne(millisecondsTimeout, exitContext);
			lock (_done)
			{
				if (flag)
				{
					_remainingWorkItems = 1;
					_done.Reset();
				}
				else
				{
					_remainingWorkItems++;
				}
			}
			return flag;
		}

		private void HandleWorkItem(object state)
		{
			QueuedCallback queuedCallback = (QueuedCallback)state;
			try
			{
				queuedCallback.Callback(queuedCallback.State);
			}
			finally
			{
				DoneWorkItem();
			}
		}

		private void DoneWorkItem()
		{
			lock (_done)
			{
				_remainingWorkItems--;
				if (_remainingWorkItems == 0)
				{
					_done.Set();
				}
			}
		}

		private void ThrowIfDisposed()
		{
			if (_done == null)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
		}

		public void Dispose()
		{
			if (_done != null)
			{
				((IDisposable)_done).Dispose();
				_done = null;
			}
		}
	}
}
