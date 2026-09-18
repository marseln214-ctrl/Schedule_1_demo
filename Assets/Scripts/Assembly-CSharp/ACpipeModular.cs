using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[ExecuteInEditMode]
public class ACpipeModular : MonoBehaviour
{
	[HideInInspector]
	public List<GameObject> itemsList = new List<GameObject>();

	[HideInInspector]
	public GameObject largeACPipe;

	[HideInInspector]
	public GameObject smallACpipe;

	[HideInInspector]
	public GameObject innerCorner;

	[HideInInspector]
	public GameObject outerCorner;

	private void Start()
	{
		largeACPipe = Resources.Load("Models/AC_Pipe_Long") as GameObject;
		smallACpipe = Resources.Load("Models/AC_Pipe_Medium") as GameObject;
		innerCorner = Resources.Load("Models/AC_Pipe_Side_left") as GameObject;
		outerCorner = Resources.Load("Models/AC_Pipe_Side_Right") as GameObject;
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
			MonoBehaviour.print(child.gameObject.transform.parent.gameObject.name);
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
