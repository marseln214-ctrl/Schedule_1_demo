using System;
using ScheduleOne.DevUtilities;
using ScheduleOne.Employees;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.Property;
using UnityEngine;

namespace ScheduleOne.Persistence.Loaders
{
	public class EmployeeLoader : NPCLoader
	{
		public override string NPCType => typeof(EmployeeData).Name;

		public Employee LoadAndCreateEmployee(string mainPath)
		{
			if (TryLoadFile(mainPath, "NPC", out var contents))
			{
				EmployeeData employeeData = null;
				try
				{
					employeeData = JsonUtility.FromJson<EmployeeData>(contents);
				}
				catch (Exception ex)
				{
					Console.LogError(GetType()?.ToString() + " error reading data: " + ex);
					return null;
				}
				if (employeeData == null)
				{
					Console.LogWarning("Failed to load employee data");
					return null;
				}
				ScheduleOne.Property.Property property = Singleton<PropertyManager>.Instance.GetProperty(employeeData.AssignedProperty);
				EEmployeeType type = EEmployeeType.Botanist;
				if (employeeData.DataType == typeof(PackagerData).Name)
				{
					type = EEmployeeType.Packager;
				}
				else if (employeeData.DataType == typeof(BotanistData).Name)
				{
					type = EEmployeeType.Botanist;
				}
				else if (employeeData.DataType == typeof(ChemistData).Name)
				{
					type = EEmployeeType.Chemist;
				}
				else if (employeeData.DataType == typeof(CleanerData).Name)
				{
					type = EEmployeeType.Cleaner;
				}
				else
				{
					Console.LogError("Failed to recognize employee type: " + employeeData.DataType);
				}
				Employee employee = NetworkSingleton<EmployeeManager>.Instance.CreateEmployee_Server(property, type, employeeData.FirstName, employeeData.LastName, employeeData.ID, employeeData.IsMale, employeeData.AppearanceIndex, employeeData.Position, employeeData.Rotation, employeeData.GUID);
				if (employee == null)
				{
					Console.LogWarning("Failed to create employee");
					return null;
				}
				if (employeeData.PaidForToday)
				{
					employee.SetIsPaid();
				}
				TryLoadInventory(mainPath, employee);
				return employee;
			}
			return null;
		}
	}
}
