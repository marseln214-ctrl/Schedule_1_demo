using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

namespace FluffyUnderware.DevTools
{
	[RequireComponent(typeof(PoolManager))]
	[HelpURL("https://curvyeditor.com/doclink/dtprefabpool")]
	public class PrefabPool : UnityObjectPool<GameObject>
	{
		[FieldCondition("m_Identifier", "", false, ActionAttribute.ActionEnum.ShowWarning, "Please enter an identifier! (Select a prefab to set automatically)", ActionAttribute.ActionPositionEnum.Below)]
		[SerializeField]
		private string m_Identifier = string.Empty;

		[SerializeField]
		private List<GameObject> m_Prefabs = new List<GameObject>();

		public override string Identifier
		{
			get
			{
				return m_Identifier;
			}
			set
			{
				m_Identifier = value;
			}
		}

		public List<GameObject> Prefabs
		{
			get
			{
				return m_Prefabs;
			}
			set
			{
				m_Prefabs = value;
			}
		}

		protected override void OnValidate()
		{
			base.OnValidate();
			if (m_Identifier == string.Empty && Prefabs.Any((GameObject p) => p != null))
			{
				Identifier = Prefabs.First().name;
			}
		}

		public void Initialize([NotNull] string identifier, PoolSettings settings, params GameObject[] prefabs)
		{
			Identifier = identifier;
			Prefabs = new List<GameObject>(prefabs);
			Initialize(settings);
		}

		protected override GameObject CreateObject()
		{
			if (Prefabs.Count == 0)
			{
				throw new InvalidOperationException("[Curvy] The Prefab Pool '" + Identifier + "' in game object '" + base.gameObject.name + "' could not create a pool element because its Prefabs list is empty");
			}
			GameObject gameObject = Prefabs[UnityEngine.Random.Range(0, Prefabs.Count)];
			if (gameObject == null)
			{
				throw new InvalidOperationException("[Curvy] The Prefab Pool '" + Identifier + "' in game object '" + base.gameObject.name + "' could not create a pool element because its Prefabs list contains a null or destroyed object");
			}
			GameObject gameObject2 = UnityEngine.Object.Instantiate(gameObject);
			if (gameObject2 == null)
			{
				throw new InvalidOperationException("[Curvy] The Prefab Pool '" + Identifier + "' in game object '" + base.gameObject.name + "' could not instantiate prefab " + gameObject.name);
			}
			ConfigureCreatedGameObject(gameObject2, gameObject.name);
			return gameObject2;
		}

		protected override GameObject GetItemGameObject(GameObject item)
		{
			return item;
		}
	}
}
