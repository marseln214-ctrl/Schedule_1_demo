using System.Collections.Generic;
using ScheduleOne.DevUtilities;
using ScheduleOne.Employees;
using ScheduleOne.Management;
using ScheduleOne.ObjectScripts;

namespace ScheduleOne.Quests
{
	public class Quest_Botanists : Quest_Employees
	{
		public QuestEntry AssignSuppliesEntry;

		public QuestEntry AssignWorkEntry;

		public QuestEntry AssignDestinationEntry;

		protected override void MinPass()
		{
			base.MinPass();
			if (AssignSuppliesEntry.State == EQuestState.Active)
			{
				foreach (Employee employee in GetEmployees())
				{
					if (((employee as Botanist).Configuration as BotanistConfiguration).Supplies.SelectedObject != null)
					{
						AssignSuppliesEntry.Complete();
						break;
					}
				}
			}
			if (AssignWorkEntry.State == EQuestState.Active)
			{
				foreach (Employee employee2 in GetEmployees())
				{
					if (((employee2 as Botanist).Configuration as BotanistConfiguration).AssignedPots.Count > 0)
					{
						AssignWorkEntry.Complete();
						break;
					}
				}
			}
			if (AssignDestinationEntry.State != EQuestState.Active)
			{
				return;
			}
			foreach (Employee employee3 in GetEmployees())
			{
				foreach (Pot assignedPot in ((employee3 as Botanist).Configuration as BotanistConfiguration).AssignedPots)
				{
					if ((assignedPot.Configuration as PotConfiguration).Destination.SelectedObject != null)
					{
						AssignDestinationEntry.Complete();
						break;
					}
				}
			}
		}

		public override List<Employee> GetEmployees()
		{
			return NetworkSingleton<EmployeeManager>.Instance.GetEmployeesByType(EEmployeeType.Botanist);
		}
	}
}
