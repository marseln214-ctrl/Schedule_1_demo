using System;
using System.Collections;
using ScheduleOne.DevUtilities;
using ScheduleOne.NPCs;
using ScheduleOne.PlayerScripts;
using TMPro;
using UnityEngine;

namespace ScheduleOne.UI.Management
{
	public class WorldspaceUIElement : MonoBehaviour
	{
		public const float TRANSITION_TIME = 0.1f;

		[Header("References")]
		public RectTransform RectTransform;

		public RectTransform Container;

		public TextMeshProUGUI TitleLabel;

		public AssignedWorkerDisplay AssignedWorkerDisplay;

		private Coroutine scaleRoutine;

		public bool IsEnabled { get; protected set; }

		public bool IsVisible => base.gameObject.activeSelf;

		public virtual void Show()
		{
			IsEnabled = true;
			base.gameObject.SetActive(value: true);
			SetScale(1f, null);
		}

		public virtual void Hide(Action callback = null)
		{
			IsEnabled = false;
			SetScale(0f, delegate
			{
				Done();
			});
			void Done()
			{
				base.gameObject.SetActive(value: false);
				if (callback != null)
				{
					callback();
				}
			}
		}

		public virtual void Destroy()
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}

		public void UpdatePosition(Vector3 worldSpacePosition)
		{
			if (PlayerSingleton<PlayerCamera>.Instance.transform.InverseTransformPoint(worldSpacePosition).z > 0f)
			{
				RectTransform.position = PlayerSingleton<PlayerCamera>.Instance.Camera.WorldToScreenPoint(worldSpacePosition);
				Container.gameObject.SetActive(value: true);
			}
			else
			{
				Container.gameObject.SetActive(value: false);
			}
		}

		public virtual void SetInternalScale(float scale)
		{
			Container.localScale = new Vector3(scale, scale, 1f);
		}

		private void SetScale(float scale, Action callback)
		{
			if (scaleRoutine != null)
			{
				Singleton<CoroutineService>.Instance.StopCoroutine(scaleRoutine);
			}
			float startScale;
			float lerpTime;
			if (!base.gameObject.activeInHierarchy)
			{
				RectTransform.localScale = new Vector3(scale, scale, 1f);
				if (callback != null)
				{
					callback();
				}
			}
			else
			{
				startScale = RectTransform.localScale.x;
				lerpTime = 0.1f / Mathf.Abs(startScale - scale);
				scaleRoutine = Singleton<CoroutineService>.Instance.StartCoroutine(Routine());
			}
			IEnumerator Routine()
			{
				for (float i = 0f; i < lerpTime; i += Time.deltaTime)
				{
					float num = Mathf.Lerp(startScale, scale, i / lerpTime);
					RectTransform.localScale = new Vector3(num, num, 1f);
					yield return new WaitForEndOfFrame();
				}
				RectTransform.localScale = new Vector3(scale, scale, 1f);
				if (callback != null)
				{
					callback();
				}
			}
		}

		public virtual void HoverStart()
		{
		}

		public virtual void HoverEnd()
		{
		}

		public void SetAssignedNPC(NPC npc)
		{
			AssignedWorkerDisplay.Set(npc);
		}
	}
}
