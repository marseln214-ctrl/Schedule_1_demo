using System;
using System.Collections;
using System.Reflection;
using System.Text;
using FluffyUnderware.DevTools.Extensions;

namespace FluffyUnderware.DevTools
{
	public class DTObjectDump
	{
		private const int INDENTSPACES = 5;

		private string mIndent;

		private StringBuilder mSB;

		private object mObject;

		public DTObjectDump(object o, int indent = 0)
		{
			mIndent = new string(' ', indent);
			mSB = new StringBuilder();
			mObject = o;
			Type type = o.GetType();
			FieldInfo[] allFields = type.GetAllFields(includeInherited: false, includePrivate: true);
			if (allFields.Length != 0)
			{
				AppendHeader("Fields");
			}
			FieldInfo[] array = allFields;
			foreach (FieldInfo info in array)
			{
				AppendMember(info);
			}
			PropertyInfo[] allProperties = type.GetAllProperties(includeInherited: false, includePrivate: true);
			if (allProperties.Length != 0)
			{
				AppendHeader("Properties");
			}
			PropertyInfo[] array2 = allProperties;
			foreach (PropertyInfo info2 in array2)
			{
				AppendMember(info2);
			}
		}

		public override string ToString()
		{
			return mSB.ToString();
		}

		private void AppendHeader(string name)
		{
			mSB.AppendLine(mIndent + "<b>---===| " + name + " |===---</b>\n");
		}

		private void AppendMember(MemberInfo info)
		{
			FieldInfo fieldInfo = info as FieldInfo;
			Type type;
			string arg;
			object value;
			if (fieldInfo != null)
			{
				type = fieldInfo.FieldType;
				arg = type.Name;
				value = fieldInfo.GetValue(mObject);
			}
			else
			{
				PropertyInfo obj = info as PropertyInfo;
				type = obj.PropertyType;
				arg = type.Name;
				value = obj.GetValue(mObject, null);
			}
			if (value == null)
			{
				return;
			}
			if (typeof(IEnumerable).IsAssignableFrom(type))
			{
				string text = mIndent;
				int num = 0;
				if (value is IEnumerable enumerable)
				{
					if (type.GetEnumerableType().BaseType == typeof(ValueType))
					{
						foreach (object item in enumerable)
						{
							text += $"<b>{num++.ToString()}</b>: {item.ToString()} ";
						}
					}
					else
					{
						if (typeof(IList).IsAssignableFrom(type))
						{
							arg = "IList<" + type.GetEnumerableType()?.ToString() + ">";
						}
						text += "\n";
						foreach (object item2 in enumerable)
						{
							text += $"<b>{num++.ToString()}</b>: {new DTObjectDump(item2, mIndent.Length + 5).ToString()} ";
						}
					}
				}
				mSB.Append(mIndent);
				mSB.AppendFormat("(<i>{0}</i>) <b>{1}[{2}]</b> = ", arg, info.Name, num);
				mSB.AppendLine(text);
			}
			else
			{
				mSB.Append(mIndent);
				mSB.AppendFormat("(<i>{0}</i>) <b>{1}</b> = ", arg, info.Name);
				mSB.AppendLine(mIndent + value.ToString());
			}
		}
	}
}
