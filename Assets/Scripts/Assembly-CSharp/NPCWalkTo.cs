using System.Collections;
using EasyButtons;
using ScheduleOne.NPCs;
using UnityEngine;

public class NPCWalkTo : MonoBehaviour
{
	public Transform StartPoint;

	public Transform End;

	public NPC NPC;

	private void Start()
	{
		NPC = GetComponent<NPC>();
	}

	public void Update()
	{
		if (Input.GetKeyDown(KeyCode.B))
		{
			Walk();
		}
	}

	[Button]
	public void Walk()
	{
		NPC.Movement.Warp(StartPoint.position);
		NPC.Movement.SetDestination(End.position);
		StartCoroutine(WalkRoutine());
		IEnumerator WalkRoutine()
		{
			yield return new WaitUntil(() => !NPC.Movement.IsMoving);
			NPC.Movement.FaceDirection(End.forward);
		}
	}
}
