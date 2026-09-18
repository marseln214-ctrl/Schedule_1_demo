using System.Collections.Generic;

namespace ScheduleOne.Police
{
	public class Offense
	{
		public class Charge
		{
			public string chargeName = "<ChargeName>";

			public int crimeIndex = 1;

			public int quantity = 1;

			public Charge(string _chargeName, int _crimeIndex, int _quantity)
			{
				chargeName = _chargeName;
				crimeIndex = _crimeIndex;
				quantity = _quantity;
			}
		}

		public List<Charge> charges = new List<Charge>();

		public List<string> penalties = new List<string>();

		public Offense(List<Charge> _charges)
		{
			charges.AddRange(_charges);
		}
	}
}
