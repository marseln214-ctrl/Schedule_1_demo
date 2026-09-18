using System.Collections;
using System.Collections.Generic;
using ScheduleOne.Audio;
using UnityEngine;

namespace ScheduleOne.ObjectScripts
{
	public class CashCounter : MonoBehaviour
	{
		public const float NoteLerpTime = 0.18f;

		public bool IsOn;

		[Header("References")]
		public GameObject UpperNotes;

		public GameObject LowerNotes;

		public Transform NoteStartPoint;

		public Transform NoteEndPoint;

		public List<Transform> MovingNotes = new List<Transform>();

		public AudioSourceController Audio;

		private bool lerping;

		public virtual void LateUpdate()
		{
			UpperNotes.gameObject.SetActive(IsOn);
			LowerNotes.gameObject.SetActive(IsOn);
			if (IsOn)
			{
				if (!lerping)
				{
					lerping = true;
					for (int i = 0; i < MovingNotes.Count; i++)
					{
						StartCoroutine(LerpNote(MovingNotes[i]));
					}
				}
				if (!Audio.AudioSource.isPlaying)
				{
					Audio.Play();
				}
			}
			else
			{
				lerping = false;
				if (Audio.AudioSource.isPlaying)
				{
					Audio.Stop();
				}
			}
		}

		private IEnumerator LerpNote(Transform note)
		{
			yield return new WaitForSeconds((float)MovingNotes.IndexOf(note) / (float)(MovingNotes.Count + 1) * 0.18f);
			note.gameObject.SetActive(value: true);
			while (IsOn)
			{
				note.position = NoteStartPoint.position;
				note.rotation = NoteStartPoint.rotation;
				for (float i = 0f; i < 0.18f; i += Time.deltaTime)
				{
					note.position = Vector3.Lerp(NoteStartPoint.position, NoteEndPoint.position, i / 0.18f);
					note.rotation = Quaternion.Lerp(NoteStartPoint.rotation, NoteEndPoint.rotation, i / 0.18f);
					yield return new WaitForEndOfFrame();
				}
			}
			note.gameObject.SetActive(value: false);
		}
	}
}
