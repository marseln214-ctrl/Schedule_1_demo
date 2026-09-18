using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ScheduleOne.Audio
{
	[RequireComponent(typeof(Button))]
	[RequireComponent(typeof(EventTrigger))]
	[RequireComponent(typeof(AudioSourceController))]
	public class ButtonSound : MonoBehaviour
	{
		public AudioSourceController AudioSource;

		public EventTrigger EventTrigger;

		[Header("Clips")]
		public AudioClip HoverClip;

		public float HoverSoundVolume = 1f;

		public AudioClip ClickClip;

		public float ClickSoundVolume = 1f;

		private Button Button;

		public void Awake()
		{
			AddEventTrigger(EventTrigger, EventTriggerType.PointerEnter, Hovered);
			AddEventTrigger(EventTrigger, EventTriggerType.PointerClick, Clicked);
			AudioSource.AudioSource.playOnAwake = false;
			Button = GetComponent<Button>();
		}

		private void OnValidate()
		{
			if (AudioSource == null)
			{
				AudioSource = GetComponent<AudioSourceController>();
			}
			if (EventTrigger == null)
			{
				EventTrigger = GetComponent<EventTrigger>();
			}
		}

		public void AddEventTrigger(EventTrigger eventTrigger, EventTriggerType eventTriggerType, Action action)
		{
			EventTrigger.Entry entry = new EventTrigger.Entry();
			entry.eventID = eventTriggerType;
			entry.callback.AddListener(delegate
			{
				action();
			});
			eventTrigger.triggers.Add(entry);
		}

		protected virtual void Hovered()
		{
			if (Button.interactable)
			{
				AudioSource.VolumeMultiplier = HoverSoundVolume;
				AudioSource.AudioSource.clip = HoverClip;
				AudioSource.PitchMultiplier = 0.9f;
				AudioSource.Play();
			}
		}

		protected virtual void Clicked()
		{
			if (Button.interactable)
			{
				AudioSource.VolumeMultiplier = ClickSoundVolume;
				AudioSource.AudioSource.clip = ClickClip;
				AudioSource.Play();
			}
		}
	}
}
