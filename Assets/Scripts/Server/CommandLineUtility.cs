using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommandLineUtility
{
	public static bool HasCommandLineArgument(string name)
	{
		string[] arguments = Environment.GetCommandLineArgs();
		for (int i = 0; i < arguments.Length; ++i)
		{
			if (arguments[i] == name)
				return true;
		}

		return false;
	}

	public static bool GetCommandLineArgument(string name, out string argument)
	{
		string[] arguments = Environment.GetCommandLineArgs();
		for (int i = 0; i < arguments.Length; ++i)
		{
			if (arguments[i] == name && arguments.Length > (i + 1))
			{
				argument = arguments[i + 1];
				return true;
			}
		}

		argument = default;
		return false;
	}

	public static bool GetCommandLineArgument(string name, out int argument)
	{
		string[] arguments = Environment.GetCommandLineArgs();
		for (int i = 0; i < arguments.Length; ++i)
		{
			if (arguments[i] == name && arguments.Length > (i + 1) && int.TryParse(arguments[i + 1], out int parsedArgument) == true)
			{
				argument = parsedArgument;
				return true;
			}
		}

		argument = default;
		return false;
	}

	public static bool GetCommandLineArgument(string name, out ushort argument)
	{
		string[] arguments = Environment.GetCommandLineArgs();
		for (int i = 0; i < arguments.Length; ++i)
		{
			if (arguments[i] == name && arguments.Length > (i + 1) && ushort.TryParse(arguments[i + 1], out ushort parsedArgument) == true)
			{
				argument = parsedArgument;
				return true;
			}
		}

		argument = default;
		return false;
	}
}
