// SPDX-License-Identifier: ${license}
/*
 * Copyright 2024, 2025 Jiamu Sun <barroit@linux.com>
 */

/*
 * I re-implemented the configuration system because the API design sucks so
 * hard. The developer of API.Config must have been kicked in the head by a
 * mule, dragged through a cactus patch, and then patted on the back by someone
 * equally clueless.
 */

using System.Text;
using UnityEngine;

using PugMod;

public class Pconf
{

private static Pconf pconf;

private string mod;
private IConfigFilesystem fs;

public Pconf(string mod, IConfigFilesystem fs)
{
	this.mod = mod;
	this.fs = fs;
	pconf = this;
}

private T __get<T>(string name) where T : struct
{
	string file = $"{mod}/{name}.json";

	if (!fs.FileExists(file))
		return default;

	byte[] data = fs.Read(file);
	string json = Encoding.UTF8.GetString(data);

	return JsonUtility.FromJson<T>(json);
}

public static T get<T>(string name) where T : struct
{
	return pconf.__get<T>(name);
}

private T __get<T>(string name, in T defval) where T : struct
{
	T ret = get<T>(name);

	if (!default(T).Equals(ret))
		return ret;

	set(name, in defval);
	return defval;
}

public static T get<T>(string name, in T defval) where T : struct
{
	return pconf.__get(name, defval);
}

private void __set<T>(string name, in T value) where T : struct
{
	string file = $"{mod}/{name}.json";

	if (!fs.DirectoryExists(mod))
		fs.CreateDirectory(mod);

	string json = JsonUtility.ToJson(value, true);
	byte[] data = Encoding.UTF8.GetBytes(json);

	fs.Write(file, data);
}

public static void set<T>(string name, in T value) where T : struct
{
	pconf.__set(name, value);
}

} /* class Pconf */
