// SPDX-License-Identifier: GPL-3.0-or-later
/*
 * Copyright 2024, 2025 Jiamu Sun <barroit@linux.com>
 */

using PugMod;
using UnityEngine;

public class autofish : IMod {

public const string PROG_NAME = "autofish";

private const string BTN_PREFAB = "Assets/autofish/fisher_switch.prefab";

private LoadedMod mod;
private AssetBundle assets;

private GameObject btn_prefab;
private GameObject btn;

private InventoryUI inv;
private int old_size = 0;

private const float x = -7.625f;
private const float y = -5.5625f;

public void EarlyInit()
{
	new upref(PROG_NAME, API.ConfigFilesystem);
}

public void Init()
{
	inv = (InventoryUI)Manager.ui.playerInventoryUI;

	foreach (LoadedMod __mod in API.ModLoader.LoadedMods) {
		if (__mod.Handlers.Contains(this)) {
			mod = __mod;
			break;
		}
	}

	Transform root = inv.transform.GetChild(0);

	assets = mod.AssetBundles[0];
	btn_prefab = assets.LoadAsset<GameObject>(BTN_PREFAB);

	btn = Object.Instantiate(btn_prefab, root, false);
	btn.SetActive(true);
}

public void Shutdown() {}

/*
 * What the fuck does this method do?
 */
public void ModObjectLoaded(Object _) {}

/*
 * OOP people's brains are smooth. They just can't understand short names like
 * 'unloadable'.
 */
public bool CanBeUnloaded()
{
	return true;
}

public void Update()
{
	if (!Manager.ui.isPlayerInventoryShowing)
		return;

	InventoryHandler handler = inv.GetInventoryHandler();
	int new_size = handler.size;

	if (old_size == new_size)
		return;
	old_size = new_size;

	float z = btn.transform.localPosition.z;

	btn.transform.localPosition = new Vector3(x, y, z);
}

} /* class autofish */
