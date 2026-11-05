using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Xml;

internal static class Helper
{
	public static void Log(string s)
	{
		if (Helper.bLogEnable)
		{
			if (Helper.logStreamWriter == null)
			{
				Helper.logStreamWriter = new StreamWriter(".\\Log_" + Helper.now.ToString("yyyyMMdd_HHmmss") + ".log", true);
			}
			Helper.logStreamWriter.Write(s);
			Helper.logStreamWriter.Write("\n");
			Helper.logStreamWriter.Flush();
		}
	}

	public static void Log(string format, params object[] args)
	{
		if (Helper.bLogEnable)
		{
			Helper.Log(string.Format(format, args));
		}
	}

	public static bool StringToBool(string s, bool defaultValue)
	{
		bool result;
		bool flag;
		float num;
		int num2;
		if (s == null)
		{
			result = defaultValue;
		}
		else if (bool.TryParse(s, out flag))
		{
			result = flag;
		}
		else if (float.TryParse(s, out num))
		{
			result = (num > 0.5f);
		}
		else if (int.TryParse(s, out num2))
		{
			result = (num2 > 0);
		}
		else
		{
			result = defaultValue;
		}
		return result;
	}

	public static int StringToInt(string s, int defaultValue)
	{
		int result = 0;
		if (s == null || !int.TryParse(s, out result))
		{
			result = defaultValue;
		}
		return result;
	}

	public static float StringToFloat(string s, float defaultValue)
	{
		float result = 0f;
		if (s == null || !float.TryParse(s, out result))
		{
			result = defaultValue;
		}
		return result;
	}

	public static XmlDocument LoadXmlDocument(string xmlFilePath)
	{
		XmlDocument xmlDocument = new XmlDocument();
		try
		{
			if (File.Exists(xmlFilePath))
			{
				xmlDocument.Load(xmlFilePath);
			}
		}
		catch (Exception ex)
		{
			Helper.ShowException(ex);
		}
		return xmlDocument;
	}

	public static TEnum ToEnum<TEnum>(this string strEnumValue, TEnum defaultValue)
	{
		TEnum result;
		if (!Enum.IsDefined(typeof(TEnum), strEnumValue))
		{
			result = defaultValue;
		}
		else
		{
			result = (TEnum)((object)Enum.Parse(typeof(TEnum), strEnumValue));
		}
		return result;
	}

	public static FieldInfo GetFieldInfo(Type type, string fieldName)
	{
		return type.GetField(fieldName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
	}

	public static object GetInstanceField(Type type, object instance, string fieldName)
	{
		FieldInfo fieldInfo = Helper.GetFieldInfo(type, fieldName);
		if (fieldInfo != null)
		{
			return fieldInfo.GetValue(instance);
		}
		return null;
	}

	public static void SetInstanceField(Type type, object instance, string fieldName, object val)
	{
		FieldInfo fieldInfo = Helper.GetFieldInfo(type, fieldName);
		if (fieldInfo != null)
		{
			fieldInfo.SetValue(instance, val);
		}
	}

	public static void ShowStackFrames(StackFrame[] stackFrames)
	{
		foreach (StackFrame stackFrame in stackFrames)
		{
			Console.WriteLine("{0}({1}.{2}) : {3}.{4}", new object[]
			{
				stackFrame.GetFileName(),
				stackFrame.GetFileLineNumber(),
				stackFrame.GetFileColumnNumber(),
				stackFrame.GetMethod().DeclaringType,
				stackFrame.GetMethod()
			});
		}
	}

	public static void ShowException(Exception ex)
	{
		Console.WriteLine("{0}", ex.Message);
		Helper.ShowStackFrames(new StackTrace(ex, true).GetFrames());
	}

	public static Assembly GetCurrentAssembly()
	{
		return Assembly.GetExecutingAssembly();
	}

	public static FileVersionInfo GetCurrentAssemblyFileVersionInfo()
	{
		return FileVersionInfo.GetVersionInfo(Helper.GetCurrentAssembly().Location);
	}

	private static StreamWriter logStreamWriter = null;

	public static readonly DateTime now = DateTime.Now;

	public static bool bLogEnable = true;
}
