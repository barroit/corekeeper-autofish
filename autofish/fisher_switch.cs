// SPDX-License-Identifier: GPL-3.0-or-later
/*
 * Copyright 2024, 2025 Jiamu Sun <barroit@linux.com>
 */

using I2.Loc;
using Unity.Entities;
using UnityEngine;

public class fisher_switch : ButtonUIElement {

public SpriteRenderer rod;

public LocalizedString btn_on_msg;
public LocalizedString btn_off_msg;

/*
 * rod_disable rgba(108, 91, 74, 1)
 * rod_enable  rgba(255, 255, 255, 1)
 */
private readonly Color rod_disable = new Color(0.4235f, 0.3568f, 0.2901f, 1f);
private readonly Color rod_enable = new Color(1f, 1f, 1f, 1f);

public void toggle_state()
{
	EntityManager manager = fisher.manager;
	Entity entity = fisher.entity;
	fisher_data data = manager.GetComponentData<fisher_data>(entity);

	data.active = !data.active;
	manager.SetComponentData(entity, data);
}

protected override void LateUpdate()
{
	EntityManager manager = fisher.manager;
	Entity entity = fisher.entity;
	fisher_data data = manager.GetComponentData<fisher_data>(entity);

	base.LateUpdate();

	if (data.active) {
		rod.color = rod_enable;
		optionalHoverDesc = btn_off_msg;
	} else {
		rod.color = rod_disable;
		optionalHoverDesc = btn_on_msg;
	}
}

} /* class fisher_switch */
