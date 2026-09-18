using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[ExecuteInEditMode]
public class WarehouseModular : MonoBehaviour
{
	[HideInInspector]
	public List<GameObject> itemsList = new List<GameObject>();

	[HideInInspector]
	public GameObject largeWall;

	[HideInInspector]
	public GameObject mediumWall;

	[HideInInspector]
	public GameObject smallWall;

	[HideInInspector]
	public GameObject miniWall;

	[HideInInspector]
	public GameObject tinyWall;

	[HideInInspector]
	public GameObject windowWall;

	[HideInInspector]
	public GameObject smallWindowWall;

	[HideInInspector]
	public GameObject innerCorner;

	[HideInInspector]
	public GameObject outerCorner;

	[HideInInspector]
	public GameObject garageFrame;

	[HideInInspector]
	public GameObject doorFrame;

	[HideInInspector]
	public GameObject doubleDoorFrame;

	private MeshFilter myMeshFilter;

	private void Start()
	{
		myMeshFilter = GetComponent<MeshFilter>();
		largeWall = Resources.Load("Models/LargeWall") as GameObject;
		mediumWall = Resources.Load("Models/MediumWall") as GameObject;
		smallWall = Resources.Load("Models/SmallWall") as GameObject;
		miniWall = Resources.Load("Models/Extra_SmallWall") as GameObject;
		tinyWall = Resources.Load("Models/Extra_SmallWall1") as GameObject;
		windowWall = Resources.Load("Models/WindowWall") as GameObject;
		smallWindowWall = Resources.Load("Models/SmallWindowWall") as GameObject;
		innerCorner = Resources.Load("Models/LeftCorner") as GameObject;
		outerCorner = Resources.Load("Models/RightCorner") as GameObject;
		garageFrame = Resources.Load("Models/GarageDoorFrame") as GameObject;
		doorFrame = Resources.Load("Models/DoorWall") as GameObject;
		doubleDoorFrame = Resources.Load("Models/DoubleDoorWall") as GameObject;
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
