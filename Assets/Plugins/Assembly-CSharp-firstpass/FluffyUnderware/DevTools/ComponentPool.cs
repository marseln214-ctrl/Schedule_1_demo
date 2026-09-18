using System;
using System.Linq;
using FluffyUnderware.DevTools.Extensions;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FluffyUnderware.DevTools
{
	[HelpURL("https://curvyeditor.com/doclink/dtcomponentpool")]
	public class ComponentPool : UnityObjectPool<Component>, ISerializationCallbackReceiver
	{
		[SerializeField]
		[HideInInspector]
		private string m_Identifier;

		public override string Identifier
		{
			get
			{
				return m_Identifier;
			}
			set
			{
				throw new InvalidOperationException("Component pool's identifier should always indicate the pooled type's assembly qualified name");
			}
		}

		public Type Type
		{
			get
			{
				Type type = Type.GetType(Identifier);
				if (type == null)
				{
					DTLog.LogWarning("[DevTools] ComponentPool's Type is an unknown type " + m_Identifier, this);
				}
				return type;
			}
		}

		public void Initialize(Type type, PoolSettings settings)
		{
			string assemblyQualifiedName = type.AssemblyQualifiedName;
			if (assemblyQualifiedName == null)
			{
				throw new InvalidOperationException();
			}
			m_Identifier = assemblyQualifiedName;
			Initialize(settings);
		}

		protected override Component CreateObject()
		{
			Type type = Type;
			if (type == null)
			{
				throw new InvalidOperationException("[DevTools] ComponentPool " + m_Identifier + " could not create component because the associated type is null");
			}
			GameObject gameObject = new GameObject();
			ConfigureCreatedGameObject(gameObject, Identifier);
			return gameObject.AddComponent(type);
		}

		protected override GameObject GetItemGameObject(Component item)
		{
			return item.gameObject;
		}

		[UsedImplicitly]
		[Obsolete]
		public void OnSceneLoaded(Scene scn, LoadSceneMode mode)
		{
		}

		[UsedImplicitly]
		[Obsolete("Use other Pop method instead")]
		public T Pop<T>(Transform parent) where T : Component
		{
			return Pop(parent) as T;
		}

		public void OnBeforeSerialize()
		{
		}

		public void OnAfterDeserialize()
		{
			if (!(Type.GetType(m_Identifier) == null))
			{
				return;
			}
			string[] array = m_Identifier.Split(',');
			if (array.Length >= 5)
			{
				string typeName = string.Join(",", array.SubArray(0, array.Length - 4));
				Type type = TypeExt.GetLoadedTypes().FirstOrDefault((Type t) => t.FullName == typeName);
				if (type != null)
				{
					m_Identifier = type.AssemblyQualifiedName;
				}
			}
		}
	}
}
