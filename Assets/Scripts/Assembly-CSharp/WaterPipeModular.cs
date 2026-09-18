using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[ExecuteInEditMode]
public class WaterPipeModular : MonoBehaviour
{
	[HideInInspector]
	public List<GameObject> itemsList = new List<GameObject>();

	[HideInInspector]
	public GameObject largeWaterPipe;

	[HideInInspector]
	public GameObject mediumWaterPipe;

	[HideInInspector]
	public GameObject smallWaterpipe;

	[HideInInspector]
	public GameObject innerCorner;

	[HideInInspector]
	public GameObject outerCorner;

	private void Start()
	{
		largeWaterPipe = Resources.Load("Models/Water_Pipe_Long") as GameObject;
		mediumWaterPipe = Resources.Load("Models/Water_Pipe_Medium") as GameObject;
		smallWaterpipe = Resources.Load("Models/Water_Pipe_Small") as GameObject;
		innerCorner = Resources.Load("Models/Water_Pipe_left") as GameObject;
		outerCorner = Resources.Load("Models/Water_Pipe_right") as GameObject;
	}

	public void BuildNextItem(GameObject item)
	{
		if (itemsList.Count == 0)
		{
			GameObject gameObject = Object.Instantiate(item, base.transform.position, item.transform.rotation);
			gameObject.transform.SetParent(base.transform);
			itemsList.Add(gameObject);
		}
		else
		{
			Transform child = itemsList.Last().transform.GetChild(0);
			GameObject gameObject2 = Object.Instantiate(item, child.position, child.rotation);
			gameObject2.transform.SetParent(base.transform);
			itemsList.Add(gameObject2);
		}
	}

	public void DeleteLastItem()
	{
		GameObject gameObject = itemsList.Last();
		if (Application.isPlaying)
		{
			Object.Destroy(gameObject);
		}
		if (Application.isEditor)
		{
			Object.DestroyImmediate(gameObject);
		}
		itemsList.Remove(gameObject);
	}
}
