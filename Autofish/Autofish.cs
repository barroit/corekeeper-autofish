// SPDX-License-Identifier: GPL-3.0-or-later
/*
 * Copyright 2024, 2025 Jiamu Sun <barroit@linux.com>
 */

using PugMod;
using UnityEngine;

public class Autofish : IMod {

public static readonly string NAME = "autofish";

private readonly string BTN_PREFAB = "Assets/Autofish/fisher_switch.prefab";

private LoadedMod autofish;
private AssetBundle assets;

private GameObject btn_prefab;
private GameObject btn;

private InventoryUI inv;
private int old_size = 0;

public void EarlyInit()
{
	new Pconf(NAME, API.ConfigFilesystem);
}

public void Init()
{
	inv = (InventoryUI)Manager.ui.playerInventoryUI;

	foreach (LoadedMod mod in API.ModLoader.LoadedMods) {
		if (mod.Handlers.Contains(this)) {
			autofish = mod;
			break;
		}
	}

	Transform root = inv.transform.GetChild(0);

	assets = autofish.AssetBundles[0];
	btn_prefab = assets.LoadAsset<GameObject>(BTN_PREFAB);

	btn = Object.Instantiate(btn_prefab, root, false);
	btn.SetActive(true);
}

public void Shutdown() {}

/*
 * What the fuck does this method do?
 */
public void ModObjectLoaded(Object _) {}

public bool CanBeUnloaded()
{
	return true;
}

private float side_offsetof(int size, float spread)
{
	return (0f - (size - 1) / 2f) * spread;
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

	int col = handler.columns;
	int row = Mathf.CeilToInt((float)new_size / col);

	float col_off = side_offsetof(col, inv.spread);
	float row_off = side_offsetof(row, inv.spread);

	float x = 0f - col_off + 1.5f;
	float y = 0f - row_off - 1.5f - 1.375f;
	float z = btn.transform.localPosition.z;

	btn.transform.localPosition = new Vector3(x, y, z);
}

} /* class Autofish */
