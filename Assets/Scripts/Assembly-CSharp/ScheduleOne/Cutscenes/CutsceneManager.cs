using System.Collections.Generic;
using EasyButtons;
using ScheduleOne.DevUtilities;
using UnityEngine;

namespace ScheduleOne.Cutscenes
{
	public class CutsceneManager : Singleton<CutsceneManager>
	{
		public List<Cutscene> Cutscenes;

		[Header("Run cutscene by name")]
		[SerializeField]
		private string cutsceneName = "Wake up morning";

		private Cutscene playingCutscene;

		[Button]
		private void RunCutscene()
		{
			Play(cutsceneName);
		}

		public void Play(string name)
		{
			Cutscene cutscene = Cutscenes.Find((Cutscene c) => c.Name == name);
			if (cutscene != null)
			{
				cutscene.Play();
				playingCutscene = cutscene;
				playingCutscene.onEnd.AddListener(Ended);
			}
		}

		private void Ended()
		{
			playingCutscene.onEnd.RemoveListener(Ended);
			playingCutscene = null;
		}
	}
}
