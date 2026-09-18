using System.Collections.Generic;

namespace ScheduleOne.Management.Presets.Options
{
	public class ItemList : Option
	{
		public bool All;

		public bool None;

		public List<string> Selection = new List<string>();

		public bool CanBeAll { get; protected set; } = true;


		public bool CanBeNone { get; protected set; } = true;


		public List<string> OptionList { get; protected set; } = new List<string>();


		public ItemList(string name, List<string> optionList, bool canBeAll, bool canBeNone)
			: base(name)
		{
			OptionList.AddRange(optionList);
			CanBeAll = canBeAll;
			CanBeNone = canBeNone;
		}

		public override void CopyTo(Option other)
		{
			base.CopyTo(other);
			ItemList obj = other as ItemList;
			obj.All = All;
			obj.None = None;
			obj.Selection = new List<string>(Selection);
			obj.CanBeAll = CanBeAll;
			obj.CanBeNone = CanBeNone;
			obj.OptionList = new List<string>(OptionList);
		}

		public override string GetDisplayString()
		{
			if (All)
			{
				return "All";
			}
			if (None || Selection.Count == 0)
			{
				return "None";
			}
			List<string> list = new List<string>();
			for (int i = 0; i < Selection.Count; i++)
			{
				list.Add(Registry.GetItem(Selection[i]).Name);
			}
			return string.Join(", ", list);
		}
	}
}
