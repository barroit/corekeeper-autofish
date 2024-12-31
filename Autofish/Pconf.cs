// SPDX-License-Identifier: ${license}
/*
 * Copyright 2024 Jiamu Sun <barroit@linux.com>
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

public static string mod;

public static T get<T>(string name)
{
	IConfigFilesystem fs = API.ConfigFilesystem;
	string file = $"{mod}/{name}.json";

	if (!fs.FileExists(file))
		return default;

	byte[] data = fs.Read(file);
	string json = Encoding.UTF8.GetString(data);

	return JsonUtility.FromJson<T>(json);
}

public static T get<T>(string name, in T defval) where T : struct
{
	T ret = get<T>(name);

	if (!default(T).Equals(ret))
		return ret;

	set(name, in defval);
	return defval;
}

public static void set<T>(string name, in T value)
{
	IConfigFilesystem fs = API.ConfigFilesystem;
	string file = $"{mod}/{name}.json";

	if (!fs.DirectoryExists(mod))
		fs.CreateDirectory(mod);

	string json = JsonUtility.ToJson(value, true);
	byte[] data = Encoding.UTF8.GetBytes(json);

	fs.Write(file, data);
}

} /* class Pconf */
