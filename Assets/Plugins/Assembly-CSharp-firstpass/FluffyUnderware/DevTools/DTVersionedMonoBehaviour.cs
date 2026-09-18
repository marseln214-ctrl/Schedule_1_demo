using System;
using FluffyUnderware.DevTools.Extensions;
using JetBrains.Annotations;
using UnityEngine;

namespace FluffyUnderware.DevTools
{
	public abstract class DTVersionedMonoBehaviour : MonoBehaviour
	{
		[SerializeField]
		[HideInInspector]
		private string m_Version;

		public string Version
		{
			get
			{
				return m_Version;
			}
			protected set
			{
				m_Version = value;
			}
		}

		protected bool IsActiveAndEnabled { get; private set; }

		protected virtual void OnEnable()
		{
			IsActiveAndEnabled = true;
			ResetOnEnable();
		}

		protected virtual void ResetOnEnable()
		{
		}

		protected virtual void OnDisable()
		{
			IsActiveAndEnabled = false;
		}

		protected virtual void OnValidate()
		{
		}

		protected virtual void Reset()
		{
			OnValidate();
		}

		[UsedImplicitly]
		[Obsolete("Use ObjectExt.Destroy(...) instead")]
		public void Destroy()
		{
			base.gameObject.Destroy(isUndoable: false, doPrefabCheck: true);
		}
	}
}
