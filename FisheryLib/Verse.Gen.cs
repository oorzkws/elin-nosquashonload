// Copyright (c) 2022 bradson
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v.2.0.If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.
extern alias UnityCore;
using System.Collections;
using static UnityCore::UnityEngine.Debug;

namespace Verse;

public static class Gen {
	public static string ToStringSafe<T>(this T obj)
	{
		if (obj == null)
		{
			return "null";
		}
		string text;
		try
		{
			text = obj.ToString();
		}
		catch (Exception ex)
		{
			int num = 0;
			bool flag = false;
			try
			{
				num = obj.GetHashCode();
				flag = true;
			}
			catch
			{
			}
			if (flag)
			{
				LogError("Exception in ToString(): " + ex + $" {num ^ 1857461521}");
			}
			else
			{
				LogError("Exception in ToString(): " + ex);
			}
			text = "error";
		}
		return text;
	}

	public static string ToStringSafeEnumerable(this IEnumerable enumerable)
	{
		if (enumerable == null)
		{
			return "null";
		}
		string text2;
		try
		{
			string text = "";
			foreach (object obj in enumerable)
			{
				if (text.Length > 0)
				{
					text += ", ";
				}
				text += obj.ToStringSafe<object>();
			}
			text2 = text;
		}
		catch (Exception ex)
		{
			int num = 0;
			bool flag = false;
			try
			{
				num = enumerable.GetHashCode();
				flag = true;
			}
			catch
			{
			}
			if (flag)
			{
				LogError("Exception while enumerating: " + ex + $" {num ^ 581736153}");
			}
			else
			{
				LogError("Exception while enumerating: " + ex);
			}
			text2 = "error";
		}
		return text2;
	}
}