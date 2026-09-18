using System.Collections;
using ScheduleOne.PlayerScripts;
using UnityEngine;

namespace ScheduleOne.Packaging
{
	public class FunctionalBaggie : FunctionalPackaging
	{
		public SkinnedMeshRenderer[] BagMeshes;

		public GameObject FunnelCollidersContainer;

		public GameObject FullyPackedBlocker;

		public Collider DynamicCollider;

		private float ClosedDelta;

		public override CursorManager.ECursorType HoveredCursor { get; protected set; } = CursorManager.ECursorType.Finger;


		public void SetClosed(float closedDelta)
		{
			ClosedDelta = closedDelta;
			SkinnedMeshRenderer[] bagMeshes = BagMeshes;
			for (int i = 0; i < bagMeshes.Length; i++)
			{
				bagMeshes[i].SetBlendShapeWeight(0, closedDelta * 100f);
			}
		}

		public override void StartClick(RaycastHit hit)
		{
			if (base.IsFull && ClosedDelta == 0f)
			{
				ClickableEnabled = false;
				if (!base.IsSealed)
				{
					Seal();
				}
			}
			base.StartClick(hit);
		}

		public override void Seal()
		{
			base.Seal();
			FunnelCollidersContainer.gameObject.SetActive(value: false);
			DynamicCollider.enabled = true;
			StartCoroutine(Routine());
			IEnumerator Routine()
			{
				float lerpTime = 0.25f;
				for (float i = 0f; i < lerpTime; i += Time.deltaTime)
				{
					SetClosed(i / lerpTime);
					yield return new WaitForEndOfFrame();
				}
				SetClosed(1f);
			}
		}

		protected override void FullyPacked()
		{
			base.FullyPacked();
			FullyPackedBlocker.SetActive(value: true);
		}
	}
}
