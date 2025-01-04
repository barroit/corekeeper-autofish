// SPDX-License-Identifier: GPL-3.0-or-later
/*
 * Copyright 2024, 2025 Jiamu Sun <barroit@linux.com>
 */

using System;
using Unity.Burst;
using Unity.Entities;
using Unity.NetCode;

using PlayerEquipment;
using PlayerState;

using static CommandInputButtonNames;
using static Unity.Collections.LowLevel.Unsafe.UnsafeUtility;

[Serializable]
struct fisher_conf {
	public bool active;
};

[BurstCompile]
[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
[UpdateInGroup(typeof(RunSimulationSystemGroup), OrderLast = true)]
[UpdateAfter(typeof(SendClientInputSystem))]
public partial class fisher : SystemBase {

public static TickTimer tmr;
public static bool active;

private fisher_conf pref = new fisher_conf {
	active = true,
};

protected override void OnCreate()
{
	uint rate = (uint)NetworkingManager.GetSimulationTickRateForPlatform();

	pref = upref.get("settings", pref);

	tmr = new TickTimer(0.2f, rate);
	active = pref.active;
}

[BurstCompile]
protected override void OnUpdate()
{
	NetworkTime time = SystemAPI.GetSingleton<NetworkTime>();
	NetworkTick tick = time.ServerTick;

	if (!active)
		return;

	foreach (var (__input, __fishing, __slot) in
		 SystemAPI.Query<RefRW<ClientInputData>,
				 RefRO<FishingStateCD>,
				 RefRO<EquipmentSlotCD>>()
			  .WithAll<GhostOwnerIsLocal>()) {
		ClientInput input = As<ClientInputData,
				       ClientInput>(ref __input.ValueRW);
		FishingStateCD fishing = __fishing.ValueRO;
		EquipmentSlotCD slot = __slot.ValueRO;

		if (slot.slotType != EquipmentSlotType.FishingRodSlot)
			continue;

		input.useFishingMiniGame = false;

		if (Manager.ui.isAnyInventoryShowing ||
		    Manager.menu.IsAnyMenuActive()) {
			if (tmr.isRunning)
				tmr.Stop(tick);
			continue;
		}

		if (input.IsButtonSet(SecondInteract_HeldDown))
			goto next;

		if (fisher.tmr.isRunning) {
			if (!tmr.IsTimerElapsed(tick)) {
				input.SetButton(SecondInteract_HeldDown, true);
				goto next;
			}

			tmr.Stop(tick);
		}

		if (fishing.fishIsNibbling &&!fishing.isFishingAtOctopusBoss)
			tmr.Start(tick);

next:
		__input.ValueRW = As<ClientInput, ClientInputData>(ref input);
	}
}

protected override void OnDestroy()
{
	pref.active = active;
	upref.set("settings", pref);
}

} /* partial class fisher */
