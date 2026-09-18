using System.Collections.Generic;
using UnityEngine;

namespace ScheduleOne.Storage
{
	public class PalletZone : MonoBehaviour
	{
		private List<Pallet> pallets = new List<Pallet>();

		[Header("Prefabs")]
		[SerializeField]
		protected GameObject palletPrefab;

		private bool orderReceivedThisFrame;

		public bool isClear
		{
			get
			{
				if (pallets.Count == 0 || AreAllPalletsClear())
				{
					return !orderReceivedThisFrame;
				}
				return false;
			}
		}

		protected void OnTriggerStay(Collider other)
		{
			Pallet componentInParent = other.GetComponentInParent<Pallet>();
			if (componentInParent != null && !pallets.Contains(componentInParent))
			{
				pallets.Add(componentInParent);
			}
		}

		protected void FixedUpdate()
		{
			pallets.Clear();
		}

		protected void LateUpdate()
		{
			orderReceivedThisFrame = false;
		}

		public Pallet GeneratePallet()
		{
			Pallet component = Object.Instantiate(palletPrefab).GetComponent<Pallet>();
			component.transform.position = base.transform.position;
			component.transform.rotation = base.transform.rotation;
			return component;
		}

		private bool AreAllPalletsClear()
		{
			for (int i = 0; i < pallets.Count; i++)
			{
				if (!pallets[i].isEmpty)
				{
					return false;
				}
			}
			return true;
		}
	}
}
