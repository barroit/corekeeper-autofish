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
using System.Text.Json;

using PugMod;

/*
 * FIXME: Maybe we should stick to lowercase + underscore.
 */

[Serializable]
struct Filedata<T> {
	public string name;
	public T value;
}

public class Pconf {

private static Pconf pconf;
private static JsonSerializerOptions json_conf = new JsonSerializerOptions {
	IncludeFields = true,
};

private string mod;
private IConfigFilesystem fs;

public Pconf(string mod, IConfigFilesystem fs)
{
	this.mod = mod;
	this.fs = fs;
	pconf = this;
}

private bool __get<T>(string name, out T __data) where T : struct
{
	string file = $"{mod}/{name}.json";

	__data = default;
	if (!fs.FileExists(file))
		return false;

	byte[] json = fs.Read(file);
	Filedata<T> data;

	try {
		data = JsonSerializer.Deserialize<Filedata<T>>(json,
							       json_conf);
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
	pconf.__get(name, out T ret);
	return ret;
}

public static T get<T>(string name, in T defval) where T : struct
{
	bool pass = pconf.__get(name, out T ret);

	if (pass)
		return ret;

	pconf.__set(name, in defval);
	return defval;
}

private void __set<T>(string name, in T __data) where T : struct
{
	string file = $"{mod}/{name}.json";

	if (!fs.DirectoryExists(mod))
		fs.CreateDirectory(mod);

	Filedata<T> data = new Filedata<T> {
		name = name,
		value = __data,
	};
	byte[] json = JsonSerializer.SerializeToUtf8Bytes(data, json_conf);

	fs.Write(file, json);
}

public static void set<T>(string name, in T value) where T : struct
{
	pconf.__set(name, value);
}

} /* class Pconf */
