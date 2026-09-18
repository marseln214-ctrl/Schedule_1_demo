using ScheduleOne.NPCs;
using UnityEngine;

public class TrailerSaleAnim : MonoBehaviour
{
	public NPC[] NPCs;

	public void PlayAnim()
	{
		Debug.Log("Playing");
		NPC[] nPCs = NPCs;
		for (int i = 0; i < nPCs.Length; i++)
		{
			nPCs[i].Avatar.Anim.SetTrigger("GrabItem");
		}
	}
}
