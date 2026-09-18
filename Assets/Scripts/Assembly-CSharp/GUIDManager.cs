using System;
using System.Collections.Generic;
using ScheduleOne;

public static class GUIDManager
{
	private static List<Guid> registeredGUIDs = new List<Guid>();

	private static Dictionary<Guid, object> guidToObject = new Dictionary<Guid, object>();

	public static void RegisterObject(IGUIDRegisterable obj)
	{
		if (registeredGUIDs.Contains(obj.GUID))
		{
			ScheduleOne.Console.LogWarning("RegisterObject called and passed obj whose GUID is already registered. Replacing old entries with new");
			registeredGUIDs.Remove(obj.GUID);
			guidToObject.Remove(obj.GUID);
		}
		registeredGUIDs.Add(obj.GUID);
		guidToObject.Add(obj.GUID, obj);
	}

	public static void DeregisterObject(IGUIDRegisterable obj)
	{
		registeredGUIDs.Remove(obj.GUID);
		guidToObject.Remove(obj.GUID);
	}

	public static T GetObject<T>(Guid guid)
	{
		if (!registeredGUIDs.Contains(guid))
		{
			return default(T);
		}
		object obj = guidToObject[guid];
		if (!(obj is T))
		{
			ScheduleOne.Console.LogWarning("Object is not of requested type. Returning default(T)");
			return default(T);
		}
		return (T)obj;
	}

	public static Type GetObjectType(Guid guid)
	{
		return GetObject<IGUIDRegisterable>(guid)?.GetType();
	}

	public static Guid GenerateUniqueGUID()
	{
		Guid guid = default(Guid);
		bool flag = false;
		while (!flag)
		{
			guid = Guid.NewGuid();
			if (!registeredGUIDs.Contains(guid))
			{
				flag = true;
			}
		}
		return guid;
	}

	public static bool IsGUIDAlreadyRegistered(Guid guid)
	{
		if (registeredGUIDs.Contains(guid))
		{
			return true;
		}
		return false;
	}

	public static bool IsGUIDValid(string guid)
	{
		if (guid == null)
		{
			return false;
		}
		if (guid == string.Empty)
		{
			return false;
		}
		if (new Guid(guid) == Guid.Empty)
		{
			return false;
		}
		return true;
	}

	public static void Clear()
	{
		ScheduleOne.Console.Log("GUIDManager cleared!");
		registeredGUIDs.Clear();
		guidToObject.Clear();
	}
}
