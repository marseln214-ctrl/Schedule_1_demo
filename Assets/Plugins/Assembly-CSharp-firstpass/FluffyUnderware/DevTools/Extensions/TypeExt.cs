using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace FluffyUnderware.DevTools.Extensions
{
	public static class TypeExt
	{
		public static Type[] GetLoadedTypes()
		{
			IEnumerable<Assembly> loadedAssemblies = GetLoadedAssemblies();
			List<Type> list = new List<Type>(loadedAssemblies.Count() * 100);
			foreach (Assembly item in loadedAssemblies)
			{
				try
				{
					list.AddRange(item.GetTypes());
				}
				catch (ReflectionTypeLoadException ex)
				{
					for (int i = 0; i < ex.Types.Length; i++)
					{
						Type type = ex.Types[i];
						if (type != null)
						{
							list.Add(type);
						}
					}
				}
			}
			return list.ToArray();
		}

		public static IEnumerable<Assembly> GetLoadedAssemblies()
		{
			return AppDomain.CurrentDomain.GetAssemblies();
		}

		public static Dictionary<U, Type> GetAllTypesWithAttribute<U>(this Type type)
		{
			Dictionary<U, Type> dictionary = new Dictionary<U, Type>();
			foreach (Type item in (IEnumerable<Type>)GetLoadedTypes())
			{
				if (item.IsSubclassOf(type))
				{
					object[] customAttributes = item.GetCustomAttributes(typeof(U), inherit: false);
					if (customAttributes.Length != 0)
					{
						dictionary.Add((U)customAttributes[0], item);
					}
				}
			}
			return dictionary;
		}

		public static List<FieldInfo> GetFieldsWithAttribute<T>(this Type type, bool includeInherited = false, bool includePrivate = false) where T : Attribute
		{
			FieldInfo[] allFields = type.GetAllFields(includeInherited, includePrivate);
			List<FieldInfo> list = new List<FieldInfo>();
			FieldInfo[] array = allFields;
			foreach (FieldInfo fieldInfo in array)
			{
				if (fieldInfo.GetCustomAttribute<T>() != null)
				{
					list.Add(fieldInfo);
				}
			}
			return list;
		}

		public static T GetCustomAttribute<T>(this Type type) where T : Attribute
		{
			object[] customAttributes = type.GetCustomAttributes(typeof(T), inherit: true);
			if (customAttributes.Length == 0)
			{
				return null;
			}
			return (T)customAttributes[0];
		}

		public static MethodInfo MethodByName(this Type type, string name, bool includeInherited = false, bool includePrivate = false)
		{
			BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public;
			if (includePrivate)
			{
				bindingFlags |= BindingFlags.NonPublic;
			}
			if (includeInherited)
			{
				return type.GetMethodIncludingBaseClasses(name, bindingFlags);
			}
			return type.GetMethod(name, bindingFlags);
		}

		public static FieldInfo FieldByName(this Type type, string name, bool includeInherited = false, bool includePrivate = false)
		{
			BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public;
			if (includePrivate)
			{
				bindingFlags |= BindingFlags.NonPublic;
			}
			if (includeInherited)
			{
				return type.GetFieldIncludingBaseClasses(name, bindingFlags);
			}
			return type.GetField(name, bindingFlags);
		}

		public static PropertyInfo PropertyByName(this Type type, string name, bool includeInherited = false, bool includePrivate = false)
		{
			BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public;
			if (includePrivate)
			{
				bindingFlags |= BindingFlags.NonPublic;
			}
			if (includeInherited)
			{
				return type.GetPropertyIncludingBaseClasses(name, bindingFlags);
			}
			return type.GetProperty(name, bindingFlags);
		}

		public static FieldInfo[] GetAllFields(this Type type, bool includeInherited = false, bool includePrivate = false)
		{
			BindingFlags bindingFlags = BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public;
			if (includePrivate)
			{
				bindingFlags |= BindingFlags.NonPublic;
			}
			if (includeInherited)
			{
				Type type2 = type;
				List<FieldInfo> list = new List<FieldInfo>();
				while (type2 != typeof(object))
				{
					list.AddRange(type2.GetFields(bindingFlags));
					type2 = type2.BaseType;
				}
				return list.ToArray();
			}
			return type.GetFields(bindingFlags);
		}

		public static PropertyInfo[] GetAllProperties(this Type type, bool includeInherited = false, bool includePrivate = false)
		{
			BindingFlags bindingFlags = BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public;
			if (includePrivate)
			{
				bindingFlags |= BindingFlags.NonPublic;
			}
			if (includeInherited)
			{
				Type type2 = type;
				List<PropertyInfo> list = new List<PropertyInfo>();
				while (type2 != typeof(object))
				{
					list.AddRange(type2.GetProperties(bindingFlags));
					type2 = type2.BaseType;
				}
				return list.ToArray();
			}
			return type.GetProperties(bindingFlags);
		}

		public static bool IsFrameworkType(this Type type)
		{
			if (!type.IsPrimitive && !type.Equals(typeof(string)))
			{
				return type.Equals(typeof(DateTime));
			}
			return true;
		}

		public static bool IsArrayOrList(this Type type)
		{
			if (!type.IsArray)
			{
				if (type.IsGenericType)
				{
					return type.GetGenericTypeDefinition() == typeof(List<>);
				}
				return false;
			}
			return true;
		}

		public static Type GetEnumerableType(this Type t)
		{
			Type type = FindIEnumerable(t);
			if (type == null)
			{
				return t;
			}
			return type.GetGenericArguments()[0];
		}

		private static Type FindIEnumerable(Type seqType)
		{
			if (seqType == null || seqType == typeof(string))
			{
				return null;
			}
			if (seqType.IsArray)
			{
				return typeof(IEnumerable<>).MakeGenericType(seqType.GetElementType());
			}
			if (seqType.IsGenericType)
			{
				Type[] genericArguments = seqType.GetGenericArguments();
				foreach (Type type in genericArguments)
				{
					Type type2 = typeof(IEnumerable<>).MakeGenericType(type);
					if (type2.IsAssignableFrom(seqType))
					{
						return type2;
					}
				}
			}
			Type[] interfaces = seqType.GetInterfaces();
			if (interfaces != null && interfaces.Length != 0)
			{
				Type[] genericArguments = interfaces;
				for (int i = 0; i < genericArguments.Length; i++)
				{
					Type type3 = FindIEnumerable(genericArguments[i]);
					if (type3 != null)
					{
						return type3;
					}
				}
			}
			if (seqType.BaseType != null && seqType.BaseType != typeof(object))
			{
				return FindIEnumerable(seqType.BaseType);
			}
			return null;
		}

		private static MethodInfo GetMethodIncludingBaseClasses(this Type type, string name, BindingFlags bindingFlags)
		{
			MethodInfo method = type.GetMethod(name, bindingFlags);
			if (type.BaseType == typeof(object))
			{
				return method;
			}
			Type type2 = type;
			while (type2 != typeof(object))
			{
				method = type2.GetMethod(name, bindingFlags);
				if (method != null)
				{
					return method;
				}
				type2 = type2.BaseType;
			}
			return null;
		}

		private static FieldInfo GetFieldIncludingBaseClasses(this Type type, string name, BindingFlags bindingFlags)
		{
			FieldInfo field = type.GetField(name, bindingFlags);
			if (type.BaseType == typeof(object))
			{
				return field;
			}
			Type type2 = type;
			while (type2 != typeof(object))
			{
				field = type2.GetField(name, bindingFlags);
				if (field != null)
				{
					return field;
				}
				type2 = type2.BaseType;
			}
			return null;
		}

		private static PropertyInfo GetPropertyIncludingBaseClasses(this Type type, string name, BindingFlags bindingFlags)
		{
			PropertyInfo property = type.GetProperty(name, bindingFlags);
			if (type.BaseType == typeof(object))
			{
				return property;
			}
			Type type2 = type;
			while (type2 != typeof(object))
			{
				property = type2.GetProperty(name, bindingFlags);
				if (property != null)
				{
					return property;
				}
				type2 = type2.BaseType;
			}
			return null;
		}

		public static bool Matches(this Type type, params Type[] types)
		{
			foreach (Type type2 in types)
			{
				if (type == type2 || type.IsAssignableFrom(type2))
				{
					return true;
				}
			}
			return false;
		}
	}
}
