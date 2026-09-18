using System;
using System.Collections.Generic;
using System.Linq;
using FluffyUnderware.DevTools;
using JetBrains.Annotations;
using UnityEngine;

namespace FluffyUnderware.Curvy.Generator.Modules
{
	[ModuleInfo("Input/Transform Spots", ModuleName = "Input Transform Spots", Description = "Defines an array of placement spots taken from existing Transforms")]
	[HelpURL("https://curvyeditor.com/doclink/cginputtransformspots")]
	public class InputTransformSpots : CGModule
	{
		[Serializable]
		public struct TransformSpot : IEquatable<TransformSpot>
		{
			[SerializeField]
			private int index;

			[SerializeField]
			private Transform transform;

			public int Index => index;

			public Transform Transform => transform;

			public bool Equals(TransformSpot other)
			{
				if (index == other.index)
				{
					return object.Equals(transform, other.transform);
				}
				return false;
			}

			public override bool Equals(object obj)
			{
				if (obj is TransformSpot other)
				{
					return Equals(other);
				}
				return false;
			}

			public override int GetHashCode()
			{
				return (index * 397) ^ ((transform != null) ? transform.GetHashCode() : 0);
			}

			public static bool operator ==(TransformSpot left, TransformSpot right)
			{
				return left.Equals(right);
			}

			public static bool operator !=(TransformSpot left, TransformSpot right)
			{
				return !left.Equals(right);
			}
		}

		[HideInInspector]
		[OutputSlotInfo(typeof(CGSpots))]
		public CGModuleOutputSlot OutSpots = new CGModuleOutputSlot();

		[ArrayEx]
		[SerializeField]
		private List<TransformSpot> transformSpots = new List<TransformSpot>();

		private readonly Dictionary<CGSpot, TransformSpot> outputToInputDictionary = new Dictionary<CGSpot, TransformSpot>();

		public List<TransformSpot> TransformSpots
		{
			get
			{
				return transformSpots;
			}
			set
			{
				if (transformSpots != value)
				{
					transformSpots = value;
					base.Dirty = true;
				}
			}
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			Properties.MinWidth = 250f;
		}

		protected override void OnDisable()
		{
			base.OnDisable();
		}

		public override void Reset()
		{
			base.Reset();
			TransformSpots.Clear();
		}

		[UsedImplicitly]
		private void Update()
		{
			if (base.Dirty || OutSpots.Data.Length == 0)
			{
				return;
			}
			foreach (KeyValuePair<CGSpot, TransformSpot> item in outputToInputDictionary)
			{
				CGSpot key = item.Key;
				TransformSpot value = item.Value;
				if (key.Position != value.Transform.position)
				{
					base.Dirty = true;
					break;
				}
			}
		}

		public override void Refresh()
		{
			base.Refresh();
			if (OutSpots.IsLinked)
			{
				outputToInputDictionary.Clear();
				List<CGSpot> spots = TransformSpots.Where((TransformSpot s) => s.Transform != null).Select(delegate(TransformSpot s)
				{
					CGSpot cGSpot = new CGSpot(s.Index, s.Transform.position, s.Transform.rotation, s.Transform.lossyScale);
					outputToInputDictionary[cGSpot] = s;
					return cGSpot;
				}).ToList();
				OutSpots.SetDataToElement(new CGSpots(spots));
			}
		}

		protected override void ResetOnEnable()
		{
			base.ResetOnEnable();
			outputToInputDictionary.Clear();
		}
	}
}
