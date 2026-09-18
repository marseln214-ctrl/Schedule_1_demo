using System;
using UnityEngine;

namespace FluffyUnderware.Curvy
{
	public class CurvyEventArgs : EventArgs
	{
		public readonly MonoBehaviour Sender;

		public readonly object Data;

		public CurvyEventArgs(MonoBehaviour sender, object data)
		{
			Sender = sender;
			Data = data;
		}
	}
}
