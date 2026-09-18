using System.Collections.Generic;
using ScheduleOne.DevUtilities;
using ScheduleOne.Employees;
using ScheduleOne.Management;

namespace ScheduleOne.Quests
{
	public class Quest_Chemists : Quest_Employees
	{
		public QuestEntry AssignWorkEntry;

		protected override void MinPass()
		{
			base.MinPass();
			if (AssignWorkEntry.State != EQuestState.Active)
			{
				return;
			}
			foreach (Employee employee in GetEmployees())
			{
				if (((employee as Chemist).Configuration as ChemistConfiguration).TotalStations > 0)
				{
					AssignWorkEntry.Complete();
					break;
				}
			}
		}

		public override List<Employee> GetEmployees()
		{
			return NetworkSingleton<EmployeeManager>.Instance.GetEmployeesByType(EEmployeeType.Chemist);
		}
	}
}
