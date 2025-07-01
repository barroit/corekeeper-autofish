// SPDX-License-Identifier: GPL-3.0-or-later
/*
 * Copyright 2024, 2025 Jiamu Sun <barroit@linux.com>
 */

/*
 * I re-implemented the configuration system because the API design sucks so
 * hard. The developer of API.Config must have been kicked in the head by a
 * mule, dragged through a cactus patch, and then patted on the back by someone
 * equally clueless.
 */

using System;
using System.Text;
using UnityEngine;

using PugMod;

/*
 * FIXME: Maybe we should stick to lowercase + underscore.
 */

[Serializable]
struct pref_data<T> {
	public string name;
	public T value;
}

public class upref {

private static upref __this;

private string mod;
private IConfigFilesystem fs;

public upref(string mod, IConfigFilesystem fs)
{
	this.mod = mod;
	this.fs = fs;
	__this = this;
}

private bool __get<T>(string name, out T __data) where T : struct
{
	string file = $"{mod}/{name}.json";

	__data = default;
	if (!fs.FileExists(file))
		return false;

	byte[] raw = fs.Read(file);
	string json = Encoding.UTF8.GetString(raw);
	pref_data<T> data = new pref_data<T>();

	try {
		JsonUtility.FromJsonOverwrite(json, data);
	} catch (Exception) {
		return false;
	}

	if (data.name != name)
		return false;

	__data = data.value;
	return true;
}

public static T get<T>(string name) where T : struct
{
	__this.__get(name, out T ret);
	return ret;
}

public static T get<T>(string name, in T defval) where T : struct
{
	bool pass = __this.__get(name, out T ret);

	if (pass)
		return ret;

	__this.__set(name, in defval);
	return defval;
}

private void __set<T>(string name, in T __data) where T : struct
{
	string file = $"{mod}/{name}.json";

	if (!fs.DirectoryExists(mod))
		fs.CreateDirectory(mod);

	pref_data<T> data = new pref_data<T> {
		name = name,
		value = __data,
	};
	string json = JsonUtility.ToJson(data, prettyPrint: true);
	byte[] raw = Encoding.UTF8.GetBytes(json);

	fs.Write(file, raw);
}

public static void set<T>(string name, in T value) where T : struct
{
	__this.__set(name, value);
}

} /* class Pconf */
