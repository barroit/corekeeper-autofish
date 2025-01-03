// SPDX-License-Identifier: GPL-3.0-or-later
/*
 * Copyright 2024, 2025 Jiamu Sun <barroit@linux.com>
 */

using I2.Loc;
using Unity.Entities;
using UnityEngine;

public class FisherSwitch : ButtonUIElement {

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
	EntityManager manager = Fisher.manager;
	Entity entity = Fisher.entity;
	FisherCD fisher = manager.GetComponentData<FisherCD>(Fisher.entity);

	fisher.active = !fisher.active;
	manager.SetComponentData(entity, fisher);
}

protected override void LateUpdate()
{
	EntityManager manager = Fisher.manager;
	Entity entity = Fisher.entity;
	FisherCD fisher = manager.GetComponentData<FisherCD>(entity);

	base.LateUpdate();

	if (fisher.active) {
		rod.color = rod_enable;
		optionalHoverDesc = btn_off_msg;
	} else {
		rod.color = rod_disable;
		optionalHoverDesc = btn_on_msg;
	}
}

} /* class FisherSwitch */
