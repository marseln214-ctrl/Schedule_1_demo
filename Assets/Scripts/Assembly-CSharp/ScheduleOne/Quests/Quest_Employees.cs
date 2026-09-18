using System.Collections.Generic;
using FishNet;
using ScheduleOne.Employees;

namespace ScheduleOne.Quests
{
	public abstract class Quest_Employees : Quest
	{
		public EEmployeeType EmployeeType;

		public QuestEntry AssignBedEntry;

		public QuestEntry PayEntry;

		public abstract List<Employee> GetEmployees();

		protected override void MinPass()
		{
			base.MinPass();
			if (InstanceFinder.IsServer)
			{
				if (AssignBedEntry.State == EQuestState.Active && AreAnyEmployeesAssignedBeds())
				{
					AssignBedEntry.Complete();
				}
				if (PayEntry.State == EQuestState.Active && AreAnyEmployeesPaid())
				{
					PayEntry.Complete();
				}
			}
		}

		protected bool AreAnyEmployeesAssignedBeds()
		{
			foreach (Employee employee in GetEmployees())
			{
				if (employee.GetBed() != null)
				{
					return true;
				}
			}
			return false;
		}

		protected bool AreAnyEmployeesPaid()
		{
			foreach (Employee employee in GetEmployees())
			{
				if (employee.PaidForToday)
				{
					return true;
				}
			}
			return false;
		}
	}
}
