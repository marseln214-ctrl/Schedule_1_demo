using System;
using System.Collections.Generic;
using FluffyUnderware.DevTools;
using JetBrains.Annotations;
using UnityEngine;

namespace FluffyUnderware.Curvy.Generator
{
	public static class CGResourceHandler
	{
		private static readonly Dictionary<string, ICGResourceLoader> resourceLoadersCache = new Dictionary<string, ICGResourceLoader>();

		public static void RegisterResourceLoader(string resourceName, ICGResourceLoader loader)
		{
			if (resourceLoadersCache.ContainsKey(resourceName))
			{
				DTLog.LogError("[Curvy] Trying to register a loader for resource '" + resourceName + "' multiple times. Attempt is ignored.");
			}
			else
			{
				resourceLoadersCache[resourceName] = loader;
			}
		}

		[NotNull]
		public static Component CreateResource(CGModule module, [NotNull] string resName, [NotNull] string context)
		{
			if (resourceLoadersCache.ContainsKey(resName))
			{
				return resourceLoadersCache[resName].Create(module, context);
			}
			throw new InvalidOperationException("[Curvy] CGResourceHandler: Missing loader for resource '" + resName + "'. Make sure the loader registers itself using CGResourceHandler.RegisterResourceLoader");
		}

		public static void DestroyResource(CGModule module, [NotNull] string resName, Component obj, [NotNull] string context, bool kill)
		{
			if (resourceLoadersCache.ContainsKey(resName))
			{
				resourceLoadersCache[resName].Destroy(module, obj, context, kill);
			}
			else
			{
				DTLog.LogError("[Curvy] CGResourceHandler: Missing loader for resource '" + resName + "'. Make sure the loader registers itself using CGResourceHandler.RegisterResourceLoader", module);
			}
		}
	}
}
