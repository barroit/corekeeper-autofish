// SPDX-License-Identifier: GPL-3.0-or-later
/*
 * Copyright 2024 Jiamu Sun <barroit@linux.com>
 */

using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;

using PlayerEquipment;
using PlayerState;

using static CommandInputButtonNames;
using static Unity.Collections.LowLevel.Unsafe.UnsafeUtility;

public struct FisherCD : IComponentData
{
	public TickTimer clicking;
	public bool pulled;
	public bool enabled;
}

[Serializable]
public struct Preference
{

public bool enabled;

} /* struct Preference */

[BurstCompile]
[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
[UpdateInGroup(typeof(RunSimulationSystemGroup), OrderLast = true)]
[UpdateAfter(typeof(SendClientInputSystem))]
public partial class Fisher : SystemBase
{

public static Entity entity;
public static EntityManager manager;

private Preference pref = new Preference {
	enabled = true,
};

protected override void OnCreate()
{
	pref = Pconf.get("settings", pref);
	manager = EntityManager;
	entity = manager.CreateSingleton<FisherCD>();

	uint rate = (uint)NetworkingManager.GetSimulationTickRateForPlatform();
	FisherCD fisher = new FisherCD {
		clicking = new TickTimer(0.1f, rate),
		pulled   = false,
		enabled  = pref.enabled,
	};

	manager.SetComponentData(entity, fisher);
}

[BurstCompile]
protected override void OnUpdate()
{
	NetworkTime time = SystemAPI.GetSingleton<NetworkTime>();
	NetworkTick tick = time.ServerTick;

	RefRW<FisherCD> __fisher = SystemAPI.GetSingletonRW<FisherCD>();
	FisherCD fisher = __fisher.ValueRW;

	if (!fisher.enabled)
		return;

	foreach (var (__input, __fishing, __player, __slot) in
		 SystemAPI.Query<RefRW<ClientInputData>,
				 RefRO<FishingStateCD>,
				 RefRO<PlayerStateCD>,
				 RefRO<EquipmentSlotCD>>()
			  .WithAll<GhostOwnerIsLocal>()) {
		ClientInput input = As<ClientInputData,
				       ClientInput>(ref __input.ValueRW);
		FishingStateCD fishing = __fishing.ValueRO;
		PlayerStateCD player = __player.ValueRO;
		EquipmentSlotCD slot = __slot.ValueRO;

		if (slot.slotType != EquipmentSlotType.FishingRodSlot)
			continue;

		input.useFishingMiniGame = false;

		if (Manager.ui.isAnyInventoryShowing ||
		    Manager.menu.IsAnyMenuActive()) {
			fisher.pulled = false;
			if (fisher.clicking.isRunning)
				fisher.clicking.Stop(tick);
			continue;
		}

		if (input.IsButtonSet(SecondInteract_HeldDown)) {
			fisher.pulled = false;
			goto next;
		}

		if (fisher.clicking.isRunning) {
			if (!fisher.clicking.IsTimerElapsed(tick)) {
				input.SetButton(SecondInteract_HeldDown, true);
				goto next;
			}

			fisher.clicking.Stop(tick);
		}

		if (fisher.pulled &&
		    player.currentState != PlayerStateEnum.Fishing) {
			fisher.clicking.Start(tick);
			fisher.pulled = false;
		} else if (fishing.fishIsNibbling &&
			   !fishing.isFishingAtOctopusBoss) {
			fisher.clicking.Start(tick);
			fisher.pulled = true;
		}

next:
		__input.ValueRW = As<ClientInput, ClientInputData>(ref input);
	}

	__fisher.ValueRW = fisher;
}

protected override void OnDestroy()
{
	FisherCD fisher = SystemAPI.GetSingleton<FisherCD>();

	pref.enabled = fisher.enabled;
	Pconf.set("settings", pref);
}

} /* partial class Fisher */
