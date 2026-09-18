using System;
using JetBrains.Annotations;

namespace FluffyUnderware.DevTools
{
	public interface IPool
	{
		[NotNull]
		string Identifier { get; set; }

		[NotNull]
		PoolSettings Settings { get; }

		int Count { get; }

		[UsedImplicitly]
		[Obsolete]
		void Clear();

		[UsedImplicitly]
		[Obsolete]
		void Reset();

		[UsedImplicitly]
		[Obsolete]
		void Update();
	}
}
