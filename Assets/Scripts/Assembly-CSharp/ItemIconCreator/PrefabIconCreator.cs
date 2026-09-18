using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ItemIconCreator
{
	[ExecuteInEditMode]
	public class PrefabIconCreator : IconCreator
	{
		[Header("Items")]
		public GameObject[] itemsToShot;

		public Transform itemPosition;

		private GameObject instantiatedItem;

		public override void BuildIcons()
		{
			StartCoroutine(BuildAllIcons());
		}

		public override bool CheckConditions()
		{
			if (!base.CheckConditions())
			{
				return false;
			}
			if (itemsToShot.Length == 0)
			{
				Debug.LogError("There's no prefab to shoot");
				return false;
			}
			if (itemPosition == null)
			{
				Debug.LogError("Item position is null");
				return false;
			}
			return true;
		}

		protected override void Update()
		{
			if (preview && !isCreatingIcons)
			{
				if (instantiatedItem != null)
				{
					if (dynamicFov)
					{
						UpdateFOV(instantiatedItem);
					}
					if (lookAtObjectCenter)
					{
						LookAtTargetCenter(instantiatedItem);
					}
					instantiatedItem.transform.position = itemPosition.transform.position;
					instantiatedItem.transform.rotation = itemPosition.transform.rotation;
				}
				else if (instantiatedItem == null && itemsToShot.Length != 0)
				{
					ClearShit();
					if (itemPosition.childCount > 0 && itemPosition.GetChild(0).GetComponent<MeshRenderer>() != null)
					{
						instantiatedItem = itemPosition.GetChild(0).gameObject;
					}
					else
					{
						instantiatedItem = Object.Instantiate(itemsToShot[0], itemPosition.transform.position, itemPosition.transform.rotation, itemPosition);
					}
				}
			}
			base.Update();
		}

		private void ClearShit()
		{
			List<Transform> list = new List<Transform>();
			for (int i = 0; i < itemPosition.childCount; i++)
			{
				list.Add(itemPosition.GetChild(i));
			}
			for (int j = 0; j < list.Count; j++)
			{
				Object.DestroyImmediate(list[j].gameObject);
			}
		}

		public IEnumerator BuildAllIcons()
		{
			Initialize();
			for (int i = 0; i < itemsToShot.Length; i++)
			{
				finalPath = "C:/Users/Tyler/Desktop/";
				if (instantiatedItem != null)
				{
					Object.DestroyImmediate(instantiatedItem);
				}
				if (whiteCam != null)
				{
					whiteCam.enabled = false;
				}
				if (blackCam != null)
				{
					blackCam.enabled = false;
				}
				ClearShit();
				instantiatedItem = Object.Instantiate(itemsToShot[i], itemPosition.transform.position, itemPosition.transform.rotation);
				if (IconCreatorCanvas.instance != null)
				{
					IconCreatorCanvas.instance.SetInfo(itemsToShot.Length, i, itemsToShot[i].name, isRecording: true, nextIconKey);
				}
				currentObject = instantiatedItem.transform;
				if (dynamicFov)
				{
					UpdateFOV(instantiatedItem);
				}
				if (lookAtObjectCenter)
				{
					LookAtTargetCenter(instantiatedItem);
				}
				if (mode == Mode.Manual)
				{
					CanMove = true;
					yield return new WaitUntil(() => Input.GetKeyDown(nextIconKey));
					CanMove = false;
				}
				if (IconCreatorCanvas.instance != null)
				{
					IconCreatorCanvas.instance.SetTakingPicture();
					yield return null;
					yield return null;
				}
				yield return CaptureFrame(itemsToShot[i].name, i);
			}
			if (IconCreatorCanvas.instance != null)
			{
				IconCreatorCanvas.instance.SetInfo(0, 0, "", isRecording: false, nextIconKey);
			}
			DeleteCameras();
		}
	}
}
